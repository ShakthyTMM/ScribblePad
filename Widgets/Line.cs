using CAD;
using System;
using System.Windows.Input;

namespace CADMaster.UI.Widgets;

#region Class Line -----------------------------------------------------------------
public class Line : Widget {
   #region Constructor -----------------------------------------------------------
   public Line (Editor eventSource) : base (eventSource) {
      mEditor.Status = "Line: Pick start point of line";
      mInputs = new string[] { "X", "Y", "dX", "dY", "Length", "Angle" };
   }
   #endregion

   #region Methods ---------------------------------------------------------------
   protected override void OnMouseDown (object sender, MouseButtonEventArgs e) {
      base.OnMouseDown (sender, e);
      mEditor.Status = "Line: Pick start point of line";
      if (mEditor.IsDrawing) mEditor.CurrentShape = new Data.Line (StartPoint);
   }

   protected override void OnMouseMove (object sender, MouseEventArgs e) {
      base.OnMouseMove (sender, e);
      Inputbox[2].Text = Math.Round (Dx, 2).ToString ();
      Inputbox[3].Text = Math.Round (Dy, 2).ToString ();
      Inputbox[4].Text = Math.Round (Distance, 2).ToString ();
      Inputbox[5].Text = Math.Round (Angle, 2).ToString ();
      if (mEditor.IsDrawing) mEditor.Status = "Line: Pick end point of line";
   }
   #endregion

   #region Properties ------------------------------------------------------------
   double Dx => Math.Abs (StartPoint.X - EndPoint.X); double Dy => Math.Abs (StartPoint.Y - EndPoint.Y);
   double Distance => Math.Sqrt (Math.Pow (EndPoint.X - StartPoint.X, 2) + Math.Pow (EndPoint.Y - StartPoint.Y, 2));
   double Angle => Math.Atan2 (StartPoint.X - EndPoint.X, StartPoint.Y - EndPoint.Y) * (180 / Math.PI);
   #endregion
}
#endregion


