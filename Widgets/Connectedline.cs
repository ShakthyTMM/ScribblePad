using CAD;
using Data;
using System;
using System.Windows.Input;
using Point = Data.Point;

namespace CADMaster.UI.Widgets;

#region Class Connectedline --------------------------------------------------------
public class Connectedline : Widget {
   #region Constructor ----------------------------------------------------------
   public Connectedline (Editor eventSource) : base (eventSource) {
      mEditor.Status = "Connected Line: Pick start point";
      mInputs = new string[] { "X", "Y", "dX", "dY", "Length", "Angle" };
   }
   #endregion

   #region Methods --------------------------------------------------------------
   protected override void OnMouseDown (object sender, MouseButtonEventArgs e) {
      if (e.RightButton == MouseButtonState.Released)
         if (mEditor.IsDrawing) {
            var mEnd = e.GetPosition (mEditor);
            var end = mEditor.mInvProjXfm.Transform (mEnd); mEndPoint = new Point (end.X, end.Y);
            mEditor.CurrentShape.Update (mEndPoint);
            mEditor.CurrentShape.AddLines (mEndPoint);
         } else {
            var mStart = e.GetPosition (mEditor);
            var start = mEditor.mInvProjXfm.Transform (mStart); mStartPoint = new Point (start.X, start.Y);
            mEditor.CurrentShape = new ConnectedLine (mStartPoint);
            (mEditor.IsDrawing, mEditor.IsModified) = (true, true);
         }
   }

   protected override void OnMouseMove (object sender, MouseEventArgs e) {
      base.OnMouseMove (sender, e);
      Inputbox[2].Text = Math.Round (Dx, 2).ToString ();
      Inputbox[3].Text = Math.Round (Dy, 2).ToString ();
      Inputbox[4].Text = Math.Round (Distance, 2).ToString ();
      Inputbox[5].Text = Math.Round (Angle, 2).ToString ();
      if (mEditor.IsDrawing) mEditor.Status = "Connected Line: Pick end point [Esc- To finish]";
   }

   public override void AddShapes () {
      mEditor.CurrentShape.RemoveLine ();
      mEditor.Status = "Connected Line: Pick start point";
      base.AddShapes ();
   }
   #endregion

   #region Properties -----------------------------------------------------------
   double Dx => Math.Abs (StartPoint.X - EndPoint.X); double Dy => Math.Abs (StartPoint.Y - EndPoint.Y);
   double Distance => Math.Sqrt (Math.Pow (EndPoint.X - StartPoint.X, 2) + Math.Pow (EndPoint.Y - StartPoint.Y, 2));
   double Angle => Math.Atan2 (StartPoint.X - EndPoint.X, StartPoint.Y - EndPoint.Y) * (180 / Math.PI);
   #endregion
}
#endregion
