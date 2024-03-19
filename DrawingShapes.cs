using Data;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace Frontend;
public static class DrawingShapes {    // Draws the shapes
   #region Implementation -----------------------------------------------------------------
   /// <summary>Draws the shape</summary>
   /// <param name="dc"></param>
   /// <param name="shape">Shape to be drawn</param>
   public static void Draw (DrawingContext dc, Shape shape) {
      sStartPoint = new (shape.StartPoint.X, shape.StartPoint.Y);
      sEndPoint = new (shape.EndPoint.X, shape.EndPoint.Y);
      switch (shape) {
         case Rectangle: DrawRectangle (dc); break;
         case Circle: DrawCircle (dc, (Circle)shape); break;
         case Line: DrawLine (dc); break;
      }
   }

   static void DrawRectangle (DrawingContext dc) {
      Rect rect = new (sStartPoint,sEndPoint);
      dc.DrawRectangle (null,sPen, rect);
   }

   static void DrawCircle (DrawingContext dc, Circle shape) =>
      dc.DrawEllipse (null,sPen, sStartPoint, shape.Radius, shape.Radius);

   static void DrawLine (DrawingContext dc) => dc.DrawLine (sPen, sStartPoint, sEndPoint);
   #endregion

   #region Private data ------------------------------------------------------------------------
   static Point sStartPoint, sEndPoint;
   static Pen sPen = new (Brushes.White, 2);
   #endregion
}

