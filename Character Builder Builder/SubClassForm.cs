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
    public partial class SubClassForm : Form, IMainEditor
    {
        public LinkedList<SubClass> UndoBuffer = new LinkedList<SubClass>();
        public LinkedList<SubClass> RedoBuffer = new LinkedList<SubClass>();
        private string lastid = null;
        private int UnsavedChanges;
        private static int MaxBuffer = 100;
        private SubClass cls;
        private bool doHistory = false;
        public SubClassForm(SubClass cls)
        {
            InitializeComponent();
            this.cls = cls;
            userControl11.Editor = this;
            refresh();
            features1.HistoryManager = this;
            decriptions1.HistoryManager = this;
            MulticlassSpellLevels.HistoryManager = this;
            featuresFirstClass.HistoryManager = this;
            featuresMultiClass.HistoryManager = this;
            ClassDefinition.ImportAll();
            foreach (string s in ClassDefinition.simple.Keys)
            {
                ParentClass.AutoCompleteCustomSource.Add(s);
                ParentClass.Items.Add(s);
            }

        }

        private void refresh()
        {
            bool oldHistory = doHistory;
            doHistory = false;
            name.DataBindings.Clear();
            name.DataBindings.Add("Text", cls, "Name", true, DataSourceUpdateMode.OnPropertyChanged);
            SheetName.DataBindings.Clear();
            SheetName.DataBindings.Add("Text", cls, "SheetName", true, DataSourceUpdateMode.OnPropertyChanged);
            flavour.DataBindings.Clear();
            NewlineFormatter.Add(flavour.DataBindings, "Text", cls, "Flavour", true, DataSourceUpdateMode.OnPropertyChanged);
            source.DataBindings.Clear();
            source.DataBindings.Add("Text", cls, "Source", true, DataSourceUpdateMode.OnPropertyChanged);
            description.DataBindings.Clear();
            NewlineFormatter.Add(description.DataBindings, "Text", cls, "Description", true, DataSourceUpdateMode.OnPropertyChanged);
            ParentClass.DataBindings.Clear();
            ParentClass.DataBindings.Add("Text", cls, "ClassName", true, DataSourceUpdateMode.OnPropertyChanged);
            features1.features = cls.Features;
            featuresFirstClass.features = cls.FirstClassFeatures;
            featuresMultiClass.features = cls.MulticlassingFeatures;
            decriptions1.descriptions = cls.Descriptions;
            preview.Navigate("about:blank");
            preview.Document.OpenNew(true);
            preview.Document.Write(cls.toHTML());
            preview.Refresh();
            source.AutoCompleteCustomSource.Clear();
            source.AutoCompleteCustomSource.AddRange(SourceManager.Sources.ToArray());
            MulticlassSpellLevels.Items = cls.MulticlassingSpellLevels;
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
            preview.Document.Write(cls.toHTML());
            preview.Refresh();
        }


        public void MakeHistory(string id)
        {
            if (!doHistory) return;
            if (id == null) id = "";
            if (id == "" || id != lastid)
            {
                UndoBuffer.AddLast(cls.clone());
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
                RedoBuffer.AddLast(cls);
                cls = UndoBuffer.Last.Value;
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
                UndoBuffer.AddLast(cls);
                cls = RedoBuffer.Last.Value;
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
            if (name.Text == null || name.Text.Length == 0)
            {
                MessageBox.Show("Unable to save without a name");
                return false;
            }
            bool saved = cls.save(false);
            if (!saved && MessageBox.Show("File exists! Overwrite?", "File exists", MessageBoxButtons.YesNo) == DialogResult.Yes) saved = cls.save(true);
            if (saved)
            {
                UnsavedChanges = 0;
                Saved?.Invoke(this, cls.Name + " " + ConfigManager.SourceSeperator + " " + cls.Source);
            }
            return saved;
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

        private void name_TextChanged(object sender, EventArgs e)
        {
            MakeHistory("name");
        }

        private void flavour_TextChanged(object sender, EventArgs e)
        {
            MakeHistory("flavour");
        }

        private void source_TextChanged(object sender, EventArgs e)
        {
            MakeHistory("Source");
        }

        private void description_TextChanged(object sender, EventArgs e)
        {
            MakeHistory("Desc");
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

        private void ParentClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            MakeHistory("Parent");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            MakeHistory("SheetName");
        }
    }
}
