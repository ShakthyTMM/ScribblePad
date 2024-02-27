using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.IO;

namespace DrawingShapes {
   public abstract class Shape {        // Base class
      public Shape () { }

      public Shape (Point startPoint) => mStartPoint = startPoint;

      public abstract void Draw (DrawingContext dc, Brush brush);    // Renders the shape

      public abstract void Open (BinaryReader reader);    // Opens and loads binary file

      public abstract void Save (BinaryWriter writer);    // Saves the drawing in binary format

      public abstract void Update (Point endpoint);    // Updates the state of the shape

      protected Point mStartPoint;    // Starting point of the shape
   }

   class Rectangle : Shape {        // Rectangle class
      public Rectangle () { mRect = new Rect (); }

      public Rectangle (Point startPoint) : base (startPoint) => mRect = new Rect (startPoint, startPoint);

      public override void Draw (DrawingContext dc, Brush brush) =>
         dc.DrawRectangle (null, new Pen (brush, 2), mRect);

      public override void Open (BinaryReader reader) {
         mStartPoint.X = reader.ReadDouble ();
         mStartPoint.Y = reader.ReadDouble ();
         mEndPoint.X = reader.ReadDouble ();
         mEndPoint.Y = reader.ReadDouble ();
         mRect = new Rect (mStartPoint, mEndPoint);
      }

      public override void Save (BinaryWriter writer) {
         writer.Write (1);    // Used for identification of rectangle from the binary file
         writer.Write (mStartPoint.X);
         writer.Write (mStartPoint.Y);
         writer.Write (mEndPoint.X);
         writer.Write (mEndPoint.Y);
      }

      public override void Update (Point endPoint) {
         mEndPoint = endPoint;
         mRect = new Rect (mStartPoint, mEndPoint);
      }

      Rect mRect;
      Point mEndPoint;
   }

   class Ellipse : Shape {        // Ellipse class
      public Ellipse () => mEllipse = new EllipseGeometry ();

      public Ellipse (Point startPoint) : base (startPoint) => mEllipse = new EllipseGeometry (startPoint, 0, 0);

      public override void Draw (DrawingContext dc, Brush brush) =>
         dc.DrawGeometry (null, new Pen (brush, 2), mEllipse);

      public override void Open (BinaryReader reader) {
         radiusX = reader.ReadDouble ();
         radiusY = reader.ReadDouble ();
         center.X = reader.ReadDouble ();
         center.Y = reader.ReadDouble ();
         mEllipse = new EllipseGeometry (center, radiusX, radiusY);
      }

      public override void Save (BinaryWriter writer) {
         writer.Write (2);    // Used for identification of ellipse in binary file
         writer.Write (radiusX); writer.Write (radiusY);
         writer.Write (center.X);
         writer.Write (center.Y);
      }

      public override void Update (Point endPoint) {
         radiusX = Math.Abs (endPoint.X - mStartPoint.X) / 2;
         radiusY = Math.Abs (endPoint.Y - mStartPoint.Y) / 2;
         center = new (mStartPoint.X + radiusX, mStartPoint.Y + radiusY);
         mEllipse = new EllipseGeometry (center, radiusX, radiusY);
      }

      EllipseGeometry mEllipse;
      double radiusX, radiusY;
      Point center;
   }

   class Circle : Shape {        // Circle class
      public Circle () { }

      public Circle (Point startPoint) : base (startPoint) => mCircle = new EllipseGeometry (startPoint, 0, 0);

      public override void Draw (DrawingContext dc, Brush brush) =>
         dc.DrawGeometry (null, new Pen (brush, 2), mCircle);

      public override void Open (BinaryReader reader) {
         center.X = reader.ReadDouble ();
         center.Y = reader.ReadDouble ();
         radius = reader.ReadDouble ();
         mCircle = new EllipseGeometry (center, radius, radius);
      }

      public override void Save (BinaryWriter writer) {
         writer.Write (3);    // Used for identification of circle in binary file
         writer.Write (center.X);
         writer.Write (center.Y);
         writer.Write (radius);
      }

      public override void Update (Point endPoint) {
         radius = Math.Abs (endPoint.X - mStartPoint.X) / 2;
         center = new (mStartPoint.X + radius, mStartPoint.Y + radius);
         mCircle = new EllipseGeometry (center, radius, radius);
      }

      EllipseGeometry mCircle;
      double radius;
      Point center;
   }

   class Line : Shape {       // Line class
      public Line () { }

      public Line (Point startPoint) : base (startPoint) => mStartPoint = startPoint;

      public override void Draw (DrawingContext dc, Brush brush) =>
        dc.DrawLine (new Pen (brush, 2), mStartPoint, mEndpoint);

      public override void Open (BinaryReader reader) {
         mStartPoint.X = reader.ReadDouble ();
         mStartPoint.Y = reader.ReadDouble ();
         mEndpoint.X = reader.ReadDouble ();
         mEndpoint.Y = reader.ReadDouble ();
      }

      public override void Save (BinaryWriter writer) {
         writer.Write (4);    // Used for identification of line in binary file
         writer.Write (mStartPoint.X);
         writer.Write (mStartPoint.Y);
         writer.Write (mEndpoint.X);
         writer.Write (mEndpoint.Y);
      }

      public override void Update (Point endPoint) => mEndpoint = endPoint;

      Point mEndpoint;
   }

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
            p.X = reader.ReadDouble ();
            p.Y = reader.ReadDouble ();
            mPoints.Add (p);
         }
      }

      public override void Save (BinaryWriter writer) {
         writer.Write (5);    // Used for identification of scribble in binary file
         writer.Write (mPoints.Count);
         foreach (var point in mPoints) {
            writer.Write (point.X);
            writer.Write (point.Y);
         }
      }

      public override void Update (Point endPoint) => mPoints.Add (endPoint);

      List<Point> mPoints = new ();    // List of points of scribble
   }

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
            p.X = reader.ReadDouble ();
            p.Y = reader.ReadDouble ();
            mPoints.Add (p);
         }
      }

      public override void Save (BinaryWriter writer) {
         writer.Write (6);    // Used for identification of scribble in binary file
         writer.Write (mPoints.Count);
         foreach (var point in mPoints) {
            writer.Write (point.X);
            writer.Write (point.Y);
         }
      }

      public override void Update (Point endPoint) => mPoints.Add (endPoint);

      List<Point> mPoints = new ();
   }
}
