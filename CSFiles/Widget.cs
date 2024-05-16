using Data;
using System;
using System.Collections.Generic;
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
      mEditor.MouseLeftButtonDown += OnMouseDown;
      mEditor.MouseMove += OnMouseMove;
   }

   public void Detach () {
      mEditor.MouseLeftButtonDown -= OnMouseDown;
      mEditor.MouseMove -= OnMouseMove;
   }

   protected virtual void OnMouseDown (object sender, MouseButtonEventArgs e) {
      var startpt = mEditor.mInvProjXfm.Transform (e.GetPosition (mEditor));
      var pt = new Point (startpt.X, startpt.Y);
      AddInputs (pt);
      DrawingClicked (pt);
   }

   protected virtual void OnMouseMove (object sender, MouseEventArgs e) {
      if (mEditor.IsDrawing) {
         var hoverpt = mEditor.mInvProjXfm.Transform (e.GetPosition (mEditor));
         var pt = new Point (hoverpt.X, hoverpt.Y);
         AddInputs (pt);
         DrawingHover (pt);
      }
   }

   void AddInputs (Point point) { Inputbox[0].Text = Math.Round (point.X, 2).ToString (); Inputbox[1].Text = Math.Round (point.Y, 2).ToString (); }

   protected void GetInputs () {
      Inputbox[2].Text = Math.Round (Dx, 2).ToString ();
      Inputbox[3].Text = Math.Round (Dy, 2).ToString ();
      Inputbox[4].Text = Math.Round (Distance, 2).ToString ();
      Inputbox[5].Text = Math.Round (Angle, 2).ToString ();
   }

   void DrawingClicked (Point pt) {
      var pline = PointClicked (pt);
      if (pline != null) {
         mEditor.mDrawing.Add (pline);
         mEditor.EnableUndoRedo ();
         mEditor.InvalidateVisual ();
      }
   }

   void DrawingHover (Point pt) {
      PointHover (pt);
      mEditor.InvalidateVisual ();
   }

   #region Abstract and virtual methods -------------------------------------------------
   public abstract Pline? PointClicked (Point pt);

   public abstract void PointHover (Point pt);

   public abstract void Draw ();

   public virtual void AddLine () { }
   #endregion
   #endregion

   #region Properties and fields ---------------------------------------------------------
   protected Point StartPoint { get { if (mStartPt != null) return (Point)mStartPt; return new Point (); } }

   public Point EndPoint { get { if (mHoverpt != null) return (Point)mHoverpt; return new Point (); } }
   protected Point mEndPoint;

   public bool IsEndofCL { get; set; }

   public string[]? Inputs => mInputs;
   protected string[]? mInputs;

   public List<TextBox> Data { get => Inputbox; set => Inputbox = value; }
   protected List<TextBox> Inputbox = new ();

   double Dx => StartPoint.Dx (EndPoint); double Dy => StartPoint.Dy (EndPoint);

   double Distance => StartPoint.Distance (EndPoint);

   double Angle => StartPoint.AngleTo (EndPoint) * (180 / Math.PI);

   protected IReadOnlyList<Point> Points => mPoints;

   protected Editor mEditor;
   protected List<Point> mPoints = new ();
   protected Point? mStartPt, mHoverpt;
   #endregion
}
#endregion

#region Class DrawingShapes -----------------------------------------------------------
public class DrawingShapes : IDraw {
   #region Private Constructor -----------------------------------------------------------
   private DrawingShapes () { }
   #endregion

   #region Properties and fields ---------------------------------------------------------
   public static DrawingShapes GetInstance { get { mDS ??= new DrawingShapes (); return mDS; } }
   static DrawingShapes? mDS;

   public Pen Pen { set => mPen1 = value; }
   Pen? mPen1;

   public DrawingContext DrawingContext { set => mDC = value; }
   DrawingContext? mDC;

   public Matrix Xfm { get => mXfm; set => mXfm = value; }
   Matrix mXfm;
   #endregion

   #region Methods -----------------------------------------------------------------------
   void getPoints (Point startPoint, Point endPoint) {
      start = new (startPoint.X, startPoint.Y);
      end = new (endPoint.X, endPoint.Y);
      mStart = mXfm.Transform (start); mEnd = mXfm.Transform (end);
   }

   public void DrawPline (IReadOnlyList<Point> points) {
      for (int i = 0; i < points.Count - 1; i++) {
         getPoints (points[i], points[i + 1]);
         mDC?.DrawLine (mPen1, mStart, mEnd);
      }
   }
   #endregion

   #region Private Data ------------------------------------------------------------------
   System.Windows.Point mStart, mEnd, start, end;
   #endregion
}
#endregion

#region Class LineBuilder -------------------------------------------------------------
public class LineBuilder : Widget {

   #region Constructor -------------------------------------------------------------------
   public LineBuilder (Editor eventSource) : base (eventSource) {
      mEditor.Status = "Line: Pick start point of line";
      mInputs = new string[] { "X", "Y", "dX", "dY", "Length", "Angle" };
   }
   #endregion

   #region Methods -----------------------------------------------------------------------
   public override Pline? PointClicked (Point pt) {
      mEditor.Status = "Line: Pick start point of line";
      if (mStartPt is null) {
         mStartPt = pt;
         (mEditor.IsModified, mEditor.IsDrawing) = (true, true);
         return null;
      } else {
         var firstpt = mStartPt.Value;
         mEditor.IsDrawing = false;
         mStartPt = null;
         return Pline.CreateLine (firstpt, pt);
      }
   }

   public override void PointHover (Point pt) {
      if (mEditor.IsDrawing) mEditor.Status = "Line: Pick end point of line";
      mHoverpt = pt;
      GetInputs ();
   }

   public override void Draw () {
      if (mStartPt == null || mHoverpt == null) return;
      mPoints.Clear ();
      var ds = DrawingShapes.GetInstance;
      mPoints.Add ((Point)mStartPt); mPoints.Add ((Point)mHoverpt);
      ds.DrawPline (Points);
   }
   #endregion
}
#endregion

#region Class RectangleBuilder -------------------------------------------------------
public class RectangleBuilder : Widget {
   public RectangleBuilder (Editor eventSource) : base (eventSource) {
      mEditor.Status = "Rectangle: Pick first corner of rectangle";
      mInputs = new string[] { "X", "Y", "Width", "Height" };
   }

   #region Methods -----------------------------------------------------------------------
   public override Pline? PointClicked (Point pt) {
      mEditor.Status = "Rectangle: Pick first corner of rectangle";
      if (mStartPt is null) {
         mStartPt = pt;
         (mEditor.IsModified, mEditor.IsDrawing) = (true, true);
         return null;
      } else {
         var firstpt = mStartPt.Value;
         mStartPt = null;
         mEditor.IsDrawing = false;
         return Pline.CreateRectangle (firstpt, pt);
      }
   }

   public override void PointHover (Point pt) {
      mHoverpt = pt;
      if (mEditor.IsDrawing) mEditor.Status = "Rectangle: Pick the second corner of rectangle";
      Inputbox[2].Text = Math.Round (Width, 2).ToString ();
      Inputbox[3].Text = Math.Round (Height, 2).ToString ();
   }

   public override void Draw () {
      if (mStartPt == null || mHoverpt == null) return;
      mPoints.Clear ();
      var ds = DrawingShapes.GetInstance;
      mPoints.Add ((Point)mStartPt);
      var startPt1 = new Point (mHoverpt.Value.X, mStartPt.Value.Y);
      var endPt1 = new Point (mStartPt.Value.X, mHoverpt.Value.Y);
      mPoints.Add (startPt1); mPoints.Add ((Point)mHoverpt); mPoints.Add (endPt1);
      mPoints.Add ((Point)mStartPt);
      ds.DrawPline (Points);
   }
   #endregion

   #region Properties ------------------------------------------------------------
   double Width => Math.Abs (EndPoint.X - StartPoint.X);
   double Height => Math.Abs (EndPoint.Y - StartPoint.Y);
   #endregion
}
#endregion

#region Class ConnectedLine ----------------------------------------------------------
public class ConnectedlineBuilder : Widget {
   public ConnectedlineBuilder (Editor eventSource) : base (eventSource) {
      mEditor.Status = "Connected Line: Pick start point";
      mInputs = new string[] { "X", "Y", "dX", "dY", "Length", "Angle" };
   }

   public override Pline? PointClicked (Point pt) {
      mCLPoints.Add (pt);
      mStartPt = pt;
      (mEditor.IsModified, mEditor.IsDrawing) = (true, true);
      return null;
   }

   public override void PointHover (Point pt) {
      if (mEditor.IsDrawing) {
         mHoverpt = pt;
         GetInputs ();
         if (mEditor.IsDrawing) mEditor.Status = "Connected Line: Pick end point [Esc- To finish]";
      }
   }

   public override void AddLine () {
      if (mEditor.IsDrawing) {
         var pline = Pline.CreateConnectedLine (CLPoints);
         mEditor.Status = "Connected Line: Pick start point";
         mCLPoints.Clear (); mStartPt = null; mHoverpt = null;
         mEditor.mDrawing.Add (pline);
         mEditor.EnableUndoRedo ();
         mEditor.InvalidateVisual ();
         (mEditor.IsModified, mEditor.IsDrawing) = (true, false);
      }
   }

   public override void Draw () {
      var ds = DrawingShapes.GetInstance;
      if (mStartPt == null || mHoverpt == null) return;
      mPoints.Clear ();
      mPoints.Add ((Point)mStartPt); mPoints.Add ((Point)mHoverpt);
      ds.DrawPline (Points);
      ds.DrawPline (CLPoints);
   }

   public IReadOnlyList<Point> CLPoints => mCLPoints;
   List<Point> mCLPoints = new ();
}
#endregion


