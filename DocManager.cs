using Data;
using ScribblePad;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Frontend;
public class DocManager {
   #region Constructor -----------------------------------------------------------
   public DocManager (MainWindow eventSource) { mEventSource = eventSource; }
   #endregion

   #region Methods ---------------------------------------------------------------
   public void Exit (object sender, RoutedEventArgs e) {    // Exit event's routine
      if (IsSaved) {
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
         if (IsModified) Prompt (sender, e);
         if (IsSaved) { Events.ShapesList.Clear (); mEventSource.InvalidateVisual (); }
         if (!IsCancelled)
            using (BinaryReader reader = new (File.Open (dlgBox.FileName, FileMode.Open))) {
               IsSaved = true;
               mPathToFile = dlgBox.FileName;
               int totalCount = reader.ReadInt32 ();
               for (int i = 0; i < totalCount; i++) {
                  switch (reader.ReadInt32 ()) {    // Reads the file based on the shape (1- rectangle, 2- circle, 3- line)
                     case 1: Read (reader, new Rectangle ()); break;
                     case 2: Read (reader, new Line ()); break;
                     case 3: Read (reader, new Circle ()); break;

                  }

                  void Read (BinaryReader reader, Shape shape) {    // Routine to read the shapes
                     shape.Open (reader);
                     Events.ShapesList.Add (shape);
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
         case DialogResult.No:
            IsSaved = false; Events.ShapesList.Clear ();    // Clearing the drawing if pressed 'no'
            mEventSource.InvalidateVisual ();
            break;
         case DialogResult.Cancel: IsCancelled = true; return;
      }
   }

   public void New (object sender, RoutedEventArgs e) {    // New event's routine
      if (!IsModified) {
         Events.ShapesList.Clear ();
         IsSaved = false;
         mEventSource.InvalidateVisual ();
      } else Prompt (sender, e);
   }

   public void SaveAs (object sender) {    // Save As event's routine
      SaveFileDialog dlgBox = new () {
         FileName = "Untitled",
         Filter = "Binary File|*.bin"
      };
      DialogResult dr;
      if (IsSaved && sender == null) {
         dr = DialogResult.OK;
         dlgBox.FileName = mPathToFile;
      } else dr = dlgBox.ShowDialog ();
      if (dr == DialogResult.OK) {
         IsSaved = true;
         mPathToFile = dlgBox.FileName;
         using BinaryWriter writer = new (File.Open (dlgBox.FileName, FileMode.Create));
         writer.Write (Events.ShapesList.Count); // Total number of shapes
         foreach (var shape in Events.ShapesList) shape.Save (writer);
      }
   }
   #endregion

   #region Properties and fields -------------------------------------------------
   public bool IsModified => !IsSaved && Events.ShapesList.Count != 0;    // Checks whether the file is modified
   public bool IsSaved, IsCancelled = false;    // IsSaved checks if the file is saved and IsCancelled holds the state whether user cancelled the prompt
   #endregion

   #region Private fields --------------------------------------------------------
   string mPathToFile;    // Holds the path of the file saved
   readonly MainWindow mEventSource;
   #endregion
}
