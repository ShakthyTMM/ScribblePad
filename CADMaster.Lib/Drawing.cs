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
   public Point (double x, double y) {
      this.x = x; this.y = y;
   }

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
   public Bound Bound2 { get; set; }
   public List<Line> Lines => mConnectedLines;
   protected static List<Line> mConnectedLines = new ();    // List of lines 
   protected enum EShapes { Rectangle, Line, ConnectedLine }
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

# region Struct Bound --------------------------------------------------------------
public readonly struct Bound { // Bound in drawing space
   #region Constructors
   public Bound (Point cornerA, Point cornerB) {
      MinX = Math.Min (cornerA.X, cornerB.X);
      MaxX = Math.Max (cornerA.X, cornerB.X);
      MinY = Math.Min (cornerA.Y, cornerB.Y);
      MaxY = Math.Max (cornerA.Y, cornerB.Y);
   }

   public Bound (IEnumerable<Point> pts) {
      MinX = pts.Min (p => p.X);
      MaxX = pts.Max (p => p.X);
      MinY = pts.Min (p => p.Y);
      MaxY = pts.Max (p => p.Y);
   }

   public Bound (IEnumerable<Bound> bounds) {
      MinX = bounds.Min (b => b.MinX);
      MaxX = bounds.Max (b => b.MaxX);
      MinY = bounds.Min (b => b.MinY);
      MaxY = bounds.Max (b => b.MaxY);
   }

   public Bound () {
      this = Empty;
   }

   public static readonly Bound Empty = new () { MinX = double.MaxValue, MinY = double.MaxValue, MaxX = double.MinValue, MaxY = double.MinValue };
   #endregion

   #region Properties
   public double MinX { get; init; }
   public double MaxX { get; init; }
   public double MinY { get; init; }
   public double MaxY { get; init; }
   public double Width => MaxX - MinX;
   public double Height => MaxY - MinY;
   public Point Mid => new ((MaxX + MinX) / 2, (MaxY + MinY) / 2);
   public bool IsEmpty => MinX > MaxX || MinY > MaxY;
   #endregion

   #region Methods
   public Bound Inflated (Point ptAt, double factor) {
      if (IsEmpty) return this;
      var minX = ptAt.X - (ptAt.X - MinX) * factor;
      var maxX = ptAt.X + (MaxX - ptAt.X) * factor;
      var minY = ptAt.Y - (ptAt.Y - MinY) * factor;
      var maxY = ptAt.Y + (MaxY - ptAt.Y) * factor;
      return new () { MinX = minX, MaxX = maxX, MinY = minY, MaxY = maxY };
   }
   #endregion
}
#endregion
