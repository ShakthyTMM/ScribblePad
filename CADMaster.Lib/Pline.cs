namespace Data;

#region Interface IDraw -----------------------------------------------------------
public interface IDraw {
   void DrawPline (IReadOnlyList<Point> points);
}
#endregion

#region Struct Point --------------------------------------------------------------
public struct Point {    // Custom point struct with x and y co-ordinates
   #region Constructor ----------------------------------------------------------
   public Point (double x, double y) {
      this.x = x; this.y = y;
   }
   #endregion

   #region Methods --------------------------------------------------------------
   public double Dy (Point b) => Math.Abs (Y - b.Y);

   public double Dx (Point b) => Math.Abs (X - b.X);

   public double AngleTo (Point b) => Math.Atan2 (b.Y - Y, b.X - X);

   public double Distance (Point b) => Math.Sqrt (Math.Pow (b.X - X, 2) + Math.Pow (b.Y - Y, 2));
   #endregion

   #region Properties and fields ------------------------------------------------
   public double X { get { return x; } set { x = value; } }
   public double Y { get { return y; } set { y = value; } }
   double x;
   double y;
   #endregion
}
#endregion

# region Struct Bound --------------------------------------------------------------
public readonly struct Bound { // Bound in drawing space
   #region Constructors ---------------------------------------------------------
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

   #region Properties -----------------------------------------------------------
   public double MinX { get; init; }
   public double MaxX { get; init; }
   public double MinY { get; init; }
   public double MaxY { get; init; }
   public double Width => MaxX - MinX;
   public double Height => MaxY - MinY;
   public Point Mid => new ((MaxX + MinX) / 2, (MaxY + MinY) / 2);
   public bool IsEmpty => MinX > MaxX || MinY > MaxY;
   #endregion

   #region Method ---------------------------------------------------------------
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

#region Class Pline ---------------------------------------------------------------
public class Pline {
   #region Constructors ---------------------------------------------------------
   public Pline () { }
   public Pline (IEnumerable<Point> pts) => (mPts, Bound) = (pts.ToList (), new Bound (pts));
   #endregion

   #region Methods --------------------------------------------------------------
   public static Pline CreateLine (Point startPt, Point endPt) {
      return new Pline (Enum (startPt, endPt));

      static IEnumerable<Point> Enum (Point a, Point b) {
         yield return a;
         yield return b;
      }
   }

   public static Pline CreateRectangle (Point startCornerPt, Point endCornerPt) {
      var startCornerPt1 = new Point (endCornerPt.X, startCornerPt.Y);
      var endCornerPt1 = new Point (startCornerPt.X, endCornerPt.Y);
      return new Pline (Enum (startCornerPt, startCornerPt1, endCornerPt, endCornerPt1, startCornerPt));

      static IEnumerable<Point> Enum (Point a, Point b, Point c, Point d, Point e) {
         yield return a; yield return b; yield return c; yield return d; yield return e;
      }
   }

   public static Pline CreateConnectedLine (IReadOnlyList<Point> Points) => new (Points);

   public void Draw (IDraw draw) => draw.DrawPline (Points);

   public void Load (BinaryReader reader) {
      int count = reader.ReadInt32 ();
      for (int i = 0; i < count; i++) {
         (mPoint.X, mPoint.Y) = (reader.ReadDouble (), reader.ReadDouble ());
         mPts.Add (mPoint);
      }
   }

   public void Save (BinaryWriter writer) {
      writer.Write (Points.Count);
      foreach (var pline in Points) {
         writer.Write (pline.X); writer.Write (pline.Y);
      }
   }
   #endregion

   #region Properties -----------------------------------------------------------
   public Bound Bound { get; }
   public IReadOnlyList<Point> Points => mPts;
   #endregion

   #region Private Data ---------------------------------------------------------
   List<Point> mPts = new ();
   Point mPoint;
   #endregion
}
#endregion
