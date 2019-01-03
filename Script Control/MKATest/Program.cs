using System;
using System.Windows.Forms;

namespace MKATest
{
	public class MainClass
	{
        [STAThread]
        static void Main(string[] args) 
        {
            Application.Run(new CodeEditorForm());
        }
    }
}
