using Data;
using System.IO;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CAD;

#region Class DocManager ----------------------------------------------------------
public class DocManager {
   #region Constructor -------------------------------------------------------------
   public DocManager (Drawing drawing, Editor editor) => (mDrawing, mEditor) = (drawing, editor);
   #endregion

   #region Methods -----------------------------------------------------------------
   public bool Exit () {
      if (mEditor.IsModified) {
         Prompt ();
         if (!mEditor.IsCancelled) return true; else return false;
      } else return true;
   }

   public void New () {
      if (!mEditor.IsModified) mEditor.ClearScreen (); else if (mDrawing.Plines.Count != 0) Prompt ();
      mPathToFile = "Untitled";
   }

   public void Save () {
      SaveFileDialog dlgBox = new () {
         FileName = "Untitled",
         Filter = "Binary File|*.bin"
      };
      DialogResult dr;
      if (mEditor.IsSaved) {
         dr = DialogResult.OK;
         dlgBox.FileName = mPathToFile;
      } else dr = dlgBox.ShowDialog ();
      if (dr == DialogResult.OK) {
         mEditor.IsModified = false; mEditor.IsSaved = true;
         mPathToFile = dlgBox.FileName;
         using BinaryWriter writer = new (File.Open (dlgBox.FileName, FileMode.Create));
         writer.Write (mDrawing.Plines.Count); // Total number of pline
         foreach (var pline in mDrawing.Plines) pline.Save (writer);
      }
   }

   public void Load () {
      OpenFileDialog dlgBox = new () {
         Title = "Select a file",
         Filter = "Binary File|*.bin"
      };
      if (dlgBox.ShowDialog () == DialogResult.OK) {
         if (mEditor.IsModified) Prompt ();
         if (!mEditor.IsCancelled)
            using (BinaryReader reader = new (File.Open (dlgBox.FileName, FileMode.Open))) {
               mEditor.IsModified = false; mEditor.IsSaved = true;
               mPathToFile = dlgBox.FileName;
               int totalCount = reader.ReadInt32 ();
               for (int i = 0; i < totalCount; i++) {
                  var pline = new Pline ();
                  pline.Load (reader);
                  mDrawing.Plines.Add (pline);
               }
            }
      }
   }

   // Prompts the user to save the drawing
   public void Prompt () {
      mEditor.IsCancelled = false;
      DialogResult dr = MessageBox.Show ($"Do you want to save changes to {FileName}?", "CADMaster", MessageBoxButtons.YesNoCancel);
      switch (dr) {
         case DialogResult.Yes: Save (); break;   // Triggering save event if pressed 'yes'
         case DialogResult.No: mEditor.ClearScreen (); break;   // Clearing the drawing if pressed 'no'
         case DialogResult.Cancel: mEditor.IsCancelled = true; return;
      }
   }
   #endregion

   #region Property ----------------------------------------------------------------
   public string FileName => Path.GetFileNameWithoutExtension (mPathToFile);
   #endregion

   #region Private Data ------------------------------------------------------------
   Drawing mDrawing;
   Editor mEditor;
   string mPathToFile = "Untitled";
   #endregion
}
#endregion

