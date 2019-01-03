using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace AIMS.Libraries.Scripting.Engine
{
    
    public partial class Engine : Component
    {
        private Container components = null;

        #region Component Designer generated code

        public Engine()
        {
            
        }
        /// <param name="container"></param>
        
        public Engine(IContainer container)
            : this()
		{
			container.Add(this);
			InitializeComponent();
		}

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion


        //Identify if the caller is AIMS Engine and not AIMS IDE
        public bool IsRunMode
        {
            get
            {
                AppDomain app = AppDomain.CurrentDomain;
                if (app.FriendlyName == "AIMSEngine.vshost.exe")
                    return true;
                return false;
            }
        }

        private string _AssemblyName = "";
        public string OutputAssemblyName
        {
            get { return _AssemblyName; }
            set { _AssemblyName = value; }
        }

        private string _Namespace = "";
        public string DefaultNameSpace
        {
            get { return _Namespace; }
            set { _Namespace = value; }
        }

        private string _EntryMethod = "";
        public string StartMethodName
        {
            get { return _EntryMethod; }
            set { _EntryMethod = value; }
        }

        private string _DefaultClassName = "";
        public string DefaultClassName
        {
            get { return _DefaultClassName; }
            set { _DefaultClassName = value; }
        }
              

        private IDictionary<string, object> _RemoteVariables = null;
        public IDictionary<string, object> RemoteVariables
        {
            get { return _RemoteVariables; }
            set { _RemoteVariables = value; }
        }

        public object Execute(params object[] Parameters)
        {
            AppDomain secDom = null;
            bool isRunMode = this.IsRunMode;
            if(isRunMode)
                secDom = AppDomain.CurrentDomain;
            else
                secDom = AppDomain.CreateDomain("SecondaryDomain");
                

            // create the factory class in the secondary app-domain
            ScriptInstance sInstance = (ScriptInstance)secDom.CreateInstance("AIMS.Scripting.ScriptRun","ScriptInstance").Unwrap();
            // with the help of this factory, we can now create a real 'LiveClass' instance
            //If Not Compiled Then Compiled
            //In furture version compiled assemblies are stored as Blob in database alone with package

            IRun sRun;

            //sRun = sInstance.Create(secDom,  m_CompilerResults.CompiledAssembly,"IRun", Parameters);
            sRun = sInstance.Create(secDom, Path.GetFileName(_AssemblyName), _Namespace + "." + _DefaultClassName, Parameters);
            object RetObj = null;
            try
            {
                sRun.Initialize(_RemoteVariables);
                RetObj = sRun.Run(_EntryMethod, Parameters);
                sRun.Dispose(_RemoteVariables);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (!isRunMode)
                    AppDomain.Unload(secDom);
             
            }
            return RetObj;
        }
    }
}