using CAD;
using Data;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CADMaster.UI.Widgets;

#region Class Rectangle -------------------------------------------------------------
public class Rectangle : Widget {
   #region Constructor ------------------------------------------------------------
   public Rectangle (Editor eventSource) : base (eventSource) {
      mEditor.Status = "Rectangle: Pick first corner of rectangle";
      mInputs = new string[] { "X", "Y", "Width", "Height" };
   }
   #endregion

   #region Methods ----------------------------------------------------------------
   protected override void OnMouseDown (object sender, MouseButtonEventArgs e) {
      base.OnMouseDown (sender, e);
      if (mEditor.IsDrawing) mEditor.CurrentShape = new Data.Rectangle (mStartPoint);
      mEditor.Status = "Rectangle: Pick first corner of rectangle";
   }

   protected override void OnMouseMove (object sender, MouseEventArgs e) {
      if (mEditor.IsDrawing) mEditor.Status = "Rectangle: Pick the second corner of rectangle";
      base.OnMouseMove (sender, e);
      Inputbox[2].Text = Math.Round (Width, 2).ToString ();
      Inputbox[3].Text = Math.Round (Height, 2).ToString ();
   }
   #endregion

   #region Properties ------------------------------------------------------------
   double Width => Math.Abs (EndPoint.X - StartPoint.X);
   double Height => Math.Abs (EndPoint.Y - StartPoint.Y);
   #endregion
}
#endregion

