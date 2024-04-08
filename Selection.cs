using Data;
using System;
using System.Windows;
using System.Windows.Input;
using Point = System.Windows.Point;

namespace ScribblePad;
public class Selection {
   #region Constructor -----------------------------------------------------------------
   public Selection (Document document, MainWindow eventSource) {
      mDocument = document;
      mEventSource = eventSource;
      Attach ();
   }
   #endregion

   #region Methods ---------------------------------------------------------------------
   void Attach () {
      mEventSource.MouseDown += OnMouseDown;
      mEventSource.del.Click += OnDelete;
   }

   public void Detach () {
      mEventSource.MouseDown -= OnMouseDown;
      mEventSource.del.Click -= OnDelete;
   }

   void OnMouseDown (object sender, MouseButtonEventArgs e) {
      mPoint = e.GetPosition (mEventSource);
      Shape currentShape = GetSelectedShape (mPoint);
      if (currentShape != null) {
         mEventSource.SelectedShape = currentShape;
         mEventSource.InvalidateVisual ();
      }
   }

   Shape GetSelectedShape (Point point) {
      foreach (var shape in mDocument.ShapesList) {
         mStartPoint = new (shape.StartPoint.X, shape.StartPoint.Y);
         mEndPoint = new (shape.EndPoint.X, shape.EndPoint.Y);
         switch (shape) {
            case Rectangle: {
                  Rect rect = new (mStartPoint, mEndPoint);
                  if (rect.Contains (point)) return shape;
                  break;
               }
            case Line: {
                  double m, y;
                  m = (mEndPoint.Y - mStartPoint.Y) / (mEndPoint.X - mStartPoint.X);
                  if (mEndPoint.X > mStartPoint.X) y = (m * point.X) - (m * mStartPoint.X) + mStartPoint.Y;
                  else y = (m * point.X) - (m * mEndPoint.X) + mEndPoint.Y;
                  if (Math.Abs (y - point.Y) < 10) return shape;
                  break;
               }
            case Circle: {
                  double radius = ((Circle)shape).Radius;
                  double distance = Math.Sqrt (Math.Pow (point.X - mStartPoint.X, 2) + Math.Pow (point.Y - mStartPoint.Y, 2));
                  if (Math.Abs (distance - radius) < 10) return shape;
                  break;
               }
            default: break;
         }
      }
      return null;
   }

   void OnDelete (object sender, RoutedEventArgs e) {
      mDocument.Check (sender);
      if (mEventSource.SelectedShape != null) {
         mDocument.ShapesList.Remove (mEventSource.SelectedShape);
         mDocument.Stack.Push (mEventSource.SelectedShape);
         mEventSource.SelectedShape = null;
      }
      mEventSource.InvalidateVisual ();
   }
   #endregion

   #region Private Data ----------------------------------------------------------------
   Document mDocument;
   MainWindow mEventSource;
   Point mPoint, mStartPoint, mEndPoint;
   #endregion
}

