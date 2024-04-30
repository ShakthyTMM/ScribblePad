using CAD;
using System;
using System.Windows.Input;

namespace CADMaster.UI.Widgets;

#region Class Circle ------------------------------------------------------------------
public class Circle : Widget {
   #region Constructor ------------------------------------------------------------
   public Circle (Editor eventSource) : base (eventSource) {
      mEditor.Status = "Circle: Pick center point";
      mInputs = new string[] { "X", "Y", "Radius" };
   }
   #endregion

   #region Methods ----------------------------------------------------------------
   protected override void OnMouseDown (object sender, MouseButtonEventArgs e) {
      base.OnMouseDown (sender, e);
      if (mEditor.IsDrawing) mEditor.CurrentShape = new Data.Circle (StartPoint);
      mEditor.Status = "Circle: Pick center point";
   }

   protected override void OnMouseMove (object sender, MouseEventArgs e) {
      base.OnMouseMove (sender, e);
      Inputbox[2].Text = Math.Round (Radius, 2).ToString ();
      if (mEditor.IsDrawing) mEditor.Status = "Circle: Pick endpoint";
   }
   #endregion

   #region Property ---------------------------------------------------------------
   double Radius => Math.Sqrt (Math.Pow (EndPoint.X - StartPoint.X, 2) + Math.Pow (EndPoint.Y - StartPoint.Y, 2));
   #endregion
}
#endregion