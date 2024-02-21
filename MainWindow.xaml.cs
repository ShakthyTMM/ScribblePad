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

#region ScribblePad ---------------------------------------------------------------------------------------- 
namespace ScribblePad {
    public partial class MainWindow : Window {
        public MainWindow () {
            InitializeComponent ();
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseMove += OnMouseMove;
            mPen.Brush = Brushes.White;
            mPen.Thickness = 2;
        }

        #region Events ----------------------------------------
        /// <summary>Changes the colour of the pen</summary>
        void ChangeColor () {
            ColorDialog dialog = new ();
            if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                mPen.Brush = new SolidColorBrush (Color.FromArgb (dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B));
        }

        /// <summary>Event handler for the button to change colour</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnColorButtonClick (object sender, RoutedEventArgs e) => ChangeColor ();

        /// <summary>Event handler for mouse down event</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMouseDown (object sender, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed) {
                mStart = e.GetPosition (this);
                mPoints.Add (mStart);
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
                mScribbles.Add (mPoints.ToList ());
                mPoints.Clear ();
            }
        }

        /// <summary>Event handler for open button click event</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnOpenClick (object sender, RoutedEventArgs e) {
            OpenFileDialog op = new ();
            op.Title = "Select a file";
            op.Filter = "Text File|*.txt|Binary File|*.bin";
            if (op.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                switch (op.FilterIndex) {
                    case 1: OpenText (op.FileName); break;
                    case 2: OpenBinary (op.FileName); break;
                }
        }

        /// <summary>Loads the text file</summary>
        /// <param name="path">Path of the selected file</param>
        void OpenText (string path) {
            string[] lines = File.ReadAllLines (path);
            List<Point> points = new ();
            foreach (var c in lines) {
                if (c is "List" or "") AddToList ();
                else if (c.Contains ('#')) ColorConverter (c);
                else {
                    string[] coords = c.Split (',');
                    Point p = new ();
                    p.X = double.Parse (coords[0]);
                    p.Y = double.Parse (coords[1]);
                    points.Add (p);
                }
            }
            void AddToList () { mScribbles.Add (points.ToList ()); points.Clear (); } // Adds each scribble as a list to a main list

            void ColorConverter (string color) =>
                mPen.Brush = (Brush)new BrushConverter ().ConvertFromString (color); // Retrives the brush color
            InvalidateVisual ();
        }

        /// <summary>Loads the binary file</summary>
        /// <param name="path">Path of the selected binary file</param>
        void OpenBinary (string path) {
            using (BinaryReader reader = new (File.Open (path, FileMode.Open))) {
                Point p = new ();
                List<Point> points = new ();
                int totalCount = reader.ReadInt32 ();
                for (int i = 0; i < totalCount; i++) {
                    int count = reader.ReadInt32 ();
                    byte a = reader.ReadByte ();
                    byte b = reader.ReadByte ();
                    byte r = reader.ReadByte ();
                    byte g = reader.ReadByte ();
                    mPen.Brush = new SolidColorBrush (Color.FromArgb (a, r, g, b));
                    for (int j = 0; j < count; j++) {
                        p.X = reader.ReadDouble ();
                        p.Y = reader.ReadDouble ();
                        points.Add (p);
                    }
                    mScribbles.Add (points.ToList ()); points.Clear ();
                }
                InvalidateVisual ();
            }
        }

        /// <summary>Overriding on-render method for main window</summary> 
        /// <param name="dc"></param>
        protected override void OnRender (DrawingContext dc) {
            base.OnRender (dc);
            for (int i = 0; i < mScribbles.Count; i++) {
                for (int j = 1; j < mScribbles[i].Count; j++)
                    dc.DrawLine (mPen, mScribbles[i][j - 1], mScribbles[i][j]);
            }
            for (int i = 1; i < mPoints.Count; i++)
                dc.DrawLine (mPen, mPoints[i - 1], mPoints[i]);
        }

        /// <summary>Event handler for save button click event</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSaveButtonClick (object sender, RoutedEventArgs e) {
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
                for (int i = 0; i < mScribbles.Count; i++) {
                    writer.WriteLine ("List");
                    writer.WriteLine (mPen.Brush.ToString ());
                    for (int j = 0; j < mScribbles[i].Count; j++)
                        writer.WriteLine (mScribbles[i][j]);
                }
                writer.WriteLine ();
            }
        }

        /// <summary>Saves scribble pad as binary file</summary>
        /// <param name="path">Path of the file to be saved</param>
        void SaveBinary (string path) {
            using (BinaryWriter writer = new (File.Open (path, FileMode.Create))) {
                writer.Write (mScribbles.Count); // Total number of scribbles
                for (int i = 0; i < mScribbles.Count; i++) {
                    writer.Write (mScribbles[i].Count); // Number of points of a scribble
                    var brush = mPen.Brush;
                    var color = ((SolidColorBrush)brush).Color;
                    byte a = color.A; byte b = color.B; byte r = color.R; byte g = color.G;
                    writer.Write (a);
                    writer.Write (b);
                    writer.Write (r);
                    writer.Write (g);
                    for (int j = 0; j < mScribbles[i].Count; j++) {
                        writer.Write (mScribbles[i][j].X);
                        writer.Write (mScribbles[i][j].Y);
                    }
                }
            }
        }
        #endregion

        #region Private data ------------------------------------------
        Point mStart, mEnd = new ();
        List<List<Point>> mScribbles = new ();
        List<Point> mPoints = new ();
        Pen mPen = new ();
        #endregion
    }
}
#endregion