using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

//using ICSharpCode.Core;
using AIMS.Libraries.Scripting.ScriptControl.Project;

namespace AIMS.Libraries.Scripting.ScriptControl.ReferenceDialog
{
	public interface IReferencePanel
	{
		void AddReference();
	}
	
	public interface ISelectReferenceDialog
	{
		void AddReference(ReferenceType referenceType, string referenceName, string referenceLocation, object tag);
	}
	
	public enum ReferenceType {
		Assembly,
		Typelib,
		Gac,
		
		Project
	}
	
	/// <summary>
	/// Summary description for Form2.
	/// </summary>
	public class SelectReferenceDialog : System.Windows.Forms.Form, ISelectReferenceDialog
	{
		private System.Windows.Forms.ListView referencesListView;
		private System.Windows.Forms.Button selectButton;
		private System.Windows.Forms.Button removeButton;
		private System.Windows.Forms.TabPage gacTabPage;
		private System.Windows.Forms.TabPage webTabPage;
		private System.Windows.Forms.TabPage browserTabPage;
        private System.Windows.Forms.TabPage comTabPage;
		private System.Windows.Forms.Label referencesLabel;
		private System.Windows.Forms.ColumnHeader referenceHeader;
		private System.Windows.Forms.ColumnHeader typeHeader;
		private System.Windows.Forms.ColumnHeader locationHeader;
		private System.Windows.Forms.TabControl referenceTabControl;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button helpButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		IProject configureProject;
		

		public ArrayList ReferenceInformations {
			get {
				ArrayList referenceInformations = new ArrayList();
				foreach (ListViewItem item in referencesListView.Items) {
					System.Diagnostics.Debug.Assert(item.Tag != null);
					referenceInformations.Add(item.Tag);
				}
				return referenceInformations;
			}
		}

        public IProject ConfigureProject
        {
            get { return this.configureProject; }
            set {
                referencesListView.Items.Clear();
                    this.configureProject = value; 
                }
        }
		public SelectReferenceDialog(IProject configureProject)
		{
			this.configureProject = configureProject;
			
			InitializeComponent();
			gacTabPage.Controls.Add(new GacReferencePanel(this));
			browserTabPage.Controls.Add(new AssemblyReferencePanel(this));
			comTabPage.Controls.Add(new COMReferencePanel(this));
		}
		
		public void AddReference(ReferenceType referenceType, string referenceName, string referenceLocation, object tag)
		{
			foreach (ListViewItem item in referencesListView.Items) {
				if (referenceLocation == item.SubItems[2].Text && referenceName == item.Text ) {
					return;
				}
			}
			
			ListViewItem newItem = new ListViewItem(new string[] {referenceName, referenceType.ToString(), referenceLocation});
			switch (referenceType) {
				case ReferenceType.Typelib:
					newItem.Tag = new ComReferenceProjectItem(configureProject, (TypeLibrary)tag);
					break;
				case ReferenceType.Project:
					newItem.Tag = new ProjectReferenceProjectItem(configureProject, (IProject)tag);
					break;
				case ReferenceType.Gac:
					newItem.Tag = new ReferenceProjectItem(configureProject, referenceLocation);
					break;
				case ReferenceType.Assembly:
					ReferenceProjectItem assemblyReference = new ReferenceProjectItem(configureProject);
					assemblyReference.Include = Path.GetFileNameWithoutExtension(referenceLocation);
					assemblyReference.HintPath = FileUtility.GetRelativePath(configureProject.Directory, referenceLocation);
					assemblyReference.SpecificVersion = false;
					newItem.Tag = assemblyReference;
					break;
				default:
					throw new System.NotSupportedException("Unknown reference type:" + referenceType);
			}
			
			referencesListView.Items.Add(newItem);
		}
		
		void SelectReference(object sender, EventArgs e)
		{
			IReferencePanel refPanel = (IReferencePanel)referenceTabControl.SelectedTab.Controls[0];
			refPanel.AddReference();
		}
		
		void OkButtonClick(object sender, EventArgs e)
		{
			if (referencesListView.Items.Count == 0) {
				SelectReference(sender, e);
			}
		}
		
		void RemoveReference(object sender, EventArgs e)
		{
			ArrayList itemsToDelete = new ArrayList();
			
			foreach (ListViewItem item in referencesListView.SelectedItems) {
				itemsToDelete.Add(item);
			}
			
			foreach (ListViewItem item in itemsToDelete) {
				referencesListView.Items.Remove(item);
			}
		}
		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.referenceTabControl = new System.Windows.Forms.TabControl();
            this.gacTabPage = new System.Windows.Forms.TabPage();
            this.comTabPage = new System.Windows.Forms.TabPage();
            this.webTabPage = new System.Windows.Forms.TabPage();
            this.browserTabPage = new System.Windows.Forms.TabPage();
            this.referencesListView = new System.Windows.Forms.ListView();
            this.referenceHeader = new System.Windows.Forms.ColumnHeader();
            this.typeHeader = new System.Windows.Forms.ColumnHeader();
            this.locationHeader = new System.Windows.Forms.ColumnHeader();
            this.selectButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.referencesLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.referenceTabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // referenceTabControl
            // 
            this.referenceTabControl.Controls.Add(this.gacTabPage);
            this.referenceTabControl.Controls.Add(this.comTabPage);
            this.referenceTabControl.Controls.Add(this.webTabPage);
            this.referenceTabControl.Controls.Add(this.browserTabPage);
            this.referenceTabControl.Location = new System.Drawing.Point(8, 8);
            this.referenceTabControl.Name = "referenceTabControl";
            this.referenceTabControl.SelectedIndex = 0;
            this.referenceTabControl.Size = new System.Drawing.Size(472, 224);
            this.referenceTabControl.TabIndex = 0;
            // 
            // gacTabPage
            // 
            this.gacTabPage.Location = new System.Drawing.Point(4, 22);
            this.gacTabPage.Name = "gacTabPage";
            this.gacTabPage.Size = new System.Drawing.Size(464, 198);
            this.gacTabPage.TabIndex = 0;
            this.gacTabPage.Text = ".Net";
            this.gacTabPage.UseVisualStyleBackColor = true;
            // 
            // comTabPage
            // 
            this.comTabPage.Location = new System.Drawing.Point(4, 22);
            this.comTabPage.Name = "comTabPage";
            this.comTabPage.Size = new System.Drawing.Size(464, 198);
            this.comTabPage.TabIndex = 2;
            this.comTabPage.Text = "COM";
            this.comTabPage.UseVisualStyleBackColor = true;
            // 
            // webTabPage
            // 
            this.webTabPage.Location = new System.Drawing.Point(4, 22);
            this.webTabPage.Name = "webTabPage";
            this.webTabPage.Size = new System.Drawing.Size(464, 198);
            this.webTabPage.TabIndex = 1;
            this.webTabPage.Text = "Web";
            this.webTabPage.UseVisualStyleBackColor = true;
            // 
            // browserTabPage
            // 
            this.browserTabPage.Location = new System.Drawing.Point(4, 22);
            this.browserTabPage.Name = "browserTabPage";
            this.browserTabPage.Size = new System.Drawing.Size(464, 198);
            this.browserTabPage.TabIndex = 2;
            this.browserTabPage.Text = "Browse";
            this.browserTabPage.UseVisualStyleBackColor = true;
            // 
            // referencesListView
            // 
            this.referencesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.referenceHeader,
            this.typeHeader,
            this.locationHeader});
            this.referencesListView.FullRowSelect = true;
            this.referencesListView.Location = new System.Drawing.Point(8, 256);
            this.referencesListView.Name = "referencesListView";
            this.referencesListView.Size = new System.Drawing.Size(472, 97);
            this.referencesListView.TabIndex = 3;
            this.referencesListView.UseCompatibleStateImageBehavior = false;
            this.referencesListView.View = System.Windows.Forms.View.Details;
            // 
            // referenceHeader
            // 
            this.referenceHeader.Text = "Reference";
            this.referenceHeader.Width = 183;
            // 
            // typeHeader
            // 
            this.typeHeader.Text = "Type";
            this.typeHeader.Width = 57;
            // 
            // locationHeader
            // 
            this.locationHeader.Text = "Location";
            this.locationHeader.Width = 228;
            // 
            // selectButton
            // 
            this.selectButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.selectButton.Location = new System.Drawing.Point(488, 32);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(75, 23);
            this.selectButton.TabIndex = 1;
            this.selectButton.Text = "Select";
            this.selectButton.Click += new System.EventHandler(this.SelectReference);
            // 
            // removeButton
            // 
            this.removeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.removeButton.Location = new System.Drawing.Point(488, 256);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(75, 23);
            this.removeButton.TabIndex = 4;
            this.removeButton.Text = "Remove";
            this.removeButton.Click += new System.EventHandler(this.RemoveReference);
            // 
            // referencesLabel
            // 
            this.referencesLabel.Location = new System.Drawing.Point(8, 240);
            this.referencesLabel.Name = "referencesLabel";
            this.referencesLabel.Size = new System.Drawing.Size(472, 16);
            this.referencesLabel.TabIndex = 2;
            this.referencesLabel.Text = "References";
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(312, 368);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "Ok";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(400, 368);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            // 
            // helpButton
            // 
            this.helpButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.helpButton.Location = new System.Drawing.Point(488, 368);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 7;
            this.helpButton.Text = "Help";
            // 
            // SelectReferenceDialog
            // 
            this.AcceptButton = this.okButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(570, 399);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.referencesLabel);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.selectButton);
            this.Controls.Add(this.referencesListView);
            this.Controls.Add(this.referenceTabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectReferenceDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add Reference to AIMS script";
            this.referenceTabControl.ResumeLayout(false);
            this.ResumeLayout(false);

		}
	}
}
