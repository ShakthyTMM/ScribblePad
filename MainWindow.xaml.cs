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
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using System.ComponentModel;

namespace DrawingShapes {
   #region Program ---------------------------------------------------------------------------------------- 
   public partial class MainWindow : Window {
      public MainWindow () {
         InitializeComponent ();
         MouseDown += OnMouseDown;
         MouseMove += OnMouseMove;
         MouseUp += OnMouseUp;
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
            if (mCount != mShapes.Count) mStack.Clear ();
            else {
               mShapes.Add (mStack.Pop ());
               mCount = mShapes.Count;
               InvalidateVisual ();
            }
         }
      }

      /// <summary>Click event for undo button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnUnDo (object sender, RoutedEventArgs e) {
         if (mShapes.Count > 0) {
            mStack.Push (mShapes.Last ());
            mShapes.Remove (mShapes.Last ());
            mCount = mShapes.Count;
            InvalidateVisual ();
         }
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
      void OnMouseDown (object sender, MouseEventArgs e) {
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
               mCurrentShape = new Line (mStartPoint);
               break;
            case Shapes.Scribble:
               mCurrentShape = new Scribble (mStartPoint);
               break;
            case Shapes.Eraser:
               mCurrentShape = new Eraser (mStartPoint);
               break;
            default: break;
         }
         IsDrawing = true;
      }

      /// <summary>Event handler for mouse move event</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnMouseMove (object sender, MouseEventArgs e) {
         if (IsDrawing) {
            mEndPoint = e.GetPosition (this);
            mCurrentShape.Update (mEndPoint);
            InvalidateVisual ();
         }
      }

      /// <summary>Event handler for mouse up event</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnMouseUp (object sender, MouseEventArgs e) {
         if (IsDrawing) {
            mEndPoint = e.GetPosition (this);
            mCurrentShape.Update (mEndPoint);
            var brush = mPen.Brush;
            mShapes.Add ((mCurrentShape, brush));
            mCurrentShape = null;
            IsDrawing = false;
            InvalidateVisual ();
         }
      }
      #endregion

      #region Menu Items ----------------------------------------
      /// <summary>Click event for exit menu item</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnExit (object sender, RoutedEventArgs e) {
         if (!IsSaved && mShapes.Count != 0) Prompt (sender, e);
         else Close ();
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
            if (IsSaved) {
               mShapes.Clear ();
               InvalidateVisual ();
            }
            if (!IsCancelled)
               using (BinaryReader reader = new (File.Open (dlgBox.FileName, FileMode.Open))) {
                  IsSaved= true;
                  pathToFile = dlgBox.FileName;
                  int totalCount = reader.ReadInt32 ();
                  for (int i = 0; i < totalCount; i++) {
                     byte a = reader.ReadByte (); byte b = reader.ReadByte (); byte r = reader.ReadByte (); byte g = reader.ReadByte ();
                     var brush = new SolidColorBrush (Color.FromArgb (a, r, g, b));
                     switch (reader.ReadInt32 ()) { // Reads the file based on the shape (1- rectangle, 2- ellipse, 3- circle, 4- line, 5- scribble, 6- eraser)
                        case 1: Read (reader, new Rectangle ()); break;
                        case 2: Read (reader, new Ellipse ()); break;
                        case 3: Read (reader, new Circle ()); break;
                        case 4: Read (reader, new Line ()); break;
                        case 5: Read (reader, new Scribble ()); break;
                        case 6: Read (reader, new Eraser ()); break;
                     }

                     void Read (BinaryReader reader, Shape shape) {
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
            IsCancelled = false;
            using (BinaryWriter writer = new (File.Open (dlgBox.FileName, FileMode.Create))) {
               writer.Write (mShapes.Count); // Total number of shapes
               foreach (var shape in mShapes) {
                  var brush = shape.Item2;
                  var color = ((SolidColorBrush)brush).Color;
                  byte a = color.A; byte b = color.B; byte r = color.R; byte g = color.G; // Gets the ABRG values of the color
                  writer.Write (a); writer.Write (b); writer.Write (r); writer.Write (g);
                  shape.Item1.Save (writer);
               }
            }
         }
      }
      #endregion

      /// <summary>Overriding on-closing method of main window</summary>
      /// <param name="e"></param>
      protected override void OnClosing (CancelEventArgs e) {
         if (!IsSaved && mShapes.Count != 0) {
            Prompt (null, null);
            if (IsCancelled) e.Cancel = true;
            if(!IsSaved) base.OnClosing (e);
         } else base.OnClosing (e);
      }

      /// <summary>Overriding on-render method</summary>
      /// <param name="dc"></param>
      protected override void OnRender (DrawingContext dc) {
         base.OnRender (dc);
         foreach (var shape in mShapes)
            shape.Item1.Draw (dc, shape.Item2);
         if (IsDrawing && mCurrentShape != null)
            mCurrentShape.Draw (dc, mPen.Brush);
      }

      #region Private data ------------------------------------------
      List<(Shape, Brush)> mShapes = new ();    // Collection of Shapes and their corresponding color
      Stack<(Shape, Brush)> mStack = new ();    // Used for undo and redo operations
      Pen mPen = new (Brushes.White, 2);
      Point mStartPoint, mEndPoint;
      Shape mCurrentShape;         // Shape which is being rendered
      bool IsSaved = false;        // Keeps track whether the drawing is saved or not
      bool IsDrawing = false;      // Holds the mouse state
      bool IsCancelled = false;    // True, if user clicks 'cancel' to save the drawing
      string pathToFile;           // Path of the saved file
      int mCount;                  // Holds count of the shapes after undo operation
      Shapes mSelectedShape = Shapes.Scribble;    // Sets default shape as scribble
      enum Shapes { Rectangle, Ellipse, Line, Circle, Scribble, Eraser }
      #endregion
   }
   #endregion
}