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

namespace DrawingShapes {
   #region Program ---------------------------------------------------------------------------------------- 
   public partial class MainWindow : Window {
      public MainWindow () {
         InitializeComponent ();
         MouseDown += OnMouseDown;
         MouseMove += OnMouseMove;
         MouseUp += OnMouseUp;
      }

      #region Click Events ----------------------------------------
      /// <summary>Click event for circle button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnCircle (object sender, RoutedEventArgs e) => mSelectedShape = "Circle";

      /// <summary>Click event for color change button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnColorChange (object sender, RoutedEventArgs e) {
         ColorDialog dialog = new ();
         if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
            mPen.Brush = new SolidColorBrush (Color.FromArgb (dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B));  // Gets the ARGB bytes from the color selected
      }

      /// <summary>Click event for ellipse button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnEllipse (object sender, RoutedEventArgs e) => mSelectedShape = "Ellipse";

      /// <summary>Click event for erase button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnErase (object sender, RoutedEventArgs e) => mSelectedShape = "Eraser";

      /// <summary>Click event for line button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnLine (object sender, RoutedEventArgs e) => mSelectedShape = "Line";

      /// <summary>Click event for rectangle button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnRect (object sender, RoutedEventArgs e) => mSelectedShape = "Rectangle";

      /// <summary>Click event for redo button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnReDo (object sender, RoutedEventArgs e) {
         if (mStack.Count > 0) {
            mShapes.Add (mStack.Pop ());
            InvalidateVisual ();
         }
      }

      /// <summary>Click event for pen button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnScribble (object sender, RoutedEventArgs e) => mSelectedShape = "Scribble";

      /// <summary>Click event for undo button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnUnDo (object sender, RoutedEventArgs e) {
         if (mShapes.Count > 0) {
            mStack.Push (mShapes.Last ());
            mShapes.Remove (mShapes.Last ());
            InvalidateVisual ();
         }
      }
      #endregion

      #region Mouse Events ----------------------------------------
      /// <summary>Event handler for mouse down event</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnMouseDown (object sender, MouseEventArgs e) {
         mStartPoint = e.GetPosition (this);
         switch (mSelectedShape) {
            case "Rectangle":
               mCurrentShape = new Rectangle (mStartPoint);
               break;
            case "Ellipse":
               mCurrentShape = new Ellipse (mStartPoint);
               break;
            case "Circle":
               mCurrentShape = new Circle (mStartPoint);
               break;
            case "Line":
               mCurrentShape = new Line (mStartPoint);
               break;
            case "Scribble":
               mCurrentShape = new Scribble (mStartPoint);
               break;
            case "Eraser":
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

      #region Menu Items(New, Save, Open) ----------------------------------------
      /// <summary>Click event of new menu item</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnNew (object sender, RoutedEventArgs e) {
         if (IsSaved) {
            mShapes.Clear ();
            InvalidateVisual ();
         } else PopUp ();

         void PopUp () {    // Pops up a message box 
            DialogResult dr = MessageBox.Show ("Do you want to save changes to Untitled?", "ScribblePad", MessageBoxButtons.YesNo);   // Prompting the user to save the drawing
            switch (dr) {
               case System.Windows.Forms.DialogResult.Yes:
                  OnSave (sender, e);    // Triggering save event if pressed 'yes'
                  break;
               case System.Windows.Forms.DialogResult.No:
                  mShapes.Clear ();    // Clearing the drawing if pressed 'no'
                  InvalidateVisual ();
                  break;
            }
         }
      }

      /// <summary>Click event of open menu item</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnOpen (object sender, RoutedEventArgs e) {
         OpenFileDialog dlgBox = new ();
         dlgBox.Title = "Select a file";
         dlgBox.Filter = "Binary File|*.bin";
         if (dlgBox.ShowDialog () == System.Windows.Forms.DialogResult.OK)
            using (BinaryReader reader = new (File.Open (dlgBox.FileName, FileMode.Open))) {
               int totalCount = reader.ReadInt32 ();
               for (int i = 0; i < totalCount; i++) {
                  byte a = reader.ReadByte (); byte b = reader.ReadByte (); byte r = reader.ReadByte (); byte g = reader.ReadByte ();
                  var brush = new SolidColorBrush (Color.FromArgb (a, r, g, b));
                  switch (reader.ReadInt32 ()) { // Reads the file based on the shape (1- rectangle, 2- ellipse, 3- circle, 4- line, 5- scribble, 6- eraser)
                     case 1:
                        var rect = new Rectangle ();
                        rect.Open (reader);
                        mShapes.Add ((rect, brush));
                        InvalidateVisual ();
                        break;
                     case 2:
                        var ellipse = new Ellipse ();
                        ellipse.Open (reader);
                        mShapes.Add ((ellipse, brush));
                        InvalidateVisual ();
                        break;
                     case 3:
                        var circle = new Circle ();
                        circle.Open (reader);
                        mShapes.Add ((circle, brush));
                        InvalidateVisual ();
                        break;
                     case 4:
                        var line = new Line ();
                        line.Open (reader);
                        mShapes.Add ((line, brush));
                        InvalidateVisual ();
                        break;
                     case 5:
                        var scribble = new Scribble ();
                        scribble.Open (reader);
                        mShapes.Add ((scribble, brush));
                        InvalidateVisual ();
                        break;
                     case 6:
                        var eraser = new Eraser ();
                        eraser.Open (reader);
                        mShapes.Add ((eraser, brush));
                        InvalidateVisual ();
                        break;
                     default: break;
                  }
               }
            }
      }

      /// <summary>Click event for save menu item</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnSave (object sender, RoutedEventArgs e) {
         SaveFileDialog dlgBox = new ();
         dlgBox.FileName = "Untitled";
         dlgBox.Filter = "Binary File|*.bin";
         DialogResult dr = dlgBox.ShowDialog ();
         if (dr == System.Windows.Forms.DialogResult.OK) {
            IsSaved = true;
            var pathToFile = dlgBox.FileName;
            using (BinaryWriter writer = new (File.Open (pathToFile, FileMode.Create))) {
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
      bool IsDrawing = false;       // Holds the mouse state
      string mSelectedShape = "Scribble";       // The selected shape as string
      #endregion
   }
   #endregion
}