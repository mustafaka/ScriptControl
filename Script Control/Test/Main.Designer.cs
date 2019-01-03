namespace Test
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.scriptControl1 = new AIMS.Libraries.Scripting.ScriptControl.ScriptControl();
            this.engine1 = new AIMS.Libraries.Scripting.Engine.Engine(this.components);
            this.SuspendLayout();
            // 
            // scriptControl1
            // 
            this.scriptControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptControl1.Location = new System.Drawing.Point(0, 0);
            this.scriptControl1.Name = "scriptControl1";
            this.scriptControl1.ScriptLanguage = AIMS.Libraries.Scripting.ScriptControl.ScriptLanguage.CSharp;
            this.scriptControl1.Size = new System.Drawing.Size(639, 428);
            this.scriptControl1.TabIndex = 0;
            this.scriptControl1.Load += new System.EventHandler(this.scriptControl1_Load);
            this.scriptControl1.Execute += new System.EventHandler(this.scriptControl1_Execute);
            // 
            // engine1
            // 
            this.engine1.DefaultClassName = "";
            this.engine1.DefaultNameSpace = "";
            this.engine1.OutputAssemblyName = "";
            this.engine1.RemoteVariables = null;
            this.engine1.StartMethodName = "";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 428);
            this.Controls.Add(this.scriptControl1);
            this.DoubleBuffered = true;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = ".Net Script Control VSA Replacement : Rajneesh Noonia";
            this.ResumeLayout(false);

        }

        #endregion

        private AIMS.Libraries.Scripting.ScriptControl.ScriptControl scriptControl1;
        private AIMS.Libraries.Scripting.Engine.Engine engine1;







    }
}

