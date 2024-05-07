using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace CAD;

#region Class Editor ---------------------------------------------------------------------
public class Editor : Canvas {
   #region Constructor ----------------------------------------------------------------------
   public Editor () { }
   #endregion

   #region Properties -----------------------------------------------------------------------
   public bool IsDrawing { get => mIsDrawing; set => mIsDrawing = value; }
   bool mIsDrawing;

   public Shape CurrentShape { get => mCurrentShape; set => mCurrentShape = value; }
   Shape mCurrentShape;

   public bool IsModified { get => mIsModified; set => mIsModified = value; }
   bool mIsModified;

   public bool IsSaved { get => mIsSaved; set => mIsSaved = value; }
   public bool IsCancelled { get => mIsCancelled; set => mIsCancelled = value; }

   bool mIsSaved, mIsCancelled;

   public Stack<Shape> Stack => mStack;
   Stack<Shape> mStack = new ();

   public bool CanExecute_Undo => mDrawing.Shapes.Count > 0 && IsModified && mUndo;
   bool mUndo = true;

   public bool CanExecute_Redo => Stack.Count > 0 && mRedo;
   bool mRedo = true;

   public DockPanel Dockpanel => mDockPanel;
   DockPanel mDockPanel;

   public Drawing Drawing => mDrawing;

   public Matrix mProjXfm, mInvProjXfm = Matrix.Identity;

   public Matrix Xfm;
   #endregion

   #region Methods --------------------------------------------------------------------------
   protected override void OnMouseRightButtonDown (MouseButtonEventArgs e) {
      mProjXfm = Util.GetComputedMatrix (ActualWidth, ActualHeight, mDrawing.Bound);
      mInvProjXfm = mProjXfm; mInvProjXfm.Invert ();
      Xfm = mProjXfm;
      InvalidateVisual ();
   }

   protected override void OnMouseWheel (MouseWheelEventArgs e) {
      double zoomFactor = 1.05;
      if (e.Delta > 0) zoomFactor = 1 / zoomFactor;
      var ptDraw = mInvProjXfm.Transform (e.GetPosition (this)); // mouse point in drawing space
                                                                 // Actual visible drawing area
      Point cornerA = mInvProjXfm.Transform (new Point ()), cornerB = mInvProjXfm.Transform (new Point (ActualWidth, ActualHeight));
      Data.Point a = new (cornerA.X, cornerA.Y);
      Data.Point b = new (cornerB.X, cornerB.Y);
      var b1 = new Bound (a, b);
      Data.Point c = new (ptDraw.X, ptDraw.Y);
      b1 = b1.Inflated (c, zoomFactor);
      mProjXfm = Util.GetComputedMatrix (ActualWidth, ActualHeight, b1);
      mInvProjXfm = mProjXfm; mInvProjXfm.Invert ();
      Xfm = mProjXfm;
      InvalidateVisual ();
   }

   protected override void OnRender (DrawingContext dc) {
      var ds = DrawingShapes.GetInstance;
      ds.DrawingContext = dc;
      ds.Xfm = Xfm;
      base.OnRender (dc);
      foreach (var shape in mDrawing.Shapes) {
         ds.Pen = mPen;
         shape.Draw (ds);
      }
      if (mCurrentShape != null && mIsDrawing) {
         ds.Pen = mFBPen;
         mCurrentShape.Draw (ds);
      }
   }

   public void ClearScreen () {
      mDrawing.Shapes.Clear ();
      mCurrentShape = null; mIsDrawing = false;
      mIsSaved = false; mIsModified = false;
      InvalidateVisual ();
   }

   public void AddShapes () {
      mCurrentShape.Bound2 = new Bound (mCurrentShape.StartPoint, mCurrentShape.EndPoint);
      mDrawing.Add (mCurrentShape);
      if (mCount != mDrawing.Shapes.Count) { Stack.Clear (); mRedo = false; }
      mUndo = true; mRedo = true;
      mCurrentShape = null;
      mIsDrawing = false;
      InvalidateVisual ();
   }

   public void Undo () {
      if (mDrawing.Shapes.Count > 0) {
         mStack.Push (mDrawing.Shapes.Last ());
         mDrawing.Shapes.Remove (mDrawing.Shapes.Last ());
         mCount = mDrawing.Shapes.Count;
         if (mCount == 0) mUndo = false;
         InvalidateVisual ();
      }
   }

   public void Redo () {
      if (mStack.Count > 0) {
         mDrawing.Shapes.Add (mStack.Pop ());
         mCount = mDrawing.Shapes.Count;
         if (mCount > 0) mUndo = true;
         if (mStack.Count == 0) mRedo = false;
         InvalidateVisual ();
      }
   }
   #endregion

   #region Status Property ------------------------------------------------------------------
   public string Status {
      get => mStatus;
      set {
         if (mStatus != value) {
            mStatus = value;
            mDockPanel = Parent as DockPanel;
            mPrompt = mDockPanel.FindName ("mPrompt") as TextBlock;
            mPrompt.Text = mStatus; mPrompt.FontWeight = FontWeights.DemiBold; mPrompt.FontSize = 12;
         }
      }
   }
   #endregion

   #region Private Data ---------------------------------------------------------------------
   Drawing mDrawing = new ();
   Pen mFBPen = new (Brushes.Red, 1);
   Pen mPen = new (Brushes.Black, 1);
   TextBlock mPrompt;
   string mStatus = "Pick a tool";
   int mCount;
   #endregion
}
#endregion

#region Class Drawing -------------------------------------------------------------------
public class Drawing {
   #region Method ----------------------------------------------------------------------
   public void Add (Shape shape) {
      Shapes.Add (shape);
      Bound = new Bound (Shapes.Select (shape => shape.Bound2));
   }
   #endregion

   #region Properties and fields -------------------------------------------------------
   public Bound Bound { get; private set; }
   public List<Shape> Shapes { get => mShapes; }
   List<Shape> mShapes = new ();
   #endregion
}
#endregion

#region Class PanWidget -----------------------------------------------------------------
class PanWidget { // Works in screen space
   #region Constructors ----------------------------------------------------------------
   public PanWidget (UIElement eventSource, Action<Vector> onPan) {
      mOnPan = onPan;
      eventSource.MouseDown += (sender, e) => {
         if (e.ChangedButton == MouseButton.Middle) PanStart (e.GetPosition (eventSource));
      };
      eventSource.MouseUp += (sender, e) => {
         if (IsPanning) PanEnd (e.GetPosition (eventSource));
      };
      eventSource.MouseMove += (sender, e) => {
         if (IsPanning) PanMove (e.GetPosition (eventSource));
      };
      eventSource.MouseLeave += (sender, e) => {
         if (IsPanning) PanCancel ();
      };
   }
   #endregion

   #region Implementation --------------------------------------------------------------
   bool IsPanning => mPrevPt != null;

   void PanStart (Point pt) {
      mPrevPt = pt;
   }

   void PanMove (Point pt) {
      mOnPan.Invoke (pt - mPrevPt!.Value);
      mPrevPt = pt;
   }

   void PanEnd (Point? pt) {
      if (pt.HasValue)
         PanMove (pt.Value);
      mPrevPt = null;
   }

   void PanCancel () => PanEnd (null);
   #endregion

   #region Private Data ----------------------------------------------------------------
   Point? mPrevPt;
   readonly Action<Vector> mOnPan;
   #endregion
}
#endregion



