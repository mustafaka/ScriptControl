using System;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using AIMS.Libraries.CodeEditor.SyntaxFiles;

namespace MKATest
{
    public class CodeEditorForm : Form
	{
        private bool _dataChanged;
        private string _caption;

        protected bool _isNew { get; private set; }

        private StatusStrip statusBar;
        private ToolStripStatusLabel cursorLine;
        private ToolStripStatusLabel cursorCol;
        private ToolStripMenuItem mnuEdit;
        private ToolStripMenuItem mnuFind;
        private ToolStripMenuItem mnuFindNext;
        private ToolStripMenuItem mnuSave;
        private ToolStripMenuItem mnuClose;
        protected AIMS.Libraries.CodeEditor.CodeEditorControl txtText;
        private IContainer components;
        private AIMS.Libraries.CodeEditor.Syntax.SyntaxDocument syntaxDocument1;
        private ToolStripMenuItem mnuReplace;
        private ToolStripMenuItem mnuSelectAll;

        protected void SetDataChanged(bool val)
        {
            _dataChanged = val;
            this.Text = (_isNew ? "New " : "Edit ") + _caption + (_dataChanged ? " *" : "");
        }

        public CodeEditorForm()
		{
            _caption = "Script";
            _isNew = false;
            InitializeComponent();

            SetLanguage(SyntaxLanguage.CSharp);
            txtText.Document.Text = Properties.Resources.SampleCode;

            SetDataChanged(false);
        }

        protected void SetLanguage(SyntaxLanguage syntaxLanguage)
        {
            CodeEditorSyntaxLoader.SetSyntax(txtText, syntaxLanguage);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripStatusLabel statusLabel1;
            System.Windows.Forms.ToolStripStatusLabel statusLabel2;
            System.Windows.Forms.MenuStrip menuStrip;
            System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
            System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
            AIMS.Libraries.CodeEditor.WinForms.LineMarginRender lineMarginRender2 = new AIMS.Libraries.CodeEditor.WinForms.LineMarginRender();
            this.mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFind = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuReplace = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFindNext = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuClose = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.cursorLine = new System.Windows.Forms.ToolStripStatusLabel();
            this.cursorCol = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtText = new AIMS.Libraries.CodeEditor.CodeEditorControl();
            this.syntaxDocument1 = new AIMS.Libraries.CodeEditor.Syntax.SyntaxDocument(this.components);
            statusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            statusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            menuStrip = new System.Windows.Forms.MenuStrip();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            menuStrip.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusLabel1
            // 
            statusLabel1.Name = "statusLabel1";
            statusLabel1.Size = new System.Drawing.Size(32, 17);
            statusLabel1.Text = "Line:";
            statusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusLabel2
            // 
            statusLabel2.Name = "statusLabel2";
            statusLabel2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            statusLabel2.Size = new System.Drawing.Size(45, 17);
            statusLabel2.Text = "Char:";
            statusLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // menuStrip
            // 
            menuStrip.BackColor = System.Drawing.SystemColors.Window;
            menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuEdit});
            menuStrip.Location = new System.Drawing.Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new System.Drawing.Size(1230, 24);
            menuStrip.TabIndex = 1004;
            menuStrip.Text = "menuStrip1";
            // 
            // mnuEdit
            // 
            this.mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFind,
            this.mnuReplace,
            this.mnuFindNext,
            toolStripMenuItem1,
            this.mnuSelectAll,
            toolStripMenuItem2,
            this.mnuSave,
            this.mnuClose});
            this.mnuEdit.Name = "mnuEdit";
            this.mnuEdit.Size = new System.Drawing.Size(39, 20);
            this.mnuEdit.Text = "&Edit";
            // 
            // mnuFind
            // 
            this.mnuFind.Name = "mnuFind";
            this.mnuFind.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.mnuFind.Size = new System.Drawing.Size(167, 22);
            this.mnuFind.Text = "&Find...";
            this.mnuFind.Click += new System.EventHandler(this.mnuFind_Click);
            // 
            // mnuReplace
            // 
            this.mnuReplace.Name = "mnuReplace";
            this.mnuReplace.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.mnuReplace.Size = new System.Drawing.Size(167, 22);
            this.mnuReplace.Text = "&Replace...";
            this.mnuReplace.Click += new System.EventHandler(this.mnuReplace_Click);
            // 
            // mnuFindNext
            // 
            this.mnuFindNext.Name = "mnuFindNext";
            this.mnuFindNext.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.mnuFindNext.Size = new System.Drawing.Size(167, 22);
            this.mnuFindNext.Text = "Find &Next";
            this.mnuFindNext.Click += new System.EventHandler(this.mnuFindNext_Click);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(164, 6);
            // 
            // mnuSelectAll
            // 
            this.mnuSelectAll.Name = "mnuSelectAll";
            this.mnuSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.mnuSelectAll.Size = new System.Drawing.Size(167, 22);
            this.mnuSelectAll.Text = "Select &All";
            this.mnuSelectAll.Click += new System.EventHandler(this.mnuSelectAll_Click);
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(164, 6);
            // 
            // mnuSave
            // 
            this.mnuSave.Name = "mnuSave";
            this.mnuSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.mnuSave.Size = new System.Drawing.Size(167, 22);
            this.mnuSave.Text = "&Save";
            this.mnuSave.Click += new System.EventHandler(this.mnuSave_Click);
            // 
            // mnuClose
            // 
            this.mnuClose.Name = "mnuClose";
            this.mnuClose.Size = new System.Drawing.Size(167, 22);
            this.mnuClose.Text = "&Close";
            this.mnuClose.Click += new System.EventHandler(this.mnuClose_Click);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            statusLabel1,
            this.cursorLine,
            statusLabel2,
            this.cursorCol});
            this.statusBar.Location = new System.Drawing.Point(0, 713);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(1230, 22);
            this.statusBar.TabIndex = 1003;
            this.statusBar.Text = "statusStrip1";
            // 
            // cursorLine
            // 
            this.cursorLine.Name = "cursorLine";
            this.cursorLine.Size = new System.Drawing.Size(13, 17);
            this.cursorLine.Text = "0";
            this.cursorLine.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cursorCol
            // 
            this.cursorCol.Name = "cursorCol";
            this.cursorCol.Size = new System.Drawing.Size(13, 17);
            this.cursorCol.Text = "0";
            this.cursorCol.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtText
            // 
            this.txtText.ActiveView = AIMS.Libraries.CodeEditor.WinForms.ActiveView.BottomRight;
            this.txtText.AutoListPosition = null;
            this.txtText.AutoListSelectedText = "";
            this.txtText.AutoListVisible = false;
            this.txtText.CopyAsRTF = false;
            this.txtText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtText.Document = this.syntaxDocument1;
            this.txtText.FileName = null;
            this.txtText.InfoTipCount = 1;
            this.txtText.InfoTipPosition = null;
            this.txtText.InfoTipSelectedIndex = 1;
            this.txtText.InfoTipVisible = false;
            lineMarginRender2.Bounds = new System.Drawing.Rectangle(19, 0, 19, 16);
            this.txtText.LineMarginRender = lineMarginRender2;
            this.txtText.Location = new System.Drawing.Point(0, 52);
            this.txtText.LockCursorUpdate = false;
            this.txtText.Name = "txtText";
            this.txtText.Saved = false;
            this.txtText.ShowScopeIndicator = false;
            this.txtText.Size = new System.Drawing.Size(1230, 661);
            this.txtText.SmoothScroll = false;
            this.txtText.SplitviewH = -4;
            this.txtText.SplitviewV = -4;
            this.txtText.TabGuideColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(233)))), ((int)(((byte)(233)))));
            this.txtText.TabIndex = 1005;
            this.txtText.Text = "codeEditorControl1";
            this.txtText.WhitespaceColor = System.Drawing.SystemColors.ControlDark;
            this.txtText.CaretChange += new System.EventHandler(this.txtText_CaretChange);
            // 
            // syntaxDocument1
            // 
            this.syntaxDocument1.Lines = new string[] {
        ""};
            this.syntaxDocument1.MaxUndoBufferSize = 1000;
            this.syntaxDocument1.Modified = false;
            this.syntaxDocument1.UndoStep = 0;
            // 
            // TextEditorFormBase
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1230, 735);
            this.Controls.Add(this.txtText);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(menuStrip);
            this.MinimizeBox = false;
            this.Name = "TextEditorFormBase";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Script Editor";
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
		#endregion

        protected virtual void Save()
        {
            SetDataChanged(false);
        }

        private void txtText_TextChanged(object sender, System.EventArgs e)
        {
            SetDataChanged(true);
        }

        private void mnuSelectAll_Click(object sender, EventArgs e)
        {
            txtText.SelectAll();
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void txtText_CaretChange(object sender, EventArgs e)
        {
            cursorCol.Text = txtText.Caret.Position.X.ToString();
            cursorLine.Text = txtText.Caret.Position.Y.ToString();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtText.Focus();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = !CloseApproved();
        }

        private bool CloseApproved()
        {
            if (_dataChanged)
            {
                var ret = MessageBox.Show("Record was changed. Save before close?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (ret == DialogResult.Cancel)
                    return false;
                if (ret == DialogResult.Yes)
                    Save();
            }
            return true;
        }

        private void mnuClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mnuFind_Click(object sender, EventArgs e)
        {
            txtText.ActiveViewControl.ShowFind();
        }

        private void mnuFindNext_Click(object sender, EventArgs e)
        {
            txtText.ActiveViewControl.FindNext();
        }

        private void mnuReplace_Click(object sender, EventArgs e)
        {
            txtText.ActiveViewControl.ShowReplace();
        }
    }
}