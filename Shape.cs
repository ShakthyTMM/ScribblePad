using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Windows.Shapes;

namespace DrawingShapes {
   #region Shape ----------------------------------------------------------------------------------------
   public abstract class Shape {        // Base class
      public Shape () { }

      public Shape (Point startPoint) => mStartPoint = startPoint;

      public virtual void AddLines (Point startPoint) { }    // Collects the lines of connected lines

      public abstract void Draw (DrawingContext dc, Brush brush);    // Renders the shape

      public abstract void Open (BinaryReader reader);    // Opens and loads binary file

      public abstract void Save (BinaryWriter writer);    // Saves the drawing in binary format

      public abstract void Update (Point endpoint);    // Updates the state of the shape

      protected Point mStartPoint;    // Starting point of the shape

      protected enum Shapes { Rectangle, Ellipse, Circle, CustomLine, Scribble, Eraser, ConnectedLine }    // Identifies the shapes
   }

   #region Rectangle ----------------------------------------
   class Rectangle : Shape {        // Rectangle class
      public Rectangle () { mRect = new Rect (); }

      public Rectangle (Point startPoint) : base (startPoint) => mRect = new Rect (startPoint, startPoint);

      public override void Draw (DrawingContext dc, Brush brush) =>
         dc.DrawRectangle (null, new Pen (brush, 2), mRect);

      public override void Open (BinaryReader reader) {
         (mStartPoint.X, mStartPoint.Y, mEndPoint.X, mEndPoint.Y) = (reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble ());
         mRect = new Rect (mStartPoint, mEndPoint);
      }

      public override void Save (BinaryWriter writer) {
         writer.Write ((int)Shapes.Rectangle);    // Used for identification of rectangle from the binary file
         writer.Write (mStartPoint.X); writer.Write (mStartPoint.Y);
         writer.Write (mEndPoint.X); writer.Write (mEndPoint.Y);
      }

      public override void Update (Point endPoint) {
         mEndPoint = endPoint;
         mRect = new Rect (mStartPoint, mEndPoint);
      }

      Rect mRect;
      Point mEndPoint;
   }
   #endregion

   #region Ellipse ----------------------------------------
   class Ellipse : Shape {        // Ellipse class
      public Ellipse () => mEllipse = new EllipseGeometry ();

      public Ellipse (Point startPoint) : base (startPoint) => mEllipse = new EllipseGeometry (startPoint, 0, 0);

      public override void Draw (DrawingContext dc, Brush brush) =>
         dc.DrawGeometry (null, new Pen (brush, 2), mEllipse);

      public override void Open (BinaryReader reader) {
         (radiusX, radiusY, mStartPoint.X, mStartPoint.Y) = (reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble ());
         mEllipse = new EllipseGeometry (mStartPoint, radiusX, radiusY);
      }

      public override void Save (BinaryWriter writer) {
         writer.Write ((int)Shapes.Ellipse);    // Used for identification of ellipse in binary file
         writer.Write (radiusX); writer.Write (radiusY);
         writer.Write (mStartPoint.X); writer.Write (mStartPoint.Y);
      }

      public override void Update (Point endPoint) {
         radiusX = endPoint.X - mStartPoint.X;
         radiusY = endPoint.Y - mStartPoint.Y;
         mEllipse = new EllipseGeometry (mStartPoint, radiusX, radiusY);
      }

      EllipseGeometry mEllipse;
      double radiusX, radiusY;
   }
   #endregion

   #region Circle ----------------------------------------
   class Circle : Shape {        // Circle class
      public Circle () { }

      public Circle (Point startPoint) : base (startPoint) => mCircle = new EllipseGeometry (startPoint, 0, 0);

      public override void Draw (DrawingContext dc, Brush brush) =>
         dc.DrawGeometry (null, new Pen (brush, 2), mCircle);

      public override void Open (BinaryReader reader) {
         (mStartPoint.X, mStartPoint.Y, radius) = (reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble ());
         mCircle = new EllipseGeometry (mStartPoint, radius, radius);
      }

      public override void Save (BinaryWriter writer) {
         writer.Write ((int)Shapes.Circle);    // Used for identification of circle in binary file
         writer.Write (mStartPoint.X); writer.Write (mStartPoint.Y); writer.Write (radius);
      }

      public override void Update (Point endPoint) {
         radius = Math.Sqrt (Math.Pow (endPoint.X - mStartPoint.X, 2) + Math.Pow (endPoint.Y - mStartPoint.Y, 2));
         mCircle = new EllipseGeometry (mStartPoint, radius, radius);
      }

      EllipseGeometry mCircle;
      double radius;
   }
   #endregion

   #region CustomLine ----------------------------------------
   class CustomLine : Shape {       // Line class
      public CustomLine () { }

      public CustomLine (Point startPoint) : base (startPoint) => mStartPoint = startPoint;

      public override void Draw (DrawingContext dc, Brush brush) =>
        dc.DrawLine (new Pen (brush, 2), mStartPoint, mEndpoint);

      public override void Open (BinaryReader reader) =>
         (mStartPoint.X, mStartPoint.Y, mEndpoint.X, mEndpoint.Y) = (reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble ());

      public override void Save (BinaryWriter writer) {
         writer.Write ((int)Shapes.CustomLine);    // Used for identification of line in binary file
         writer.Write (mStartPoint.X); writer.Write (mStartPoint.Y);
         writer.Write (mEndpoint.X); writer.Write (mEndpoint.Y);
      }

      public override void Update (Point endPoint) => mEndpoint = endPoint;

      Point mEndpoint;
   }
   #endregion

   #region Scribble ----------------------------------------
   class Scribble : Shape {        // Scribble class
      public Scribble () { }

      public Scribble (Point startPoint) : base (startPoint) => mPoints.Add (startPoint);

      public override void Draw (DrawingContext dc, Brush brush) {
         for (int i = 1; i < mPoints.Count; i++)
            dc.DrawLine (new Pen (brush, 2), mPoints[i - 1], mPoints[i]);
      }

      public override void Open (BinaryReader reader) {
         int count = reader.ReadInt32 ();
         for (int i = 0; i < count; i++) {
            Point p = new ();
            (p.X, p.Y) = (reader.ReadDouble (), reader.ReadDouble ());
            mPoints.Add (p);
         }
      }

      public override void Save (BinaryWriter writer) {
         writer.Write ((int)Shapes.Scribble);    // Used for identification of scribble in binary file
         writer.Write (mPoints.Count);
         foreach (var point in mPoints) {
            writer.Write (point.X); writer.Write (point.Y);
         }
      }

      public override void Update (Point endPoint) => mPoints.Add (endPoint);

      List<Point> mPoints = new ();    // List of points of scribble
   }
   #endregion

   #region Eraser ----------------------------------------
   class Eraser : Shape {
      public Eraser () { }

      public Eraser (Point startPoint) : base (startPoint) => mPoints.Add (startPoint);

      public override void Draw (DrawingContext dc, Brush brush) {
         for (int i = 1; i < mPoints.Count; i++)
            dc.DrawLine (new Pen (Brushes.Black, 5), mPoints[i - 1], mPoints[i]);
      }

      public override void Open (BinaryReader reader) {
         int count = reader.ReadInt32 ();
         for (int i = 0; i < count; i++) {
            Point p = new ();
            (p.X, p.Y) = (reader.ReadDouble (), reader.ReadDouble ());
            mPoints.Add (p);
         }
      }

      public override void Save (BinaryWriter writer) {
         writer.Write ((int)Shapes.Eraser);    // Used for identification of scribble in binary file
         writer.Write (mPoints.Count);
         foreach (var point in mPoints) {
            writer.Write (point.X); writer.Write (point.Y);
         }
      }

      public override void Update (Point endPoint) => mPoints.Add (endPoint);

      List<Point> mPoints = new ();
   }
   #endregion

   #region ConnectedLine ----------------------------------------
   class ConnectedLine : Shape {
      public ConnectedLine () { }

      public ConnectedLine (Point startPoint) : base (startPoint) => mStartPoint = startPoint;

      public override void Draw (DrawingContext dc, Brush brush) {
         foreach (var line in mConnectedLines)
            dc.DrawLine (new Pen (brush, 2), new Point (line.X1, line.Y1), new Point (line.X2, line.Y2));
         dc.DrawLine (new Pen (brush, 2), mStartPoint, mEndpoint);
      }

      public override void Open (BinaryReader reader) {
         int count = reader.ReadInt32 ();
         for (int i = 0; i < count; i++) {
            Line line = new ();
            (line.X1, line.Y1, line.X2, line.Y2) = (reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble ());
            mConnectedLines.Add (line);
         }
      }

      public override void Save (BinaryWriter writer) {
         writer.Write ((int)Shapes.ConnectedLine);    // Used for identification of line in binary file
         writer.Write (mConnectedLines.Count);
         foreach (var line in mConnectedLines) {
            writer.Write (line.X1); writer.Write (line.Y1);
            writer.Write (line.X2); writer.Write (line.Y2);
         }
      }

      public override void Update (Point endPoint) => mEndpoint = endPoint;

      public override void AddLines (Point endPoint) {    // Adds the lines to the list
         Line line = new ();
         (line.X1, line.Y1, line.X2, line.Y2) = (mStartPoint.X, mStartPoint.Y, endPoint.X, endPoint.Y);
         mConnectedLines.Add (line);
         mStartPoint = endPoint;
      }

      Point mEndpoint;
      List<Line> mConnectedLines = new ();    // List of lines 
   }
   #endregion

}
#endregion

