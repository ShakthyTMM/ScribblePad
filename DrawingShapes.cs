using Data;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace Frontend;
public static class DrawingShapes {    // Draws the shapes
   #region Methods -----------------------------------------------------------------
   public static void Draw (DrawingContext dc, Shape shape, Pen pen) {
      sStartPoint = new (shape.StartPoint.X, shape.StartPoint.Y);
      sEndPoint = new (shape.EndPoint.X, shape.EndPoint.Y);
      switch (shape) {
         case Rectangle: DrawRectangle (dc, pen); break;
         case Circle: DrawCircle (dc, (Circle)shape, pen); break;
         case Line: DrawLine (dc, pen); break;
      }
   }

   static void DrawRectangle (DrawingContext dc, Pen pen) {
      Rect rect = new (sStartPoint, sEndPoint);
      dc.DrawRectangle (null, pen, rect);
   }

   static void DrawCircle (DrawingContext dc, Circle shape, Pen pen) =>
      dc.DrawEllipse (null, pen, sStartPoint, shape.Radius, shape.Radius);

   static void DrawLine (DrawingContext dc, Pen pen) => dc.DrawLine (pen, sStartPoint, sEndPoint);
   #endregion

   #region Private data ------------------------------------------------------------------------
   static Point sStartPoint, sEndPoint;
   #endregion
}

