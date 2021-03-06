﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Character_Builder_5;
using System.IO;

namespace Character_Builder_Builder
{
    public partial class ToolForm : Form, IMainEditor
    {
        public static LinkedList<Tool> UndoBuffer = new LinkedList<Tool>();
        public static LinkedList<Tool> RedoBuffer = new LinkedList<Tool>();
        private string lastid = null;
        private int UnsavedChanges;
        private static int MaxBuffer = 100;
        private Tool Tool;
        private bool doHistory = false;
        public ToolForm(Tool container)
        {
            InitializeComponent();
            this.Tool = container;
            userControl11.Editor = this;
            basicItem1.HistoryManager = this;
            refresh();
        }

        private void refresh()
        {
            bool oldHistory = doHistory;
            doHistory = false;
            basicItem1.Item = Tool;
            preview.Navigate("about:blank");
            preview.Document.OpenNew(true);
            preview.Document.Write(Tool.toHTML());
            preview.Refresh();
            onChange();
            doHistory = oldHistory;
        }



        private void showPreview(object sender, EventArgs e)
        {
            ShowPreview();
        }

        public void ShowPreview()
        {
            preview.Navigate("about:blank");
            preview.Document.OpenNew(true);
            preview.Document.Write(Tool.toHTML());
            preview.Refresh();
        }


        public void MakeHistory(string id)
        {
            if (!doHistory) return;
            if (id == null) id = "";
            if (id == "" || id != lastid)
            {
                UndoBuffer.AddLast((Tool)Tool.clone());
                RedoBuffer.Clear();
                onChange();
                if (UndoBuffer.Count > MaxBuffer) UndoBuffer.RemoveFirst();
                UnsavedChanges++;
            }
            lastid = id;
        }
        public bool Undo()
        {
            if (UndoBuffer.Count > 0)
            {
                lastid = "";
                RedoBuffer.AddLast(Tool);
                Tool = UndoBuffer.Last.Value;
                UndoBuffer.RemoveLast();
                if (UnsavedChanges > 0) UnsavedChanges--;
                refresh();
                return true;
            }
            return false;
        }
        public bool Redo()
        {
            if (RedoBuffer.Count > 0)
            {
                lastid = "";
                UndoBuffer.AddLast(Tool);
                Tool = RedoBuffer.Last.Value;
                RedoBuffer.RemoveLast();
                UnsavedChanges++;
                refresh();
                return true;
            }
            return false;
        }
        public bool CanUndo()
        {
            return UndoBuffer.Count > 0;
        }
        public bool CanRedo()
        {
            return RedoBuffer.Count > 0;
        }

        public event HistoryButtonChangeEvent ButtonChange;

        private void onChange()
        {
            ButtonChange?.Invoke(this, CanUndo(), CanRedo());
        }

        public event SavedEvent Saved;
        public bool Save()
        {
            if (Tool.Name == null || Tool.Name.Length == 0)
            {
                MessageBox.Show("Unable to save without a name");
                return false;
            }
            try
            {
                bool saved = Tool.save(false);
                if (!saved && MessageBox.Show("File exists! Overwrite?", "File exists", MessageBoxButtons.YesNo) == DialogResult.Yes) saved = Tool.save(true);
                if (saved)
                {
                    UnsavedChanges = 0;
                    Saved?.Invoke(this, Tool.Name + " " + ConfigManager.SourceSeperator + " " + Tool.Source);
                }
                return saved;
            } catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
        public void Exit()
        {
            this.Close();    
        }

        private bool checkClose()
        {
            if (UnsavedChanges > 0 && MessageBox.Show("There are " + UnsavedChanges + " unsaved changes? Do you want to discard them?", "Unsaved Changes", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return false;
            }
            return true;
        }

        private void RaceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!checkClose()) e.Cancel = true;
        }

        private void save_Click(object sender, EventArgs e)
        {
            if (Save()) Exit();
        }

        private void abort_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void RaceForm_Shown(object sender, EventArgs e)
        {
            doHistory = true;
        }
    }
}
