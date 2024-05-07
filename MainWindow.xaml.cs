// ----------------------------------------------------------------------------------------
// Training 
// Copyright (c) Metamation India.
// ----------------------------------------------------------------------------------------
// MainWindow.xaml.cs
// Scribble Pad
// To design scribble pad
// ----------------------------------------------------------------------------------------
using CADMaster.UI.Widgets;
using Bound = Data.Bound;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CAD;

#region Class MainWindow ----------------------------------------------------------
public partial class MainWindow : Window {
   #region Constructor ------------------------------------------------------------
   public MainWindow () {
      InitializeComponent ();
      mPanWidget = new PanWidget (mCanvas, OnPan);
      mDocManager = new (mCanvas.Drawing, mCanvas);
      Title = SetTitle;
      Loaded += delegate {
         var bound = new Bound (new Data.Point (-10, -10), new Data.Point (100, 100));
         mCanvas.mProjXfm = Util.GetComputedMatrix (mCanvas.ActualWidth, mCanvas.ActualHeight, bound);
         mCanvas.mInvProjXfm = mCanvas.mProjXfm; mCanvas.mInvProjXfm.Invert ();
         mCanvas.Xfm = mCanvas.mProjXfm;
      };
   }
   #endregion

   #region Implementation ---------------------------------------------------------
   #region Tools ------------------------------------------------------------------
   void OnModeChange (object sender, RoutedEventArgs e) {
      mWidget?.Detach ();
      foreach (var control in mWP.Children) {
         if (control is ToggleButton tb && sender is ToggleButton clicked_tb) {
            if (tb == clicked_tb) {
               tb.IsChecked = true;
               switch (clicked_tb.ToolTip) {
                  case "Line": mWidget = new Line (mCanvas); break;
                  case "Rectangle": mWidget = new Rectangle (mCanvas); break;
                  case "ConnectedLine": mWidget = new Connectedline (mCanvas); break;
               }
               GetInputs ();
               mWidget.Attach ();
            } else tb.IsChecked = false;
         }
      }
   }

   void OnEsc (object sender, KeyEventArgs e) { if (e.Key == Key.Escape) mWidget.AddShapes (); }

   void OnUndo_Click (object sender, RoutedEventArgs e) => mCanvas.Undo ();

   void OnRedo_Click (object sender, RoutedEventArgs e) => mCanvas.Redo ();

   void CanExecute_Redo (object sender, CanExecuteRoutedEventArgs e) {
      if (mCanvas != null) {
         e.CanExecute = mCanvas.CanExecute_Redo;
         if (e.CanExecute) mRI.Opacity = 1; else mRI.Opacity = 0.2;
      }
   }

   void CanExecute_Undo (object sender, CanExecuteRoutedEventArgs e) {
      if (mCanvas != null) {
         e.CanExecute = mCanvas.CanExecute_Undo;
         if (e.CanExecute) mUI.Opacity = 1; else mUI.Opacity = 0.2;
      }
   }

   void OnKeyDown (object sender, KeyEventArgs e) {
      if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.D4) Exit ();
      if (Keyboard.Modifiers == ModifierKeys.Control) {
         switch (e.Key) {
            case Key.N: New (); break;
            case Key.O: Load (); break;
            case Key.S: Save (); break;
            case Key.Z: mCanvas.Undo (); ; break;
            case Key.Y: mCanvas.Redo (); break;
            default: break;
         }
      }
   }

   void GetInputs () {
      mWidget.Data.Clear ();
      InputBar.Children.Clear ();
      var inputs = mWidget.Inputs;
      foreach (var input in inputs) {
         InputBar.Children.Add (new TextBlock { Text = input, Margin = new Thickness (10, 3, 0, 0), FontWeight = FontWeights.Bold });
         var tb = new TextBox { Height = 20, Width = 40, Margin = new Thickness (10, 0, 0, 0) };
         InputBar.Children.Add (tb);
         mWidget.Data.Add (tb);
      }
   }
   #endregion

   #region Menu Items -------------------------------------------------------------
   void OnNew_Click (object sender, RoutedEventArgs e) => New ();

   void OnExit_Click (object sender, RoutedEventArgs e) => Exit ();

   void OnLoad_Click (object sender, RoutedEventArgs e) => Load ();

   void OnSave_Click (object sender, RoutedEventArgs e) => Save ();

   void Save () { mDocManager.Save (); Title = SetTitle; }

   void Load () { mDocManager.Load (); mCanvas.InvalidateVisual (); Title = SetTitle; }

   void Exit () { if (mDocManager.Exit ()) Close (); }

   void New () { mDocManager.New (); Title = SetTitle; }
   #endregion

   protected override void OnClosing (CancelEventArgs e) {
      if (mDocManager.Exit ()) base.OnClosing (e);
      else e.Cancel = true;
   }

   void OnPan (Vector panDisp) {
      Matrix m = Matrix.Identity; m.Translate (panDisp.X, panDisp.Y);
      mCanvas.mProjXfm.Append (m);
      mCanvas.mInvProjXfm = mCanvas.mProjXfm; mCanvas.mInvProjXfm.Invert ();
      mCanvas.Xfm = mCanvas.mProjXfm;
      mCanvas.InvalidateVisual ();
   }
   #endregion

   #region Property ---------------------------------------------------------------
   string SetTitle => mDocManager.FileName + " - CADMaster";
   #endregion

   #region Private data -----------------------------------------------------------
   Widget mWidget;
   PanWidget mPanWidget;
   DocManager mDocManager;
   #endregion
}
#endregion

#region Class Commands ------------------------------------------------------------
///<summary>Implements RoutedCommand class from ICommand interface</summary>
public static class Commands {
   public static readonly RoutedCommand Undo = new ();
   public static readonly RoutedCommand Redo = new ();
}
#endregion
