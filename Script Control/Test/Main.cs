using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AIMS.Libraries.Scripting;
using System.CodeDom.Compiler;
namespace Test
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            scriptControl1.AddObject("Container", this);
            scriptControl1.StartEditor();
            scriptControl1.Execute += new EventHandler(scriptControl1_Execute);
        }

        void scriptControl1_Execute(object sender, EventArgs e)
        {
            try
            {
                engine1.OutputAssemblyName = scriptControl1.OutputAssemblyName;
                engine1.StartMethodName = scriptControl1.StartMethodName;
                engine1.DefaultNameSpace = scriptControl1.DefaultNameSpace;
                engine1.RemoteVariables = scriptControl1.RemoteVariables;
                engine1.DefaultClassName = scriptControl1.DefaultClassName;
                object ret = engine1.Execute(null);
                MessageBox.Show("Return Code : " + ret.ToString());
            }
            catch
            {

            }
        }

        private void scriptControl1_Load(object sender, EventArgs e)
        {

        }

       
    }
}