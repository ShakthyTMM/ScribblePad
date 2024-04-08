using Data;
using Frontend;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Point = Data.Point;

namespace ScribblePad;
public class Document {
   #region Constructors ------------------------------------------------------------------------
   public Document () { }
   public Document (MainWindow eventSource) {
      mEventSource = eventSource;
      mDocManager = new DocManager (this, eventSource, "Untitled");
      Attach ();
   }
   #endregion

   #region Methods ----------------------------------------------------------------------
   void Attach () {
      mEventSource.MouseDown += OnMouseDown;
      mEventSource.MouseMove += OnMouseMove;
   }

   void Detach () {
      mEventSource.MouseDown -= OnMouseDown;
      mEventSource.MouseMove -= OnMouseMove;
   }

   void OnMouseDown (object sender, MouseButtonEventArgs e) {
      if (e.RightButton == MouseButtonState.Released) {
         if (mIsDrawing) {
            mEndPoint.X = e.GetPosition (mEventSource).X; mEndPoint.Y = e.GetPosition (mEventSource).Y;
            mCurrentShape.Update (mEndPoint);
            AddShapes ();
         } else if (Shape != EShapes.None) {
            mStartPoint.X = e.GetPosition (mEventSource).X; mStartPoint.Y = e.GetPosition (mEventSource).Y;
            if (Shape != EShapes.None) {
               switch (Shape) {
                  case EShapes.Rectangle: mCurrentShape = new Rectangle (mStartPoint); break;
                  case EShapes.Circle: mCurrentShape = new Circle (mStartPoint); break;
                  case EShapes.Line: mCurrentShape = new Line (mStartPoint); break;
               }
            }
            mIsDrawing = true;
         }
      }
   }
   void OnMouseMove (object sender, MouseEventArgs e) {
      if (mIsDrawing) {
         mEndPoint.X = e.GetPosition (mEventSource).X; mEndPoint.Y = e.GetPosition (mEventSource).Y;
         mCurrentShape.Update (mEndPoint);
         mEventSource.InvalidateVisual ();
      }
   }

   void AddShapes () {
      ShapesList.Add (mCurrentShape);
      if (mCount != ShapesList.Count) { Stack.Clear (); mEventSource.mRI.Opacity = 0.2; }
      mEventSource.mUI.Opacity = 1;
      mCurrentShape = null;
      mIsDrawing = false;
      mEventSource.InvalidateVisual ();
   }

   public void GetSelectedShape (object sender) {
      if (mMode == Mode.Select) { selection.Detach (); Attach (); mMode = Mode.Drawing; }
      Check (sender);
      switch (((ToggleButton)sender).ToolTip) {
         case "Line": Shape = EShapes.Line; break;
         case "Rectangle": Shape = EShapes.Rectangle; break;
         case "Circle": Shape = EShapes.Circle; break;
      }
   }

   public void Check (object sender) {
      foreach (var control in mEventSource.mWP.Children)
         if (control is ToggleButton tb && sender is ToggleButton clicked_tb) {
            if (tb == clicked_tb) tb.IsChecked = true;
            else tb.IsChecked = false;
         }
   }

   public void DrawShapes (DrawingContext dc) {
      foreach (var shape in ShapesList) DrawingShapes.Draw (dc, shape, mPen);
      if (mCurrentShape != null && mIsDrawing) DrawingShapes.Draw (dc, mCurrentShape, mPen);
      else if (mEventSource.SelectedShape != null) {
         Pen pen = new (Brushes.Blue, 1);
         pen.DashStyle = new DashStyle (new double[] { 2, 2 }, 0);
         DrawingShapes.Draw (dc, mEventSource.SelectedShape, pen);
      }
   }

   public void Undo () {
      if (ShapesList.Count > 0) {
         Stack.Push (ShapesList.Last ());
         ShapesList.Remove (ShapesList.Last ());
         mCount = ShapesList.Count;
         if (mCount == 0) mEventSource.mUI.Opacity = 0.2;
         mEventSource.InvalidateVisual ();
      }
   }

   public void Redo () {
      if (Stack.Count > 0) {
         ShapesList.Add (Stack.Pop ());
         mCount = ShapesList.Count;
         if (mCount > 0) mEventSource.mUI.Opacity = 1;
         if (Stack.Count == 0) mEventSource.mRI.Opacity = 0.2;
         mEventSource.InvalidateVisual ();
      }
   }

   public void CanExecute_redo (object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = Stack.Count > 0 && mMode == Mode.Drawing;
      if (e.CanExecute) mEventSource.mRI.Opacity = 1;
   }

   public void CanExecute_undo (object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = ShapesList.Count > 0 && mDocManager.IsModified && mMode == Mode.Drawing;
      if (e.CanExecute) mEventSource.mUI.Opacity = 1;
   }

   public void New (object sender, RoutedEventArgs e) => mDocManager.New (sender, e);

   public void Save (object sender, RoutedEventArgs e) => mDocManager.SaveAs (sender);

   public void Load (object sender, RoutedEventArgs e) => mDocManager.Load (sender, e);

   public void Exit (object sender, RoutedEventArgs e) => mDocManager.Exit (sender, e);

   public void Select (object sender, RoutedEventArgs e) {
      Check (sender);
      mMode = Mode.Select;
      Detach ();
      selection = new (this, mEventSource);
   }

   public enum Mode { Drawing, Select }
   public List<Shape> ShapesList = new ();
   public Stack<Shape> Stack = new ();    // Holds the shapes after undo and redo operations
   #endregion

   #region Private Data ------------------------------------------------------------------------
   Selection selection;
   Mode mMode;
   DocManager mDocManager;
   Pen mPen = new (Brushes.White, 2);
   Point mStartPoint, mEndPoint;
   int mCount;
   MainWindow mEventSource;
   Shape mCurrentShape;
   bool mIsDrawing = false;
   enum EShapes { None, Rectangle, Line, Circle }    // None is used to prevent having rectangle as default shape
   EShapes Shape;
   #endregion
}
