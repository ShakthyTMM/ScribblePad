// ----------------------------------------------------------------------------------------
// Training 
// Copyright (c) Metamation India.
// ----------------------------------------------------------------------------------------
// MainWindow.xaml.cs
// Scribble Pad
// To design scribble pad
// ----------------------------------------------------------------------------------------
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Data;
using Frontend;

namespace ScribblePad;
public partial class MainWindow : Window {
   #region Constructor ------------------------------------------------------------
   public MainWindow () {
      InitializeComponent ();
      mDocument = new Document (this);
   }
   #endregion

   #region Implementation ---------------------------------------------------------
   #region Tools ------------------------------------------------------------------
   /// <summary>Click event for redo button</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnRedo (object sender, RoutedEventArgs e) => mDocument.Redo ();

   /// <summary>Click event for undo button</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnUndo (object sender, RoutedEventArgs e) => mDocument.Undo ();

   /// <summary>Determines whether the redo command is executable</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void CanExecute_Redo (object sender, CanExecuteRoutedEventArgs e) => mDocument.CanExecute_redo (sender, e);

   /// <summary>Determines whether the undo command is executable</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void CanExecute_Undo (object sender, CanExecuteRoutedEventArgs e) => mDocument.CanExecute_undo (sender, e);

   /// <summary>Event handler for shape buttons</summary>
   /// <param name="sender"></param>
   /// <param name="args"></param>
   void OnModeChanged (object sender, RoutedEventArgs args) => mDocument.GetSelectedShape (sender);

   /// <summary>Event handler for escape key press</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnKeyDown (object sender, KeyEventArgs e) {
      if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.D4) OnExit (sender, e);
      if (Keyboard.Modifiers == ModifierKeys.Control) {
         switch (e.Key) {
            case Key.N: OnNew (sender, e); break;
            case Key.O: OnOpen (sender, e); break;
            case Key.S: OnSave (sender, e); break;
            case Key.Z: OnUndo (sender, e); break;
            case Key.Y: OnRedo (sender, e); break;
            default: break;
         }
      }
   }

   void OnSelect (object sender, RoutedEventArgs e) => mDocument.Select (sender, e);
   #endregion

   #region Menu Items -------------------------------------------------------------
   /// <summary>Click event for exit menu item</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnExit (object sender, RoutedEventArgs e) => mDocument.Exit (sender, e);

   /// <summary>Click event of new menu item</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnNew (object sender, RoutedEventArgs e) => mDocument.New (sender, e);

   /// <summary>Click event of open menu item</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnOpen (object sender, RoutedEventArgs e) => mDocument.Load (sender, e);

   /// <summary>Click event for save menu item</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnSave (object sender, RoutedEventArgs e) => mDocument.Save (sender, e);

   /// <summary>Click event for save as menu item</summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   void OnSaveAs (object sender, RoutedEventArgs e) => mDocument.Save (sender, e);
   #endregion

   /// <summary>Overriding on-closing method of main window</summary>
   /// <param name="e"></param>
   protected override void OnClosing (CancelEventArgs e) {
      if (mDocManager.IsModified) {
         mDocManager.Prompt (null, null);
         if (!mDocManager.IsCancelled) base.OnClosing (e); else e.Cancel = true;
      } else base.OnClosing (e);
   }

   /// <summary>Overriding on-render method</summary>
   /// <param name="dc"></param>
   protected override void OnRender (DrawingContext dc) {
      base.OnRender (dc);
      mDocument.DrawShapes (dc);
   }
   #endregion

   #region Fields -----------------------------------------------------------------
   public Shape SelectedShape;
   #endregion

   #region Private data -----------------------------------------------------------
   Document mDocument = new ();
   DocManager mDocManager = new ();
   #endregion
}

/// <summary>Implements RoutedCommand class from ICommand interface</summary>
public static class Commands {
   public static readonly RoutedCommand Undo = new ();
   public static readonly RoutedCommand Redo = new ();
}
