// ----------------------------------------------------------------------------------------
// Training 
// Copyright (c) Metamation India.
// ----------------------------------------------------------------------------------------
// MainWindow.xaml.cs
// Scribble Pad
// To design scribble pad
// ----------------------------------------------------------------------------------------

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Data;
using Frontend;
using Point = Data.Point;

namespace ScribblePad;
public partial class MainWindow : Window {
   #region Constructor ------------------------------------------------------------
   public MainWindow () {
      InitializeComponent ();
      MouseDown += OnMouseDown;
      MouseMove += OnMouseMove;
      mDocManager = new DocManager (this);
   }
   #endregion

   #region Implementation ---------------------------------------------------------
   #region Tools ------------------------------------------------------------------
   /// <summary>Click event for redo button</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnRedo (object sender, RoutedEventArgs e) {
      if (Events.mStack.Count > 0) {
         Events.ShapesList.Add (Events.mStack.Pop ());
         mCount = Events.ShapesList.Count;
         if (mCount > 0) mUI.Opacity = 1;
         if (Events.mStack.Count == 0) mRI.Opacity = 0.2;
         InvalidateVisual ();
      }
   }

   /// <summary>Click event for undo button</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnUndo (object sender, RoutedEventArgs e) {
      if (Events.ShapesList.Count > 0) {
         Events.mStack.Push (Events.ShapesList.Last ());
         Events.ShapesList.Remove (Events.ShapesList.Last ());
         mCount = Events.ShapesList.Count;
         if (mCount == 0) mUI.Opacity = 0.2;
         InvalidateVisual ();
      }
   }

   /// <summary>Determines whether the redo command is executable</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void CanExecute_Redo (object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = Events.mStack.Count > 0;
      if (e.CanExecute) mRI.Opacity = 1;
   }

   /// <summary>Determines whether the undo command is executable</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void CanExecute_Undo (object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = Events.ShapesList.Count > 0;
      if (e.CanExecute) mUI.Opacity = 1;
   }

   /// <summary>Event handler for shape buttons</summary>
   /// <param name="sender"></param>
   /// <param name="args"></param>
   void OnModeChanged (object sender, RoutedEventArgs args) {
      foreach (var control in mToolBar.Items) {
         if (control is ToggleButton tb && sender is ToggleButton clicked_tb) {
            if (tb == clicked_tb) {
               tb.IsChecked = true;
               switch (tb.ToolTip) {
                  case "Line": Events.Shape = Events.EShapes.Line; break;
                  case "Rectangle": Events.Shape = Events.EShapes.Rectangle; break;
                  case "Circle": Events.Shape = Events.EShapes.Circle; break;
               }
            } else tb.IsChecked = false;
         }
      }
   }

   /// <summary>Event handler for escape key press</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnKeyDown (object sender, KeyEventArgs e) {
      if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.D4) OnExit (sender, e);
      if (Keyboard.Modifiers == ModifierKeys.Control) {
         switch (e.Key) {
            case Key.N: OnNew (sender, e); break;
            case Key.O: OnOpen (sender, e); break;
            case Key.S: OnSave (sender, e); break;
            case Key.Z: OnUndo(sender, e); break;
            case Key.Y: OnRedo (sender, e); break;
            default: break;
         }
      }
   }
   #endregion

   #region Mouse Events -----------------------------------------------------------
   /// <summary>Event handler for mouse down event</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnMouseDown (object sender, MouseButtonEventArgs e) {
      if (e.RightButton == MouseButtonState.Released) {
         if (mIsDrawing) {
            mEndPoint.X = e.GetPosition (this).X; mEndPoint.Y = e.GetPosition (this).Y;
            Events.MouseMove (mEndPoint);
            AddShapes ();
         } else if (Events.Shape != Events.EShapes.None) {
            mStartPoint.X = e.GetPosition (this).X; mStartPoint.Y = e.GetPosition (this).Y;
            Events.MouseDown (mStartPoint);
            mIsDrawing = true;
         }
      }
   }

   // Routine to add shapes to the list
   void AddShapes () {
      Events.ShapesList.Add (Events.CurrentShape);
      if (mCount != Events.ShapesList.Count) { Events.mStack.Clear (); mRI.Opacity = 0.2; }
      mUI.Opacity = 1;
      Events.CurrentShape = null;
      mIsDrawing = false;
      InvalidateVisual ();
   }

   /// <summary>Event handler for mouse move event</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnMouseMove (object sender, MouseEventArgs e) {
      if (mIsDrawing) {
         mEndPoint.X = e.GetPosition (this).X; mEndPoint.Y = e.GetPosition (this).Y;
         Events.MouseMove (mEndPoint);
         InvalidateVisual ();
      }
   }
   #endregion

   #region Menu Items -------------------------------------------------------------
   /// <summary>Click event for exit menu item</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnExit (object sender, RoutedEventArgs e) => mDocManager.Exit (sender, e);

   /// <summary>Click event of new menu item</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnNew (object sender, RoutedEventArgs e) => mDocManager.New (sender, e);

   /// <summary>Click event of open menu item</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnOpen (object sender, RoutedEventArgs e) => mDocManager.Load (sender, e);

   /// <summary>Click event for save menu item</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnSave (object sender, RoutedEventArgs e) => mDocManager.SaveAs (null);

   /// <summary>Click event for save as menu item</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnSaveAs (object sender, RoutedEventArgs e) => mDocManager.SaveAs (sender);
   #endregion

   /// <summary>Overriding on-closing method of main window</summary>
   /// <param name="e"></param>
   protected override void OnClosing (CancelEventArgs e) {
      if (mDocManager.IsModified) {
         mDocManager.Prompt (null, null);
         if (!mDocManager.IsCancelled) base.OnClosing (e); else e.Cancel = true;
      } else base.OnClosing (e);
   }

   /// <summary>Overriding on-render method</summary>
   /// <param name="dc"></param>
   protected override void OnRender (DrawingContext dc) {
      base.OnRender (dc);
      foreach (var shape in Events.ShapesList) DrawingShapes.Draw (dc, shape);
      if (Events.CurrentShape != null && mIsDrawing) DrawingShapes.Draw (dc, Events.CurrentShape);
   }
   #endregion

   #region Properties -------------------------------------------------------------
   public Point mStartPoint, mEndPoint;    // Holds the points captured by mouse events
   #endregion

   #region Private data -----------------------------------------------------------
   DocManager mDocManager;
   bool mIsDrawing = false;    // Holds the drawing state
   int mCount;    // Holds count of the shapes after undo operation
   #endregion
}

/// <summary>Implements RoutedCommand class from ICommand interface</summary>
public static class Commands {
   public static readonly RoutedCommand Undo = new ();
   public static readonly RoutedCommand Redo = new ();
}
