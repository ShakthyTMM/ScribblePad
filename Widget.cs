using Data;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Point = Data.Point;

namespace CAD;

#region Class Widget ------------------------------------------------------------------
public abstract class Widget {
   #region Constructor -------------------------------------------------------------------
   public Widget (Editor eventSource) { mEditor = eventSource; }
   #endregion

   #region Methods -----------------------------------------------------------------------
   public void Attach () {
      mEditor.MouseDown += OnMouseDown;
      mEditor.MouseMove += OnMouseMove;
   }

   public void Detach () {
      mEditor.MouseDown -= OnMouseDown;
      mEditor.MouseMove -= OnMouseMove;
   }

   protected virtual void OnMouseDown (object sender, MouseButtonEventArgs e) {
      if (e.RightButton == MouseButtonState.Released) {
         if (mEditor.IsDrawing) {
            mEndPoint.X = e.GetPosition (mEditor).X; mEndPoint.Y = e.GetPosition (mEditor).Y;
            AddInputs (mEndPoint);
            mEditor.CurrentShape.Update (mEndPoint);
            AddShapes ();
         } else {
            mStartPoint.X = e.GetPosition (mEditor).X; mStartPoint.Y = e.GetPosition (mEditor).Y;
            AddInputs (mStartPoint);
            (mEditor.IsDrawing, mEditor.IsModified) = (true, true);
         }
      }
   }

   public virtual void AddShapes () => mEditor.AddShapes ();

   protected virtual void OnMouseMove (object sender, MouseEventArgs e) {
      if (mEditor.IsDrawing) {
         mEndPoint.X = e.GetPosition (mEditor).X; mEndPoint.Y = e.GetPosition (mEditor).Y;
         AddInputs (mEndPoint);
         mEditor.CurrentShape.Update (mEndPoint);
         mEditor.InvalidateVisual ();
      }
   }

   void AddInputs (Point point) { Inputbox[0].Text = Math.Round (point.X, 2).ToString (); Inputbox[1].Text = Math.Round (point.Y, 2).ToString (); }
   #endregion

   #region Properties and fields ---------------------------------------------------------

   public Point StartPoint { get => mStartPoint; set => mStartPoint = value; }
   protected Point mStartPoint;

   public Point EndPoint { get => mEndPoint; set => mEndPoint = value; }
   protected Point mEndPoint;

   public string[] Inputs => mInputs;
   protected string[] mInputs;

   public List<TextBox> Data { get => Inputbox; set => Inputbox = value; }
   protected List<TextBox> Inputbox = new ();

   protected Editor mEditor;
   #endregion
}
#endregion

#region Class DrawingShapes -----------------------------------------------------------
public class DrawingShapes : IDraw {
   #region Private Constructor -----------------------------------------------------------
   private DrawingShapes () { }
   #endregion

   #region Properties --------------------------------------------------------------------
   public static DrawingShapes GetInstance { get { mDS ??= new DrawingShapes (); return mDS; } }
   static DrawingShapes mDS;

   public Pen Pen { set => mPen1 = value; }
   Pen mPen1;

   public DrawingContext DrawingContext { set => mDC = value; }
   DrawingContext mDC;
   #endregion

   #region Methods -----------------------------------------------------------------------
   public void DrawLine (Point startPoint, Point endPoint) {
      getPoints (startPoint, endPoint);
      mDC.DrawLine (mPen1, mStart, mEnd);
   }

   public void DrawRectangle (Point startPoint, Point endPoint) {
      getPoints (startPoint, endPoint);
      Rect rect = new (mStart, mEnd);
      mDC.DrawRectangle (null, mPen1, rect);
   }

   public void DrawCL (List<Line> lines, Point startPoint, Point endPoint) {
      foreach (var line in lines) {
         getPoints (line.StartPoint, line.EndPoint);
         mDC.DrawLine (mPen, mStart, mEnd);
      }
      getPoints (startPoint, endPoint);
      mDC.DrawLine (mPen1, mStart, mEnd);
   }

   public void DrawCircle (Point startPoint, double radius) {
      mStart = new (startPoint.X, startPoint.Y);
      mDC.DrawEllipse (null, mPen1, mStart, radius, radius);
   }

   void getPoints (Point startPoint, Point endPoint) {
      mStart = new (startPoint.X, startPoint.Y);
      mEnd = new (endPoint.X, endPoint.Y);
   }
   #endregion

   #region Private Data ------------------------------------------------------------------
   Pen mPen = new (Brushes.Black, 1);
   System.Windows.Point mStart, mEnd;
   #endregion
}
#endregion
