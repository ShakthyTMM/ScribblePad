// ----------------------------------------------------------------------------------------
// Training 
// Copyright (c) Metamation India.
// ----------------------------------------------------------------------------------------
// MainWindow.xaml.cs
// Scribble Pad
// To design scribble pad
// ----------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System;

#region ScribblePad ---------------------------------------------------------------------------------------- 
namespace ScribblePad {
   public partial class MainWindow : Window {
      public MainWindow () {
         InitializeComponent ();
         MouseDown += OnMouseDown;
         MouseUp += OnMouseUp;
         MouseMove += OnMouseMove;
      }

      #region Events ----------------------------------------
      /// <summary>Changes the colour of the pen</summary>
      void ChangeColor () {
         if (!IsErase) {
            ColorDialog dialog = new ();
            if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
               mPen.Brush = new SolidColorBrush (Color.FromArgb (dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B));
         }
      }

      /// <summary>Event handler for the button to change colour</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnColorFillClick (object sender, RoutedEventArgs e) => ChangeColor ();

      /// <summary>Event handler to erase the drawing</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnEraserClick (object sender, RoutedEventArgs e) { mPen.Brush = Brushes.Black; mPen.Thickness = 15; IsErase = true; }

      #region Mouse Events ----------------------------------------
      /// <summary>Event handler for mouse down event</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnMouseDown (object sender, MouseButtonEventArgs e) {
         if (e.ButtonState == MouseButtonState.Pressed) {
            mStart = e.GetPosition (this);
            mPoints.Add (mStart);
            mCount++;
            InvalidateVisual ();
         }
      }

      /// <summary>Event handler for mouse move event</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnMouseMove (object sender, System.Windows.Input.MouseEventArgs e) {
         if (e.LeftButton == MouseButtonState.Pressed) {
            mEnd = e.GetPosition (this);
            mPoints.Add (mEnd);
            InvalidateVisual ();
         }
      }

      /// <summary>Event handler for mouse up event</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnMouseUp (object sender, MouseButtonEventArgs e) {
         if (e.LeftButton == MouseButtonState.Released) {
            var brush = mPen.Brush;
            double thickness = mPen.Thickness;
            mScribbles.Add ((mCount, brush, thickness, mPoints.ToList ()));
            mPoints.Clear ();
         }
      }
      #endregion

      #region Open ----------------------------------------
      /// <summary>Event handler for open button click event</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnOpenClick (object sender, RoutedEventArgs e) {
         OpenFileDialog dlgBox = new ();
         dlgBox.Title = "Select a file";
         dlgBox.Filter = "Text File|*.txt|Binary File|*.bin";
         if (dlgBox.ShowDialog () == System.Windows.Forms.DialogResult.OK)
            switch (dlgBox.FilterIndex) {
               case 1: OpenText (dlgBox.FileName); break;
               case 2: OpenBinary (dlgBox.FileName); break;
            }
      }

      /// <summary>Loads the text file</summary>
      /// <param name="path">Path of the selected file</param>
      void OpenText (string path) {
         var lines = File.ReadAllLines (path);
         List<Point> points = new ();
         int totalCount = int.Parse (lines[0]);
         for (int i = 1; i <= totalCount; i++) {
            int index = Array.IndexOf (lines, $"List{i}");
            var brush = (Brush)new BrushConverter ().ConvertFromString (lines[index + 1]);
            double thickness = double.Parse (lines[index + 2]);
            (int count, int k) = (int.Parse (lines[index + 3]), Array.IndexOf (lines, lines[index + 3]));
            for (int j = 1; j <= count; j++) {
               string[] coords = lines[k + j].Split (',');
               Point p = new ();
               p.X = double.Parse (coords[0]);
               p.Y = double.Parse (coords[1]);
               points.Add (p);
            }
            mScribbles.Add ((i, brush, thickness, points.ToList ()));
            points.Clear ();
         }
         InvalidateVisual ();
      }

      /// <summary>Loads the binary file</summary>
      /// <param name="path">Path of the selected binary file</param>
      void OpenBinary (string path) {
         using (BinaryReader reader = new (File.Open (path, FileMode.Open))) {
            Point p = new ();
            List<Point> points = new ();
            int totalCount = reader.ReadInt32 ();
            for (int i = 1; i <= totalCount; i++) {
               int count = reader.ReadInt32 ();
               byte a = reader.ReadByte ();
               byte b = reader.ReadByte ();
               byte r = reader.ReadByte ();
               byte g = reader.ReadByte ();
               var brush = new SolidColorBrush (Color.FromArgb (a, r, g, b));
               double thickness = reader.ReadDouble ();
               for (int j = 0; j < count; j++) {
                  p.X = reader.ReadDouble ();
                  p.Y = reader.ReadDouble ();
                  points.Add (p);
               }
               mScribbles.Add ((i, brush, thickness, points.ToList ())); points.Clear ();
            }
            InvalidateVisual ();
         }
      }
      #endregion

      /// <summary>Event handler to switch to pen</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnPenClick (object sender, RoutedEventArgs e) {
         if (IsErase) {
            for (int i = mScribbles.Count - 1; i >= 0; i--) {
               if (mScribbles[i].color == Brushes.Black) continue;
               mPen.Brush = mScribbles[i].color;
               mPen.Thickness = mScribbles[i].thickness;
               break;
            }
            IsErase = false;
         }
      }

      /// <summary>Re-renders the deleted drawing</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnReDo (object sender, RoutedEventArgs e) {
         if (mStack.Count > 0) {
            mScribbles.Add (mStack.Pop ());
            InvalidateVisual ();
         }
      }

      /// <summary>Overriding on-render method for main window</summary> 
      /// <param name="dc"></param>
      protected override void OnRender (DrawingContext dc) {
         base.OnRender (dc);
         foreach (var (_, color, thickness, points) in mScribbles)
            for (int i = 1; i < points.Count; i++)
               dc.DrawLine (new Pen (color, thickness), points[i - 1], points[i]);
         for (int i = 1; i < mPoints.Count; i++)
            dc.DrawLine (new Pen (mPen.Brush, mPen.Thickness), mPoints[i - 1], mPoints[i]);
      }

      /// <summary>Deletes the last drawing</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnUnDo (object sender, RoutedEventArgs e) {
         if (mScribbles.Count > 0) {
            mStack.Push (mScribbles.Last ());
            mScribbles.Remove (mScribbles.Last ());
            InvalidateVisual ();
         }
      }

      #region Save ----------------------------------------
      /// <summary>Event handler for save button click event</summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnSaveClick (object sender, RoutedEventArgs e) {
         SaveFileDialog dlgBox = new ();
         dlgBox.FileName = "Drawing";
         dlgBox.Filter = "Text File|*.txt|Binary File|*.bin";
         DialogResult dr = dlgBox.ShowDialog ();
         if (dr == System.Windows.Forms.DialogResult.OK) {
            var pathToFile = dlgBox.FileName;
            switch (dlgBox.FilterIndex) {
               case 1: SaveText (pathToFile); break;
               case 2: SaveBinary (pathToFile); break;
            }
         }
      }

      /// <summary>Saves scribble pad as text file</summary>
      /// <param name="path">Path of the file to be saved</param>
      void SaveText (string path) {
         using (TextWriter writer = new StreamWriter (path)) {
            writer.WriteLine (mScribbles.Count); // Total number of scribbles
            int count = 1;
            foreach (var (_, color, thickness, points) in mScribbles) {
               writer.WriteLine ($"List{count}");
               writer.WriteLine (color.ToString ());
               writer.WriteLine ($"{thickness}");
               writer.WriteLine (points.Count);
               for (int i = 0; i < points.Count; i++)
                  writer.WriteLine (points[i]);
               count++;
            }
         }
      }

      /// <summary>Saves scribble pad as binary file</summary>
      /// <param name="path">Path of the file to be saved</param>
      void SaveBinary (string path) {
         using (BinaryWriter writer = new (File.Open (path, FileMode.Create))) {
            writer.Write (mScribbles.Count); // Total number of scribbles
            foreach (var (_, color, thickness, points) in mScribbles) {
               writer.Write (points.Count); // Number of points of a scribble
               var brush = color;
               var colour = ((SolidColorBrush)brush).Color;
               byte a = colour.A; byte b = colour.B; byte r = colour.R; byte g = colour.G; // Gets the ABRG values of the color
               writer.Write (a);
               writer.Write (b);
               writer.Write (r);
               writer.Write (g);
               writer.Write (thickness);
               for (int j = 0; j < points.Count; j++) {
                  writer.Write (points[j].X);
                  writer.Write (points[j].Y);
               }
            }
         }
      }
      #endregion

      #endregion

      #region Private data ------------------------------------------
      List<(int count, Brush color, double thickness, List<Point> points)> mScribbles = new ();
      Stack<(int count, Brush color, double thickness, List<Point> points)> mStack = new ();
      Pen mPen = new (Brushes.White, 2);
      List<Point> mPoints = new ();
      Point mStart, mEnd = new ();
      bool IsErase = false;
      int mCount;
      #endregion
   }
}
#endregion