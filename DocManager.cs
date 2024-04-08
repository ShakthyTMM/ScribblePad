using Data;
using ScribblePad;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Frontend;
public class DocManager {
   #region Constructor -----------------------------------------------------------
   public DocManager () { }
   public DocManager (Document document, MainWindow eventSource, string fileName) {
      mDocument = document; mEventSource = eventSource;
   }
   #endregion

   #region Methods ---------------------------------------------------------------
   public void Exit (object sender, RoutedEventArgs e) {    // Exit event's routine
      if (IsModified) {
         Prompt (sender, e);
         if (!IsCancelled) mEventSource.Close ();
      } else mEventSource.Close ();
   }

   public void Load (object sender, RoutedEventArgs e) {    // Open event's routine
      OpenFileDialog dlgBox = new () {
         Title = "Select a file",
         Filter = "Binary File|*.bin"
      };
      if (dlgBox.ShowDialog () == DialogResult.OK) {
         if (IsModified) Prompt (sender, e); else ClearScreen ();
         if (!IsCancelled)
            using (BinaryReader reader = new (File.Open (dlgBox.FileName, FileMode.Open))) {
               mIsSaved = true;
               mPathToFile = dlgBox.FileName;
               int totalCount = reader.ReadInt32 ();
               for (int i = 0; i < totalCount; i++) {
                  switch (reader.ReadInt32 ()) {    // Reads the file based on the shape (0 - rectangle, 1 - line, 2 - circle)
                     case 0: Read (reader, new Rectangle ()); break;
                     case 1: Read (reader, new Line ()); break;
                     case 2: Read (reader, new Circle ()); break;
                  }

                  void Read (BinaryReader reader, Shape shape) {    // Routine to read the shapes
                     shape.Open (reader);
                     mDocument.ShapesList.Add (shape);
                     mEventSource.InvalidateVisual ();
                  }
               }
            }
      }
   }

   // Prompts the user to save the drawing
   public void Prompt (object sender, RoutedEventArgs e) {
      IsCancelled = false;
      DialogResult dr = MessageBox.Show ("Do you want to save changes to Untitled?", "ScribblePad", MessageBoxButtons.YesNoCancel);
      switch (dr) {
         case DialogResult.Yes: SaveAs (sender); break;   // Triggering save event if pressed 'yes'
         case DialogResult.No: ClearScreen (); break;   // Clearing the drawing if pressed 'no'
         case DialogResult.Cancel: IsCancelled = true; return;
      }
   }

   public void New (object sender, RoutedEventArgs e) {    // New event's routine
      if (!IsModified) ClearScreen (); else Prompt (sender, e);
   }

   void ClearScreen () {
      mDocument.ShapesList.Clear (); mDocument.Stack.Clear ();
      mIsSaved = false; mEventSource.mUI.Opacity = 0.2; mEventSource.mRI.Opacity = 0.2;
      mEventSource.InvalidateVisual ();
   }

   public void SaveAs (object sender) {    // Save As event's routine
      SaveFileDialog dlgBox = new () {
         FileName = "Untitled",
         Filter = "Binary File|*.bin"
      };
      DialogResult dr;
      if (!mIsSaved && sender == null) {
         dr = DialogResult.OK;
         dlgBox.FileName = mPathToFile;
      } else dr = dlgBox.ShowDialog ();
      if (dr == DialogResult.OK) {
         mIsSaved = true;
         mPathToFile = dlgBox.FileName;
         using BinaryWriter writer = new (File.Open (dlgBox.FileName, FileMode.Create));
         writer.Write (mDocument.ShapesList.Count); // Total number of shapes
         foreach (var shape in mDocument.ShapesList) shape.Save (writer);
      }
   }
   #endregion

   #region Properties and fields -------------------------------------------------
   public bool IsModified => mDocument.ShapesList.Count > 0 && !mIsSaved;    // Checks whether the file is modified
   public bool mIsSaved = false;       // IsSaved checks if the file is saved and
   public bool IsCancelled = false;    // IsCancelled holds the state whether user cancelled the prompt
   #endregion

   #region Private Data ----------------------------------------------------------
   Document mDocument = new ();
   string mPathToFile;    // Holds the path of the file saved
   MainWindow mEventSource;
   #endregion
}
