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

      /// <summary>Click event for ellipse button</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnEllipse (object sender, RoutedEventArgs e) => mSelectedShape = "Ellipse";

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
         if (mSelectedShape != null) {
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
               default: break;
            }
            IsDrawing = true;
         }
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
            mShapes.Add (mCurrentShape);
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
         mShapes.Clear ();
         InvalidateVisual ();
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
               for (int i = 0; i < totalCount; i++)
                  switch (reader.ReadInt32 ()) { // Reads the file based on the shape (1- rectangle, 2- ellipse, 3- circle, 4- line, 5- scribble)
                     case 1:
                        var rect = new Rectangle ();
                        rect.Open (reader);
                        mShapes.Add (rect);
                        InvalidateVisual ();
                        break;
                     case 2:
                        var ellipse = new Ellipse ();
                        ellipse.Open (reader);
                        mShapes.Add (ellipse);
                        InvalidateVisual ();
                        break;
                     case 3:
                        var circle = new Circle ();
                        circle.Open (reader);
                        mShapes.Add (circle);
                        InvalidateVisual ();
                        break;
                     case 4:
                        var line = new Line ();
                        line.Open (reader);
                        mShapes.Add (line);
                        InvalidateVisual ();
                        break;
                     case 5:
                        var scribble = new Scribble ();
                        scribble.Open (reader);
                        mShapes.Add (scribble);
                        InvalidateVisual ();
                        break;
                     default: break;
                  }


            }
      }

      /// <summary>Click event for save menu item</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnSave (object sender, RoutedEventArgs e) {
         SaveFileDialog dlgBox = new ();
         dlgBox.FileName = "Drawing";
         dlgBox.Filter = "Binary File|*.bin";
         DialogResult dr = dlgBox.ShowDialog ();
         if (dr == System.Windows.Forms.DialogResult.OK) {
            var pathToFile = dlgBox.FileName;
            using (BinaryWriter writer = new (File.Open (pathToFile, FileMode.Create))) {
               writer.Write (mShapes.Count); // Total number of shapes
               foreach (var shape in mShapes)
                  shape.Save (writer);
            }
         }
      }
      #endregion

      /// <summary>Overriding on-render method</summary>
      /// <param name="dc"></param>
      protected override void OnRender (DrawingContext dc) {
         base.OnRender (dc);
         foreach (var shape in mShapes) {
            shape.Draw (dc);
         }
         if (IsDrawing && mCurrentShape != null) {
            mCurrentShape.Draw (dc);
         }
      }

      #region Private data ------------------------------------------
      List<Shape> mShapes = new (); // Collection of Shapes
      Stack<Shape> mStack = new (); // Used for undo and redo operations
      Point mStartPoint;
      Point mEndPoint;
      Shape mCurrentShape;         // Shape which is being rendered
      bool IsDrawing = false;
      string mSelectedShape;       // The selected shape as string
      #endregion
   }
   #endregion
}