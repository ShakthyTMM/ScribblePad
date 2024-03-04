// ----------------------------------------------------------------------------------------
// Training 
// Copyright (c) Metamation India.
// ----------------------------------------------------------------------------------------
// MainWindow.xaml.cs
// Scribble Pad
// To design scribble pad
// ----------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DrawingShapes {
   #region MainWindow ---------------------------------------------------------------------------------------- 
   public partial class MainWindow : Window {
      public MainWindow () {
         InitializeComponent ();
         MouseDown += OnMouseDown;
         MouseMove += OnMouseMove;
         EnableChanged (Tools.ReDo, false);    // Initially sets the 'IsEnabled' property of button as false
         EnableChanged (Tools.UnDo, false);    // and image opacity
      }

      #region Tools ----------------------------------------
      /// <summary>Click event for color change button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnColorChange (object sender, RoutedEventArgs e) {
         ColorDialog dialog = new ();
         if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
            mPen.Brush = new SolidColorBrush (Color.FromArgb (dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B));  // Gets the ARGB bytes from the color selected
      }

      /// <summary>Click event for erase button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnErase (object sender, RoutedEventArgs e) => mSelectedShape = Shapes.Eraser;

      /// <summary>Click event for redo button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnReDo (object sender, RoutedEventArgs e) {
         if (mStack.Count > 0) {
            mShapes.Add (mStack.Pop ());
            mCount = mShapes.Count;
            if (mCount > 0) EnableChanged (Tools.UnDo, true);
            if (mStack.Count == 0) EnableChanged (Tools.ReDo, false);
            InvalidateVisual ();
         }
      }

      /// <summary>Click event for undo button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnUnDo (object sender, RoutedEventArgs e) {
         if (mShapes.Count > 0) {
            mStack.Push (mShapes.Last ());
            EnableChanged (Tools.ReDo, true);
            mShapes.Remove (mShapes.Last ());
            mCount = mShapes.Count;
            if (mCount == 0) EnableChanged (Tools.UnDo, false);
            InvalidateVisual ();
         }
      }

      // Changes the 'IsEnabled' property of undo and redo buttons
      void EnableChanged (Tools tool, bool IsEnabled) {
         if (tool == Tools.ReDo)
            if (IsEnabled) { mRedo.IsEnabled = true; mRI.Opacity = 1; } else { mRedo.IsEnabled = false; mRI.Opacity = 0.2; }
         if (tool == Tools.UnDo)
            if (IsEnabled) { mUndo.IsEnabled = true; mUI.Opacity = 1; } else { mUndo.IsEnabled = false; mUI.Opacity = 0.2; }
      }

      /// <summary>Click event for pen button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnScribble (object sender, RoutedEventArgs e) => mSelectedShape = Shapes.Scribble;

      #endregion

      #region Shapes ----------------------------------------
      /// <summary>Click event for circle button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnCircle (object sender, RoutedEventArgs e) => mSelectedShape = Shapes.Circle;

      void OnConnectedLine (object sender, RoutedEventArgs e) => mSelectedShape = Shapes.ConnectedLine;

      /// <summary>Click event for ellipse button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnEllipse (object sender, RoutedEventArgs e) => mSelectedShape = Shapes.Ellipse;

      /// <summary>Click event for line button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnLine (object sender, RoutedEventArgs e) => mSelectedShape = Shapes.Line;

      /// <summary>Click event for rectangle button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnRect (object sender, RoutedEventArgs e) => mSelectedShape = Shapes.Rectangle;
      #endregion

      #region Mouse Events ----------------------------------------
      /// <summary>Event handler for mouse down event</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnMouseDown (object sender, MouseButtonEventArgs e) {
         if (e.RightButton == MouseButtonState.Released) {
            if (mCurrentShape is ConnectedLine) { UpdateShape (e); mCurrentShape.AddLines (mEndPoint); } else {
               if (IsDrawing) AddToList (e);
               else {
                  mStartPoint = e.GetPosition (this);
                  switch (mSelectedShape) {
                     case Shapes.Rectangle:
                        mCurrentShape = new Rectangle (mStartPoint);
                        break;
                     case Shapes.Ellipse:
                        mCurrentShape = new Ellipse (mStartPoint);
                        break;
                     case Shapes.Circle:
                        mCurrentShape = new Circle (mStartPoint);
                        break;
                     case Shapes.Line:
                        mCurrentShape = new CustomLine (mStartPoint);
                        break;
                     case Shapes.Scribble:
                        mCurrentShape = new Scribble (mStartPoint);
                        break;
                     case Shapes.Eraser:
                        mCurrentShape = new Eraser (mStartPoint);
                        break;
                     case Shapes.ConnectedLine:
                        mCurrentShape = new ConnectedLine (mStartPoint);
                        break;
                     default: break;
                  }
                  IsDrawing = true;
               }
            }
         }
      }

      // Routine to get end point and update the shape
      void UpdateShape (MouseEventArgs e) {
         mEndPoint = e.GetPosition (this);
         mCurrentShape.Update (mEndPoint);
      }

      // Adds the shapes to the list
      void AddToList (MouseEventArgs e) { UpdateShape (e); AddShapes (); }

      /// <summary>Event handler for mouse move event</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnMouseMove (object sender, MouseEventArgs e) {
         if (IsDrawing) { UpdateShape (e); InvalidateVisual (); }
      }

      #endregion

      #region Menu Items ----------------------------------------
      /// <summary>Click event for exit menu item</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnExit (object sender, RoutedEventArgs e) {
         if (!IsSaved && mShapes.Count != 0) {
            Prompt (sender, e);
            if (!IsCancelled) Close ();
         } else Close ();
      }

      /// <summary>Click event of new menu item</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnNew (object sender, RoutedEventArgs e) {
         if (IsSaved) {
            mShapes.Clear ();
            IsSaved = false;
            InvalidateVisual ();
         } else if (mShapes.Count != 0) Prompt (sender, e);
      }

      // Prompts the user to save the drawing
      void Prompt (object sender, RoutedEventArgs e) {
         IsCancelled = false;
         DialogResult dr = MessageBox.Show ("Do you want to save changes to Untitled?", "ScribblePad", MessageBoxButtons.YesNoCancel);
         switch (dr) {
            case System.Windows.Forms.DialogResult.Yes:
               OnSaveAs (sender, e);    // Triggering save event if pressed 'yes'
               break;
            case System.Windows.Forms.DialogResult.No:
               IsSaved = false;
               mShapes.Clear ();    // Clearing the drawing if pressed 'no'
               InvalidateVisual ();
               break;
            case System.Windows.Forms.DialogResult.Cancel:
               IsCancelled = true;
               return;
         }
      }

      /// <summary>Click event of open menu item</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnOpen (object sender, RoutedEventArgs e) {
         OpenFileDialog dlgBox = new ();
         dlgBox.Title = "Select a file";
         dlgBox.Filter = "Binary File|*.bin";
         if (dlgBox.ShowDialog () == System.Windows.Forms.DialogResult.OK) {
            if (mShapes.Count != 0 && !IsSaved) Prompt (sender, e);
            if (IsSaved) { mShapes.Clear (); InvalidateVisual (); }
            if (!IsCancelled)
               using (BinaryReader reader = new (File.Open (dlgBox.FileName, FileMode.Open))) {
                  IsSaved = true;
                  pathToFile = dlgBox.FileName;
                  int totalCount = reader.ReadInt32 ();
                  for (int i = 0; i < totalCount; i++) {
                     byte a = reader.ReadByte (); byte b = reader.ReadByte (); byte r = reader.ReadByte (); byte g = reader.ReadByte ();
                     var brush = new SolidColorBrush (Color.FromArgb (a, r, g, b));
                     switch (reader.ReadInt32 ()) { // Reads the file based on the shape (0- rectangle, 1- ellipse, 2- circle, 3- line, 4- scribble, 5- eraser, 6-connected lines)
                        case 0: Read (reader, new Rectangle ()); break;
                        case 1: Read (reader, new Ellipse ()); break;
                        case 2: Read (reader, new Circle ()); break;
                        case 3: Read (reader, new CustomLine ()); break;
                        case 4: Read (reader, new Scribble ()); break;
                        case 5: Read (reader, new Eraser ()); break;
                        case 6: Read (reader, new ConnectedLine ()); break;
                     }

                     void Read (BinaryReader reader, Shape shape) {    // Routine to read the shapes
                        shape.Open (reader);
                        mShapes.Add ((shape, brush));
                        InvalidateVisual ();
                     }
                  }
               }
         }
      }

      /// <summary>Click event for save menu item</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnSave (object sender, RoutedEventArgs e) => OnSaveAs (null, e);

      /// <summary>Click event for save as menu item</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnSaveAs (object sender, RoutedEventArgs e) {
         SaveFileDialog dlgBox = new ();
         dlgBox.FileName = "Untitled";
         dlgBox.Filter = "Binary File|*.bin";
         DialogResult dr;
         if (IsSaved && sender == null) {
            dr = System.Windows.Forms.DialogResult.OK;
            dlgBox.FileName = pathToFile;
         } else dr = dlgBox.ShowDialog ();
         if (dr == System.Windows.Forms.DialogResult.OK) {
            IsSaved = true;
            pathToFile = dlgBox.FileName;
            using (BinaryWriter writer = new (File.Open (dlgBox.FileName, FileMode.Create))) {
               writer.Write (mShapes.Count); // Total number of shapes
               foreach (var shape in mShapes) {
                  var brush = shape.brush;
                  var color = ((SolidColorBrush)brush).Color;
                  byte a = color.A; byte b = color.B; byte r = color.R; byte g = color.G; // Gets the ABRG values of the color
                  writer.Write (a); writer.Write (b); writer.Write (r); writer.Write (g);
                  shape.shape.Save (writer);
               }
            }
         }
      }
      #endregion

      /// <summary>Event handler for escape key press</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnKeyDown (object sender, System.Windows.Input.KeyEventArgs e) {
         if (e.Key == Key.Escape) { mCurrentShape.AddLines (mEndPoint); AddShapes (); }
      }

      // Routine to add shapes to the list
      void AddShapes () {
         var brush = mPen.Brush;
         mShapes.Add ((mCurrentShape, brush));
         if (mCount != mShapes.Count) { mStack.Clear (); EnableChanged (Tools.ReDo, false); }
         EnableChanged (Tools.UnDo, true);
         mCurrentShape = null;
         IsDrawing = false;
         InvalidateVisual ();
      }

      /// <summary>Overriding on-closing method of main window</summary>
      /// <param name="e"></param>
      protected override void OnClosing (CancelEventArgs e) {
         if (!IsSaved && mShapes.Count != 0) {
            Prompt (null, null);
            if (!IsCancelled) base.OnClosing (e); else e.Cancel = true;
         } else base.OnClosing (e);
      }

      /// <summary>Overriding on-render method</summary>
      /// <param name="dc"></param>
      protected override void OnRender (DrawingContext dc) {
         base.OnRender (dc);
         foreach (var shape in mShapes)
            shape.shape.Draw (dc, shape.brush);
         if (IsDrawing && mCurrentShape != null)
            mCurrentShape.Draw (dc, mPen.Brush);
      }

      #region Private data ------------------------------------------
      List<(Shape shape, Brush brush)> mShapes = new ();    // Collection of Shapes and their corresponding color
      Stack<(Shape, Brush)> mStack = new ();    // Used for undo and redo operations
      Pen mPen = new (Brushes.White, 2);
      Point mStartPoint, mEndPoint;
      Shape mCurrentShape;         // Shape which is being rendered
      bool IsSaved = false;        // Keeps track whether the drawing is saved or not
      bool IsDrawing = false;      // Holds the mouse state
      bool IsCancelled = false;    // True, if user clicks 'cancel' to save the drawing
      string pathToFile;           // Path of the saved file
      int mCount;                  // Holds count of the shapes after undo operation
      Shapes mSelectedShape = Shapes.Line;    // Sets default shape as line
      enum Tools { UnDo, ReDo }
      enum Shapes { Rectangle, Ellipse, Line, Circle, Scribble, Eraser, ConnectedLine }
      #endregion
   }
   #endregion
}