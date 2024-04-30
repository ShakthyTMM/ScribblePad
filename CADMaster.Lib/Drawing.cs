namespace Data;

#region Interface IDraw -----------------------------------------------------------
public interface IDraw {
   void DrawLine (Point startPoint, Point endPoint);
   void DrawRectangle (Point startPoint, Point endPoint);
   void DrawCL (List<Line> lines, Point startPoint, Point endPoint);
   void DrawCircle (Point startPoint, double radius);
}
#endregion

#region Struct Point --------------------------------------------------------------
public struct Point {    // Custom point struct with x and y co-ordinates
   public double X { get { return x; } set { x = value; } }
   public double Y { get { return y; } set { y = value; } }
   double x;
   double y;
}
#endregion

#region Class Shape ---------------------------------------------------------------

// Base class for all shapes
public abstract class Shape {
   #region Constructor ------------------------------------------------------
   public Shape () { }
   public Shape (Point startPoint) => StartPoint = startPoint;
   #endregion

   #region Methods ----------------------------------------------------------
   public abstract void Open (BinaryReader reader);    // Opens and loads binary file

   public abstract void Save (BinaryWriter writer);    // Saves the drawing in binary format

   public abstract void Update (Point endpoint);    // Updates the state of the shape

   public abstract bool IsSelected (Point point);

   public virtual void AddLines (Point point) { }

   public virtual void RemoveLine () { }

   public abstract void Draw (IDraw draw);
   #endregion

   #region Properties and fields---------------------------------------------

   public Point StartPoint { get; set; }    // Starting point of the shape
   public Point EndPoint { get; set; }    // Ending point of the shape
   public List<Line> Lines => mConnectedLines;
   protected static List<Line> mConnectedLines = new ();    // List of lines 
   protected enum EShapes { Rectangle, Line, Circle, ConnectedLine }
   #endregion
}
#endregion

#region Class Circle --------------------------------------------------------------
public class Circle : Shape {
   #region Constructor ------------------------------------------------------
   public Circle () { }
   public Circle (Point startPoint) : base (startPoint) { }
   #endregion

   #region Methods ----------------------------------------------------------
   public override void Open (BinaryReader reader) {
      (mStartPoint.X, mStartPoint.Y, mRadius) = (reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble ());
      StartPoint = mStartPoint;
   }

   public override void Save (BinaryWriter writer) {
      writer.Write ((int)EShapes.Circle);    // Used for identification of circle in binary file
      writer.Write (StartPoint.X); writer.Write (StartPoint.Y); writer.Write (mRadius);
   }

   public override void Update (Point endPoint) {
      mRadius = Math.Sqrt (Math.Pow (endPoint.X - StartPoint.X, 2) + Math.Pow (endPoint.Y - StartPoint.Y, 2));
   }

   public override bool IsSelected (Point point) {
      double distance = Math.Sqrt (Math.Pow (point.X - mStartPoint.X, 2) + Math.Pow (point.Y - mStartPoint.Y, 2));
      if (Math.Abs (distance - Radius) < 10) return true;
      return false;
   }

   public override void Draw (IDraw draw) {
      draw.DrawCircle (StartPoint, Radius);
   }
   #endregion

   #region Property ---------------------------------------------------------
   public double Radius { get { return mRadius; } }
   #endregion

   #region Private Data -----------------------------------------------------
   double mRadius;
   Point mStartPoint;
   #endregion
}
#endregion

#region Class Rectangle -----------------------------------------------------------
public class Rectangle : Shape {
   #region Constructor ------------------------------------------------------
   public Rectangle () { }
   public Rectangle (Point startPoint) : base (startPoint) { }
   #endregion

   #region Methods ----------------------------------------------------------
   public override void Open (BinaryReader reader) {
      (mStartPoint.X, mStartPoint.Y, mEndPoint.X, mEndPoint.Y) = (reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble ());
      StartPoint = mStartPoint; EndPoint = mEndPoint;
   }

   public override void Save (BinaryWriter writer) {
      writer.Write ((int)EShapes.Rectangle);    // Used for identification of rectangle from the binary file
      writer.Write (StartPoint.X); writer.Write (StartPoint.Y);
      writer.Write (EndPoint.X); writer.Write (EndPoint.Y);
   }

   public override void Update (Point endPoint) => EndPoint = endPoint;

   public override bool IsSelected (Point point) {
      if ((int)point.X == (int)StartPoint.X || (int)point.X == (int)EndPoint.X) return true;
      if ((int)point.Y == (int)StartPoint.Y || (int)point.Y == (int)EndPoint.Y) return true;
      return false;
   }

   public override void Draw (IDraw draw) {
      draw.DrawRectangle (StartPoint, EndPoint);
   }
   #endregion

   #region Private Data -----------------------------------------------------
   Point mStartPoint, mEndPoint;
   #endregion
}
#endregion

#region Class Line ----------------------------------------------------------------
public class Line : Shape {
   #region Constructor ------------------------------------------------------
   public Line () { }
   public Line (Point startPoint) : base (startPoint) { }
   #endregion

   #region Methods ----------------------------------------------------------
   public override void Draw (IDraw draw) {
      draw.DrawLine (StartPoint, EndPoint);
   }

   public override void Open (BinaryReader reader) {
      (mStartPoint.X, mStartPoint.Y, mEndPoint.X, mEndPoint.Y) = (reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble ());
      StartPoint = mStartPoint; EndPoint = mEndPoint;
   }

   public override void Save (BinaryWriter writer) {
      writer.Write ((int)EShapes.Line);    // Used for identification of line in binary file
      writer.Write (StartPoint.X); writer.Write (StartPoint.Y);
      writer.Write (EndPoint.X); writer.Write (EndPoint.Y);
   }

   public override bool IsSelected (Point point) {
      double m, y;
      m = (EndPoint.Y - StartPoint.Y) / (EndPoint.X - StartPoint.X);
      if (EndPoint.X > StartPoint.X) y = (m * point.X) - (m * StartPoint.X) + StartPoint.Y;
      else y = (m * point.X) - (m * EndPoint.X) + EndPoint.Y;
      if (Math.Abs (y - point.Y) < 10) return true;
      return false;
   }

   public override void Update (Point endPoint) => EndPoint = endPoint;
   #endregion

   #region Private Data -----------------------------------------------------
   Point mStartPoint, mEndPoint;
   #endregion
}
#endregion

#region Class Connectedline -------------------------------------------------------
public class ConnectedLine : Shape {
   #region Constructor ------------------------------------------------------
   public ConnectedLine () { }
   public ConnectedLine (Point startPoint) : base (startPoint) => StartPoint = startPoint;
   #endregion

   #region Methods ----------------------------------------------------------
   public override void Open (BinaryReader reader) {
      int count = reader.ReadInt32 ();
      for (int i = 0; i < count; i++) {
         (mStartPoint.X, mStartPoint.Y, mEndPoint.X, mEndPoint.Y) = (reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble (), reader.ReadDouble ());
         Line line = new (mStartPoint);
         line.Update (mEndPoint);
         mConnectedLines.Add (line);
      }
   }

   public override void Save (BinaryWriter writer) {
      writer.Write ((int)EShapes.ConnectedLine);    // Used for identification of line in binary file
      writer.Write (mConnectedLines.Count);
      foreach (var line in mConnectedLines) {
         writer.Write (line.StartPoint.X); writer.Write (line.StartPoint.Y);
         writer.Write (line.EndPoint.X); writer.Write (line.EndPoint.Y);
      }
   }

   public override void Update (Point endPoint) => EndPoint = endPoint;

   public override void AddLines (Point endPoint) {    // Adds the lines to the list
      Line line = new (StartPoint);
      line.Update (endPoint);
      mConnectedLines.Add (line);
      StartPoint = endPoint;
   }

   public override void RemoveLine () => EndPoint = StartPoint;

   public override bool IsSelected (Point point) {
      throw new NotImplementedException ();
   }

   public override void Draw (IDraw draw) {
      draw.DrawCL (Lines, StartPoint, EndPoint);
   }
   #endregion

   #region Private Data -----------------------------------------------------
   Point mStartPoint, mEndPoint;
   #endregion
}
#endregion
