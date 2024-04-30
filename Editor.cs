using Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CAD;

#region Class Editor ---------------------------------------------------------------------
public class Editor : Canvas {
   #region Constructor ----------------------------------------------------------------------
   public Editor () { }
   #endregion

   #region Properties -----------------------------------------------------------------------
   public List<Shape> Shapes { get => mShapes; }
   List<Shape> mShapes = new ();

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

   public bool CanExecute_Undo => Shapes.Count > 0 && IsModified && mUndo;
   bool mUndo = true;

   public bool CanExecute_Redo => Stack.Count > 0 && mRedo;
   bool mRedo = true;

   public DockPanel Dockpanel => mDockPanel;
   DockPanel mDockPanel;
   #endregion

   #region Methods --------------------------------------------------------------------------
   protected override void OnRender (DrawingContext dc) {
      var ds = DrawingShapes.GetInstance;
      ds.DrawingContext = dc;
      base.OnRender (dc);
      foreach (var shape in mShapes) {
         ds.Pen = pen2;
         shape.Draw (ds);
      }
      if (mCurrentShape != null && mIsDrawing) {
         ds.Pen = pen1;
         mCurrentShape.Draw (ds);
      }
   }

   public void ClearScreen () {
      mShapes.Clear ();
      mCurrentShape = null; mIsDrawing = false;
      mIsSaved = false; mIsModified = false;
      InvalidateVisual ();
   }

   public void AddShapes () {
      mShapes.Add (mCurrentShape);
      if (mCount != Shapes.Count) { Stack.Clear (); mRedo = false; }
      mUndo = true; mRedo = true;
      mCurrentShape = null;
      mIsDrawing = false;
      InvalidateVisual ();
   }

   public void Undo () {
      if (Shapes.Count > 0) {
         mStack.Push (Shapes.Last ());
         Shapes.Remove (Shapes.Last ());
         mCount = Shapes.Count;
         if (mCount == 0) mUndo = false;
         InvalidateVisual ();
      }
   }

   public void Redo () {
      if (mStack.Count > 0) {
         Shapes.Add (mStack.Pop ());
         mCount = Shapes.Count;
         if (mCount > 0) mUndo = true;
         if (mStack.Count == 0) mRedo = false;
         InvalidateVisual ();
      }
   }
   #endregion

   #region Property -------------------------------------------------------------------------
   public string Status {
      get => mStatus;
      set {
         if (mStatus != value) {
            mStatus = value;
            mDockPanel = Parent as DockPanel;
            mPrompt = mDockPanel.FindName ("mPrompt") as TextBlock;
            mPrompt.Text = value; mPrompt.FontWeight = FontWeights.DemiBold; mPrompt.FontSize = 12;
         }
      }
   }
   #endregion

   #region Private Data ---------------------------------------------------------------------
   Pen pen1 = new (Brushes.Red, 1);
   Pen pen2 = new (Brushes.Black, 1);
   TextBlock mPrompt;
   string mStatus = "Pick a tool";
   int mCount;
   #endregion
}
#endregion


