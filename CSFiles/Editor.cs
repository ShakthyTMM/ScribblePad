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
   public bool IsDrawing { get; set; }


   public bool IsModified { get => mIsModified; set => mIsModified = value; }
   bool mIsModified;

   public bool IsSaved { get => mIsSaved; set => mIsSaved = value; }
   public bool IsCancelled { get => mIsCancelled; set => mIsCancelled = value; }

   bool mIsSaved, mIsCancelled;

   public Stack<Pline> Stack => mStack;
   Stack<Pline> mStack = new ();

   public bool CanExecute_Undo => mDrawing.Plines.Count > 0 && IsModified && mUndo;
   bool mUndo = true;

   public bool CanExecute_Redo => Stack.Count > 0 && mRedo;
   bool mRedo = true;

   public DockPanel? Dockpanel => mDockPanel;
   DockPanel? mDockPanel;

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
      foreach (var shape in mDrawing.Plines) {
         ds.Pen = mPen;
         shape.Draw (ds);
      }
      ds.Pen = mFBPen;
      mCurrentWidget?.Draw ();
   }

   public void ClearScreen () {
      mDrawing.Plines.Clear ();
      IsDrawing = false;
      mIsSaved = false; mIsModified = false;
      InvalidateVisual ();
   }

   public void Undo () {
      if (mDrawing.Plines.Count > 0) {
         mRedo = true;
         mStack.Push (mDrawing.Plines.Last ());
         mDrawing.Plines.Remove (mDrawing.Plines.Last ());
         mCount = mDrawing.Plines.Count;
         if (mCount == 0) mUndo = false;
         InvalidateVisual ();
      }
   }

   public void Redo () {
      if (mStack.Count > 0) {
         mDrawing.Plines.Add (mStack.Pop ());
         mCount = mDrawing.Plines.Count;
         if (mCount > 0) mUndo = true;
         if (mStack.Count == 0) mRedo = false;
         InvalidateVisual ();
      }
   }

   public void EnableUndoRedo () {
      if (mCount != mDrawing.Plines.Count) { Stack.Clear (); mRedo = false; }
      mUndo = true; mRedo = true;
   }

   #endregion

   #region Status Property ------------------------------------------------------------------
   public string Status {
      get => mStatus;
      set {
         if (mStatus != value) {
            mStatus = value;
            mDockPanel = Parent as DockPanel;
            if(mDockPanel!=null) mPrompt = mDockPanel.FindName ("mPrompt") as TextBlock;
            if (mPrompt != null) { mPrompt.Text = mStatus; mPrompt.FontWeight = FontWeights.DemiBold; mPrompt.FontSize = 12; }
         }
      }
   }
   #endregion

   #region Private Data ---------------------------------------------------------------------
   public Drawing mDrawing = new ();
   public Widget? mCurrentWidget;
   Pen mFBPen = new (Brushes.Red, 1);
   Pen mPen = new (Brushes.Black, 1);
   TextBlock? mPrompt;
   string mStatus = "Pick a tool";
   int mCount;
   #endregion
}
#endregion

#region Class Drawing -------------------------------------------------------------------
public class Drawing {
   #region Method ----------------------------------------------------------------------
   public void Add (Pline pline) {
      Plines.Add (pline);
      Bound = new Bound (Plines.Select (pline => pline.Bound));
   }
   #endregion

   #region Properties and fields -------------------------------------------------------
   public Bound Bound { get; private set; }
   public List<Pline> Plines { get => mPlines; }
   List<Pline> mPlines = new ();
   #endregion
}
#endregion





