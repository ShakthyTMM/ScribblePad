﻿using Data;
using System.IO;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CAD;

#region Class DocManager ----------------------------------------------------------
public class DocManager {
   #region Constructor -------------------------------------------------------------
   public DocManager (Editor editor) => mEditor = editor;
   #endregion

   #region Methods -----------------------------------------------------------------
   public bool Exit () {
      if (mEditor.IsModified) {
         Prompt ();
         if (!mEditor.IsCancelled) return true; else return false;
      } else return true;
   }

   public void New () {
      if (!mEditor.IsModified) mEditor.ClearScreen (); else if (mEditor.Shapes.Count != 0) Prompt ();
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
         writer.Write (mEditor.Shapes.Count); // Total number of shapes
         foreach (var shape in mEditor.Shapes) shape.Save (writer);
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
                  switch (reader.ReadInt32 ()) {    // Reads the file based on the shape (0 - rectangle, 1 - line, 2 - circle, 3- connected lines)
                     case 0: Read (reader, new Rectangle ()); break;
                     case 1: Read (reader, new Line ()); break;
                     case 2: Read (reader, new Circle ()); break;
                     case 3: Read (reader, new ConnectedLine ()); break;
                  }

                  void Read (BinaryReader reader, Shape shape) {    // Routine to read the shapes
                     shape.Open (reader);
                     mEditor.Shapes.Add (shape);
                  }
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
   Editor mEditor;
   string mPathToFile = "Untitled";
   #endregion
}
#endregion

