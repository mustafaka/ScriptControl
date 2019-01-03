using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Windows.Forms;
using AIMS.Libraries.CodeEditor.SyntaxFiles;
using AIMS.Libraries.CodeEditor;
using AIMS.Libraries.CodeEditor.WinForms;
using AIMS.Libraries.CodeEditor.Syntax;
using System.Threading;
using System.IO;
using AIMS.Libraries.Scripting.NRefactory;
using AIMS.Libraries.Scripting.Dom.NRefactoryResolver;
using AIMS.Libraries.Scripting.Dom;
using AIMS.Libraries.Scripting.Dom.CSharp;
using AIMS.Libraries.Scripting.Dom.VBNet;
using AIMS.Libraries.Scripting.CodeCompletion;
using AIMS.Libraries.Scripting.ScriptControl.CodeCompletion;
using AIMS.Libraries.Scripting.ScriptControl.Project;
using AIMS.Libraries.Scripting.ScriptControl.ReferenceDialog;
using AIMS.Libraries.Forms.Docking;
using AIMS.Libraries.Scripting.ScriptControl;
namespace AIMS.Libraries.Scripting.ScriptControl
{
    public partial class ScriptControl : UserControl
    {
        private ScriptLanguage _scriptLanguage;
        private ErrorList _winErrorList = null;
        private Output _winOutput = null;
        private ProjectExplorer _winProjExplorer = null;
        private static IProject m_AIMSProject = null;
        private WeakReference m_SelectRefDialog = null;
        private IDictionary<string, object> _RemoteVariables=null;
        public event EventHandler Execute;

        protected virtual void OnExecute()
        {
            if (Execute != null)
            {
                Execute(this, null);
            }
        }

        public ScriptLanguage ScriptLanguage
        {
            get { return _scriptLanguage; }
            set
            {
                if (value == ScriptLanguage.CSharp)
                {
                    this.tsbSelectLanguage.Image = global::AIMS.Libraries.Scripting.ScriptControl.Properties.Resources.VSProject_CSCodefile;
                    this.tsbSelectLanguage.ImageTransparentColor = System.Drawing.Color.Magenta;
                }
                else
                {
                    this.tsbSelectLanguage.Image = global::AIMS.Libraries.Scripting.ScriptControl.Properties.Resources.VSProject_VBCodefile;
                    this.tsbSelectLanguage.ImageTransparentColor = System.Drawing.Color.Magenta;
                }
                ConvertToLanguage(_scriptLanguage, value);
                _scriptLanguage = value;
                
            }
        }

        private void ConvertToLanguage(ScriptLanguage OldLang, ScriptLanguage NewLanguage)
        {

            ResetParserLanguage(NewLanguage);
            //Disable On Change Event
            foreach (IDockableWindow docWin in dockContainer1.Contents)
            {
                Document doc = null;
                if (docWin is Document)
                {
                    doc = docWin as Document;
                    DocumentEvents(doc, false);
                    if(OldLang != NewLanguage)
                    {
                        doc.FileName = Path.GetFileNameWithoutExtension(doc.FileName) + (NewLanguage == ScriptLanguage.CSharp ? ".cs" : ".vb");
                        if (NewLanguage == ScriptLanguage.CSharp)
                        {
                            doc.ScriptLanguage = NewLanguage;
                            doc.Contents = Parser.ProjectParser.GetFileContents(doc.FileName);
                        }
                        else
                        {
                            doc.Contents = Parser.ProjectParser.GetFileContents(doc.FileName);
                            doc.ScriptLanguage = NewLanguage;
                        }
                    }

                    DocumentEvents(doc, true);
                }
            }
            //Enable On Change Event
            if(_winErrorList != null)
            _winErrorList.ConvertToLanguage(OldLang, NewLanguage);
            _winProjExplorer.Language = NewLanguage;
        }

        public IDictionary<string, object> RemoteVariables
        {
            get { return _RemoteVariables; }
        }
        public string DefaultNameSpace
        {
            get { return m_AIMSProject.RootNamespace; }
        }

        public string DefaultClassName
        {
            get { return "Program"; }
        }

        public string StartMethodName
        {
            get { return "Main"; }
        }

        private string GetUserSrcCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("namespace " + this.DefaultNameSpace);
            sb.AppendLine("{");
            sb.AppendLine("\tpublic partial class " + this.DefaultClassName);
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tpublic int " + this.StartMethodName + "()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t");
            sb.AppendLine("\t\t\treturn 0;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            return sb.ToString();
        }
        public string OutputAssemblyName
        {
            get { return m_AIMSProject.OutputAssemblyFullPath; }
        }
        private string GetAddObjectSrcCode()
        {
            //Add AddObject & ReturnCode stuff;
            StringBuilder sb = new StringBuilder();
            foreach (string key in _RemoteVariables.Keys)
            {
                object obj = null;
                if(_RemoteVariables.TryGetValue(key,out obj))
                {
                    sb.Append("\t\tpublic static ");
                    sb.Append(obj.GetType().BaseType.FullName);
                    sb.Append(" ");
                    sb.Append(key);
                    sb.Append(" = null;");
                    sb.AppendLine();
                 }
            }
            sb.AppendLine();
            return sb.ToString();
        }

        public void AddObject(string Name, object Value)
        {
            try
            {
                _RemoteVariables.Add(Name, Value);
            }
            catch (Exception Ex)
            {
                throw Ex;
            }

        }

        private string GetSystemSrcCode()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("#region System Generated Source Code.Please do not change ...");
                sb.AppendLine("namespace " + this.DefaultNameSpace);
                sb.AppendLine("{");
                sb.AppendLine("\tusing System;");
                sb.AppendLine("\tusing System.Collections.Generic;");
                sb.AppendLine("\tusing System.Diagnostics;");
                sb.AppendLine("\tusing System.Reflection;");
                sb.AppendLine("\tpublic partial class " + this.DefaultClassName +  " : MarshalByRefObject, IRun");
                sb.AppendLine("\t{");
                sb.AppendLine(GetAddObjectSrcCode());
                sb.AppendLine("\t\t[DebuggerStepperBoundary()]");
                sb.AppendLine("\t\tvoid IRun.Initialize(IDictionary<string, object> Variables)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\tforeach (string name in Variables.Keys)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tobject value = null;");
                sb.AppendLine("\t\t\t\ttry");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\tVariables.TryGetValue(name, out value);");
                sb.AppendLine("\t\t\t\t\tFieldInfo fInfo = this.GetType().GetField(name, BindingFlags.Public | BindingFlags.Static);");
                sb.AppendLine("\t\t\t\t\tfInfo.SetValue(this, value);");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\tcatch(Exception ex)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\tthrow ex;");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t}");
                sb.AppendLine("");
                sb.AppendLine("\t\t[DebuggerStepperBoundary()]");
                sb.AppendLine("\t\tobject IRun.Run(string StartMethod, params object[] Parameters)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\ttry");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tMethodInfo methodInfo = this.GetType().GetMethod(StartMethod,BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance);");
                sb.AppendLine("\t\t\t\treturn methodInfo.Invoke(this, Parameters);");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\tcatch (Exception ex)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tthrow ex;");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t}");
                sb.AppendLine("");
                sb.AppendLine("\t\t[DebuggerStepperBoundary()]");
                sb.AppendLine("\t\tvoid IRun.Dispose(IDictionary<string, object> Variables)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\tforeach (string name in Variables.Keys)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tobject value = null; ;");
                sb.AppendLine("\t\t\t\ttry");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\tFieldInfo fInfo = this.GetType().GetField(name, BindingFlags.Public | BindingFlags.Static);");
                sb.AppendLine("\t\t\t\t\tfInfo.SetValue(this, value);");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\tcatch (Exception ex)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\tthrow ex;");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                return sb.ToString();
            }


        public void StartEditor()
        {
            AddRefrence(new ReferenceProjectItem(m_AIMSProject,"System"));
            AddRefrence(new ReferenceProjectItem(m_AIMSProject, "System.Windows.Forms"));
            Parser.ProjectParser.ParseProjectContents("Program.Sys.cs",this.GetSystemSrcCode());
            Document doc = this.AddDocument("Program.cs");
            doc.Contents = this.GetUserSrcCode();
            doc.ParseContentsNow();
            doc.Editor.ActiveViewControl.Caret.Position = new TextPoint(0, 1);
        }

        public ScriptControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponent();
            InitilizeDocks();
            Parser.ProjectParser.Initilize(SupportedLanguage.CSharp);
            UpdateCutCopyToolbar();
            m_AIMSProject = new DefaultProject();
            _RemoteVariables = new Dictionary<string,object>();
        }

        private void ResetParserLanguage(ScriptLanguage lang)
        {
            if (lang == ScriptLanguage.CSharp)
                Parser.ProjectParser.Language = SupportedLanguage.CSharp;
            else
                Parser.ProjectParser.Language = SupportedLanguage.VBNet;
           
        }

        private void InitilizeDocks()
        {

            ////Error List
            _winErrorList = new ErrorList(this);
            _winErrorList.Text = "Error List";
            _winErrorList.Tag = "ERRORLIST";
            _winErrorList.HideOnClose = true;
            _winErrorList.Show(dockContainer1, DockState.DockBottomAutoHide);
            
            //ProjectExplorer
            _winProjExplorer = new ProjectExplorer();
            _winProjExplorer.Text = "Solution Explorer";
            _winProjExplorer.Tag = "SOLUTIONEXPLORER";
            _winProjExplorer.HideOnClose = true;
            _winProjExplorer.Show(dockContainer1, DockState.DockRightAutoHide);

            ////Output
            _winOutput = new Output();
            _winOutput.Text = "Output";
            _winOutput.Tag = "OUTPUT";
            _winOutput.HideOnClose = true;
            _winOutput.Show(dockContainer1, DockState.DockBottomAutoHide);

            dockContainer1.ActiveDocumentChanged += new EventHandler(dockContainer1_ActiveDocumentChanged);
            _winProjExplorer.FileClick += new EventHandler<ExplorerClickEventArgs>(_winProjExplorer_FileClick);
            _winProjExplorer.FileNameChanged += new EventHandler<ExplorerLabelEditEventArgs>(_winProjExplorer_FileNameChanged);
            _winProjExplorer.NewItemAdd += new EventHandler(_winProjExplorer_NewItemAdd);
            _winProjExplorer.FileItemDeleted += new EventHandler(_winProjExplorer_FileItemDeleted);
            _winErrorList.ItemDoubleClick += new EventHandler<ListViewItemEventArgs>(_winErrorList_ItemDoubleClick);
            _winProjExplorer.AddRefrenceItem += new EventHandler(_winProjExplorer_AddRefrenceItem);
            _winProjExplorer.AddWebRefrenceItem += new EventHandler(_winProjExplorer_AddWebRefrenceItem);
        }

        public void AddRefrence(ProjectItem Reference)
        {
            TreeNode refNode = _winProjExplorer.RefrenceNode;
            ArrayList list = new ArrayList();
            list.Add(Reference);
            ConvertCOM(null, list, refNode); 
        }

        void _winProjExplorer_AddWebRefrenceItem(object sender, EventArgs e)
        {
            TreeNode t = (TreeNode)sender;
            StringCollection files = new StringCollection();
            foreach (string Name in Parser.ProjectParser.ProjectFiles.Keys)
            {
                files.Add(Name);
            }
            using (AddWebReferenceDialog refDialog = new AddWebReferenceDialog(m_AIMSProject, _scriptLanguage, files))
            {
                refDialog.NamespacePrefix = m_AIMSProject.RootNamespace;
                if (refDialog.ShowDialog() == DialogResult.OK)
                {
                    // Do not overwrite existing web references.
                    refDialog.WebReference.Name = WebReference.GetReferenceName(refDialog.WebReference.WebReferencesDirectory, refDialog.WebReference.Name);
                    AddWebRefrenceToProject(t, refDialog.WebReference,refDialog.WebReferenceFileName);
                }
            }
            
        }

        void AddWebRefrenceToProject(TreeNode node, WebReference webref,string fileName)
        {

            Parser.ProjectParser.ParseProjectContents(fileName, webref.GetSourceCode(), false);
            _winProjExplorer.AddWebReference(fileName);
            ArrayList refItems = new ArrayList();
            foreach (ProjectItem item in webref.Items)
            {
                if (item is ReferenceProjectItem)
                {
                    refItems.Add(item);
                    
                }
            }
            if(refItems.Count >0)
                ConvertCOM(null, refItems, node);
        }

        void _winProjExplorer_AddRefrenceItem(object sender, EventArgs e)
        {
            TreeNode t = (TreeNode)sender;
            SelectReferenceDialog selDialog = null;
            if (m_SelectRefDialog == null)
            {
                m_SelectRefDialog = new WeakReference(null);
            }
            if (!m_SelectRefDialog.IsAlive)
            {
                m_SelectRefDialog.Target = new SelectReferenceDialog(m_AIMSProject);
            }

            selDialog = (SelectReferenceDialog) m_SelectRefDialog.Target;
            selDialog.ConfigureProject = m_AIMSProject;
            if (selDialog.ShowDialog(this.ParentForm) == DialogResult.OK)
            {
                ConvertCOM(null, selDialog.ReferenceInformations, t);
            }

            m_SelectRefDialog.Target = selDialog;
        }

        void ConvertCOM(object sender, ArrayList refrences,TreeNode node)
        {
            object[] param = new object[] { sender, (object)refrences, (object)node };
            ThreadPool.QueueUserWorkItem(new WaitCallback(ConvertCOMThread), (object)param);
        }

        void ConvertCOMThread(Object stateInfo)
        {
            object[] param = (object[])stateInfo;
            object sender = param[0];
            ArrayList refrences = param[1] as ArrayList;
            TreeNode node = param[2] as TreeNode;
            BeginInvoke(new MethodInvoker(delegate { tsMessage.Text = "Please wait..."; }));
            foreach (ReferenceProjectItem reference in refrences)
            {
                try
                {
                    if (reference.ItemType == ItemType.COMReference)
                    {
                        if (Path.IsPathRooted(reference.FileName))
                        {
                            m_AIMSProject.AddProjectItem(reference);
                        }
                        else
                        {
                            ArrayList addedRefs = ImportCom(reference as ComReferenceProjectItem);
                            foreach (ReferenceProjectItem refs in addedRefs)
                            {
                                m_AIMSProject.AddProjectItem(refs);
                                BeginInvoke(new MethodInvoker(delegate
                                {
                                    TreeNode refNode = node.Nodes.Add(refs.Name);
                                    refNode.ImageKey = "Reference.ico";
                                    refNode.Tag = NodeType.Reference;
                                }));
                            }
                            
                        }
                    }
                    else if (reference.ItemType == ItemType.Reference)
                    {
                        m_AIMSProject.AddProjectItem(reference);
                        BeginInvoke(new MethodInvoker(delegate
                        {
                            TreeNode refNode = node.Nodes.Add(reference.Name);
                            refNode.ImageKey = "Reference.ico";
                            refNode.Tag = NodeType.Reference;
                        }));
                    }
                    
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                BeginInvoke(new MethodInvoker(delegate { tsMessage.Text = "Ready"; }));
            }
        }

        private ArrayList ImportCom(ComReferenceProjectItem t)
        {
            ArrayList refrences = new ArrayList();
            refrences.Add(t as ReferenceProjectItem);
            BeginInvoke(new MethodInvoker(delegate { tsMessage.Text = "Compiling COM component '" + t.Include + "' ..." ; }));
            Converter.TlbImp importer = new Converter.TlbImp(refrences);
            importer.ReportEvent += new EventHandler<Converter.ReportEventEventArgs>(importer_ReportEvent);
            importer.ResolveRef += new EventHandler<Converter.ResolveRefEventArgs>(importer_ResolveRef);
            string outputFolder = Path.GetDirectoryName(m_AIMSProject.OutputAssemblyFullPath);
            string interopFileName = Path.Combine(outputFolder, String.Concat("Interop.", t.Include, ".dll"));
            string asmPath = interopFileName;
            importer.Import(asmPath,t.FilePath, t.Name);
            return refrences;
        }

        void importer_ResolveRef(object sender, Converter.ResolveRefEventArgs e)
        {
            BeginInvoke(new MethodInvoker(delegate { tsMessage.Text = e.Message; }));
            BeginInvoke(new MethodInvoker(delegate { _winOutput.AppendLine(e.Message); }));
        }

        void importer_ReportEvent(object sender, Converter.ReportEventEventArgs e)
        {
            string msg;
            msg =  Environment.NewLine + "COM Importer Event ..." + Environment.NewLine;
            msg += "Kind: " + e.EventKind.ToString() + Environment.NewLine;
            msg += "Code: " + e.EventCode + Environment.NewLine;
            msg += "Message: " + e.EventMsg;
            BeginInvoke(new MethodInvoker(delegate { tsMessage.Text = e.EventMsg; }));
            BeginInvoke(new MethodInvoker(delegate { _winOutput.AppendLine(msg); }));
        }


        void _winProjExplorer_FileItemDeleted(object sender, EventArgs e)
        {
            TreeNode node = (TreeNode)sender;
            Document doc = GetExistingFile(node.Text);

            if (doc != null)
            {
                doc.Close();
            }
            node.Remove();

            Parser.ProjectParser.RemoveContentFile(node.Text);
            
        }

        void _winProjExplorer_NewItemAdd(object sender, EventArgs e)
        {
            AddNewItem();
        }

        void _winProjExplorer_FileNameChanged(object sender, ExplorerLabelEditEventArgs e)
        {
            if(ValidateFileName(e.NewName))
            {
                string contents = Parser.ProjectParser.GetFileContents(e.OldName);
                Parser.ProjectParser.ProjectFiles.Remove(e.OldName);
                Parser.ProjectParser.ParseProjectContents(e.NewName, contents);
                Document doc = GetExistingFile(e.OldName);
                if (doc != null)
                {
                    doc.Text = Path.GetFileNameWithoutExtension(e.NewName);
                    doc.FileName = e.NewName;
                }
            }
            else{
                MessageBox.Show("File Name '" + e.NewName + "' already exists in the project.Please try other name.");
                e.Cancel=true;
            }

        }

        private bool ValidateFileName(string fileName)
        {   
            string[] keys = new string[Parser.ProjectParser.ProjectFiles.Keys.Count];
            Parser.ProjectParser.ProjectFiles.Keys.CopyTo(keys, 0);
            
            for (int count = 0; count <= keys.Length-1; count++)
            {
                if (keys[count].ToLower() == fileName.ToLower())
                    return false;
            }
            return true;
        }

        void _winErrorList_ItemDoubleClick(object sender, ListViewItemEventArgs e)
        {
           System.Windows.Forms.Timer tmr = new System.Windows.Forms.Timer();
           tmr.Tick += new EventHandler(ShowIntoView);
           tmr.Interval = 500;
           Document doc =  ShowFile(e.FileName);
           tmr.Tag = e; 
           tmr.Start();
            
        }

        void ShowIntoView(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer tmr = ( System.Windows.Forms.Timer)sender;
            tmr.Stop();
            ListViewItemEventArgs et = (ListViewItemEventArgs)tmr.Tag;
            Document doc = ShowFile(et.FileName);
            TextPoint t = new TextPoint(et.ColumnNo, et.LineNo);
            doc.Editor.ScrollIntoView(t);
            doc.Editor.Caret.SetPos(t);
        }

        Document GetExistingFile(string FileName)
        {
            Document doc = null;
            foreach (IDockableWindow docWin in dockContainer1.Contents)
            {

                if (docWin is Document)
                {
                    doc = docWin as Document;
                    if (doc.FileName == FileName)
                    {
                        return doc;
                    }
                }
            }
            return null;
        }

        public Document ShowFile(string FileName)
        {
            Document doc = null;
            foreach (IDockableWindow docWin in dockContainer1.Contents)
            {

                if (docWin is Document)
                {
                    doc = docWin as Document;
                    if (doc.FileName == FileName)
                    {
                        doc.Show(dockContainer1, DockState.Document);
                        return doc;
                    }
                }
            }
            // Not Found
            
            doc = OpenDocument(FileName);
            if (doc != null)
            {
                doc.Activate();
                doc.Focus();
            }
            return doc;

        }

        void _winProjExplorer_FileClick(object sender, ExplorerClickEventArgs e)
        {
            Document doc =  ShowFile(e.FileName);
            if(doc!= null) doc.ParseContentsNow();
        }

        void dockContainer1_ActiveDocumentChanged(object sender, EventArgs e)
        {
            if (dockContainer1.ActiveDocument is Document)
            {
                Document doc = dockContainer1.ActiveDocument as Document;
                _winProjExplorer.ActiveNode(doc.FileName);
            }
            UpdateCutCopyToolbar();
        }

        public Document AddDocument(string Name)
        {
            return  AddDocument(Name,false);
        }

        public Document AddDocument(string Name,bool IsWebReference)
        {
            Document doc = new Document(this);
            doc.FileName = Name;
            doc.Text = Path.GetFileNameWithoutExtension(Name);
            doc.Tag = "USERDOCUMENT";
            doc.HideOnClose = false;
            doc.ScriptLanguage = _scriptLanguage;
            DocumentEvents(doc,true);
            doc.Show(dockContainer1,DockState.Document );
            if(IsWebReference)
                _winProjExplorer.AddWebReference(Name);
            else
                _winProjExplorer.AddFile(Name);
            Parser.ProjectParser.ParseProjectContents(Name, "");
            return doc;
        }

        public  Document OpenDocument(string Name)
        {
            string contents = Parser.ProjectParser.GetFileContents(Name);
            if (contents == string.Empty)
                return null;
            Document doc = new Document(this);
            doc.FileName = Name;
            doc.Text = Path.GetFileNameWithoutExtension(Name);
            doc.Tag = "USERDOCUMENT";
            doc.HideOnClose = false;
            doc.ScriptLanguage = _scriptLanguage;
            
            DocumentEvents(doc, true);
            doc.Show(dockContainer1, DockState.Document);
            doc.Contents = contents;
            Parser.ProjectParser.ParseProjectContents(Name, contents);
            return doc;
        }

        void DocumentEvents(Document doc, bool Enable)
        {
            if (Enable)
            {
                doc.ParseContent += new EventHandler<ParseContentEventArgs>(doc_ParseContent);
                doc.FormClosing += new FormClosingEventHandler(doc_FormClosing);
                doc.CaretChange += new EventHandler<EventArgs>(doc_CaretChange);
                doc.Editor.Selection.Change += new EventHandler(Selection_Change);
            }
            else
            {
                doc.ParseContent -= doc_ParseContent;
                doc.FormClosing -= doc_FormClosing;
                doc.CaretChange -= doc_CaretChange;
                doc.Editor.Selection.Change -= Selection_Change;
            }
        }

        void Selection_Change(object sender, EventArgs e)
        {
            UpdateCutCopyToolbar();
        }

        void doc_CaretChange(object sender, EventArgs e)
        {
            UpdateCutCopyToolbar();
            Caret c = ((Document)sender).Editor.ActiveViewControl.Caret;
            tCursorPos.Text = "Ln " + c.Position.Y + ", Col " + c.Position.X;
        }

        void doc_ParseContent(object sender, ParseContentEventArgs e)
        {
            DoParsing(sender, e, true);
        }

        void DoParsing(object sender, ParseContentEventArgs e, bool IsOpened)
        {
            object[] param = new object[] { sender, (object)e, true };
            ThreadPool.QueueUserWorkItem(new WaitCallback(ParseContentThread), (object)param);
        }

        void ParseContentThread(Object stateInfo)
        {
            object[] param = (object[])stateInfo;
            object sender = param[0];
            ParseContentEventArgs e = param[1] as ParseContentEventArgs;
            bool IsOpened = (bool)param[2];
            ParseInformation pi = Parser.ProjectParser.ParseProjectContents(e.FileName, e.Content, IsOpened);
            NRefactory.Parser.Errors errors = Parser.ProjectParser.LastParserErrors;
            Document doc = sender as Document;
            BeginInvoke(new MethodInvoker(delegate { UploadParserError(doc, errors); }));
        }

        void UploadParserError(Document doc, NRefactory.Parser.Errors e)
        {
            if (_winErrorList != null)
                _winErrorList.ProjectErrors(doc, e);

        }

        void doc_FormClosing(object sender, FormClosingEventArgs e)
        {
            Document doc = sender as Document;
            DocumentEvents(doc, false);
            Parser.ProjectParser.ProjectFiles[doc.FileName].IsOpened = false;
        }
        #region ClickHandlers
        private void cNetToolStripMenuItemCSharp_Click(object sender, EventArgs e)
        {
            if (_winErrorList.ParserErrorCount == 0)
            {
                if (_scriptLanguage != ScriptLanguage.CSharp)
                    this.ScriptLanguage = ScriptLanguage.CSharp;
            }
            else
            {
                MessageBox.Show("Remove Parsing errors before converting");
            }
        }

        private void vBNetToolStripMenuItemVbNet_Click(object sender, EventArgs e)
        {
            if (_winErrorList.ParserErrorCount == 0)
            {
                if (_scriptLanguage != ScriptLanguage.VBNET)
                    this.ScriptLanguage = ScriptLanguage.VBNET;
            }
            else
            {
                MessageBox.Show("Remove Parsing errors before converting");
            }
        }

        private void tsbCut_Click(object sender, EventArgs e)
        {
            CodeEditor.WinForms.EditViewControl view = GetCurrentView();
            if (view != null) view.Cut();
            UpdateCutCopyToolbar();
        }

        private void tsbCopy_Click(object sender, EventArgs e)
        {
            CodeEditor.WinForms.EditViewControl view = GetCurrentView();
            if (view != null)
            {
                if(view.CanCopy)
                    view.Copy();
            }
            UpdateCutCopyToolbar();
        }

        private CodeEditor.WinForms.EditViewControl GetCurrentView()
        {
            Document doc =  dockContainer1.ActiveDocument as Document;
            if (doc != null)
                return doc.CurrentView;
            else
                return null;
        }

        private void tsbPaste_Click(object sender, EventArgs e)
        {
            CodeEditor.WinForms.EditViewControl view = GetCurrentView();
            if (view != null)
            {
                if (view.CanPaste)
                    view.Paste();
            }
            UpdateCutCopyToolbar();
        }

        private void tsbUndo_Click(object sender, EventArgs e)
        {
            CodeEditor.WinForms.EditViewControl view = GetCurrentView();
            if (view != null)
            {
                if (view.CanUndo)
                    view.Undo();
            }
            UpdateCutCopyToolbar();
        }

        private void tsbRedo_Click(object sender, EventArgs e)
        {

            CodeEditor.WinForms.EditViewControl view = GetCurrentView();
            if (view != null)
            {
                if (view.CanRedo)
                    view.Redo();
            }
            UpdateCutCopyToolbar();
        }

        private void tsbToggleBookmark_Click(object sender, EventArgs e)
        {
            CodeEditor.WinForms.EditViewControl view = GetCurrentView();
            if (view != null)
            {
                view.ToggleBookmark();
            }
        }

        private void tsbPreBookmark_Click(object sender, EventArgs e)
        {
            CodeEditor.WinForms.EditViewControl view = GetCurrentView();
            if (view != null)
            {
                view.GotoPreviousBookmark();
            }
        }

        private void tsbNextBookmark_Click(object sender, EventArgs e)
        {
            CodeEditor.WinForms.EditViewControl view = GetCurrentView();
            if (view != null)
            {
                view.GotoNextBookmark();
            }
        }

        private void tsbDelAllBookmark_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all of the bookmark(s).", "AIMS Script Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                CodeEditor.WinForms.EditViewControl view = GetCurrentView();
                if (view != null)
                {
                    view.Document.ClearBookmarks();
                }
            }
        }

        private void tsbDelallBreakPoints_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove all of the break point(s).", "AIMS Script Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                CodeEditor.WinForms.EditViewControl view = GetCurrentView();
                if (view != null)
                {
                    view.Document.ClearBreakpoints();
                }
            }
        }

        private void tsbFind_Click(object sender, EventArgs e)
        {
            CodeEditor.WinForms.EditViewControl view = GetCurrentView();
            if (view != null)
            {
                view.ShowFind();
            }
        }

        private void tsbReplace_Click(object sender, EventArgs e)
        {
            CodeEditor.WinForms.EditViewControl view = GetCurrentView();
            if (view != null)
            {
                view.ShowReplace();
            }
        }

        private void tsbErrorList_Click(object sender, EventArgs e)
        {
            _winErrorList.Show();
            _winOutput.Show();
        }

        private void tsbBuild_Click(object sender, EventArgs e)
        {
          CompilerResults results = CompileScript();
          LoadComileErrors(results.Errors);
        }

        private CompilerResults CompileScript()
        {
            CodeDomProvider provider = m_AIMSProject.LanguageProperties.CodeDomProvider;
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = false;
            parameters.IncludeDebugInformation = true;
            parameters.OutputAssembly = m_AIMSProject.OutputAssemblyFullPath;
            foreach(ProjectItem item in m_AIMSProject.Items)
            {
                parameters.ReferencedAssemblies.Add(item.Include + ".dll");
            }
            parameters.ReferencedAssemblies.Add("AIMS.Scripting.ScriptRun.dll");
            
            string[] sourceCode = new string[Parser.ProjectParser.ProjectFiles.Count];
            int counter = 0;
            string tmpFilePath = Path.Combine(Path.GetDirectoryName(m_AIMSProject.OutputAssemblyFullPath),"Temp");
            if (Directory.Exists(tmpFilePath))
                Directory.Delete(tmpFilePath, true);
            Directory.CreateDirectory(tmpFilePath);
            foreach(Parser.ProjectContentItem pcItem in Parser.ProjectParser.ProjectFiles.Values)
            {
                StreamWriter writer = new StreamWriter(Path.Combine(tmpFilePath,pcItem.FileName),false);
                writer.Write(pcItem.Contents);
                writer.Close();
                sourceCode[counter++] = Path.Combine(tmpFilePath, pcItem.FileName);
            }

            CompilerResults results = provider.CompileAssemblyFromFile(parameters,sourceCode);
            Directory.Delete(tmpFilePath, true);
            return results;

        }
        private void tsbRun_Click(object sender, EventArgs e)
        {
            OnExecute();
        }

        private void tsbSave_Click(object sender, EventArgs e)
        {
            //this.OnSave(sender, e);
        }

        private void tsbNew_Click(object sender, EventArgs e)
        {
            AddNewItem();
        }

        private void AddNewItem()
        {
            dockContainer1.SuspendLayout();
            StringCollection files = new StringCollection();
            foreach (string Name in Parser.ProjectParser.ProjectFiles.Keys)
            {
                files.Add(Name);
            }
            NewFileDialog f = new NewFileDialog(_scriptLanguage, files);
            f.ShowDialog(dockContainer1);
            string fileName = f.FileName;
            if (fileName.Length > 0)
            {
                Document doc = AddDocument(fileName);
                doc.Editor.ActiveViewControl.Document.Text = GetInitialContents(f, fileName);
                doc.ParseContentsNow();
                doc.Editor.ActiveViewControl.Caret.Position = new TextPoint(0, 1);

            }
            
            dockContainer1.ResumeLayout();

        }
        #endregion

        #region Private Members

        private string GetInitialContents(NewFileDialog f, string fileName)
        {
           string defNameSpace =  NewFileDialog.GetDefaultNamespace(m_AIMSProject, fileName);
           string defClassName = NewFileDialog.GenerateValidClassOrNamespaceName(Path.GetFileNameWithoutExtension(fileName), true);

           StringBuilder contents = new StringBuilder();

           if (_scriptLanguage == ScriptLanguage.CSharp)
           {
               contents.AppendLine("#region Usings ...");
               contents.AppendLine("using System;");
               contents.AppendLine("using System.Collections.Generic;");
               contents.AppendLine("using System.Text;");
               contents.AppendLine("#endregion");
               contents.AppendLine("");
               contents.AppendLine("namespace " + defNameSpace);
               contents.AppendLine("{");
               contents.AppendLine("    " + (f.SelectedItemType == SelectedItemType.Class ? "class " : "interface ") + defClassName);
               contents.AppendLine("    {");
               contents.AppendLine("");
               contents.AppendLine("    }");
               contents.AppendLine("}");
           }
           else
           {
               contents.AppendLine("#Region Usings ...");
               contents.AppendLine("Imports System");
               contents.AppendLine("Imports System.Collections.Generic");
               contents.AppendLine("Imports System.Text");
               contents.AppendLine("#End Region");
               contents.AppendLine("");
               contents.AppendLine("Namespace " + defNameSpace);
               contents.AppendLine("    " + (f.SelectedItemType == SelectedItemType.Class ? "Class " : "Interface ") + defClassName);
               contents.AppendLine("");
               contents.AppendLine("    End " + (f.SelectedItemType == SelectedItemType.Class ? "Class" : "Interface"));
               contents.AppendLine("End Namespace");
           }

           return contents.ToString();
        }

        private void UpdateCutCopyToolbar()
        {
            EditViewControl ev = GetCurrentView();
            if (ev != null)
            {
                this.tsbComment.Enabled = true;
                this.tsbFind.Enabled = true;
                this.tsbReplace.Enabled = true;
                this.tsbToggleBookmark.Enabled = true;
                this.tsbUnComment.Enabled = true;

                this.tsbCut.Enabled = (ev.Selection.IsValid?true:false);
                this.tsbCopy.Enabled = (ev.Selection.IsValid ? true : false);
                this.tsbPaste.Enabled = ev.CanPaste;
                this.tsbRedo.Enabled = ev.CanRedo;
                this.tsbUndo.Enabled = ev.CanUndo;
               
            }
            else
            {
                this.tsbCut.Enabled = false;
                this.tsbCopy.Enabled = false;
                this.tsbPaste.Enabled = false;
                this.tsbRedo.Enabled = false;
                this.tsbUndo.Enabled = false;

                this.tsbComment.Enabled = false;
                this.tsbFind.Enabled = false;
                this.tsbReplace.Enabled = false;
                this.tsbToggleBookmark.Enabled = false;
                this.tsbUnComment.Enabled = false;
              
            }
        }
        
        public void LoadComileErrors(System.CodeDom.Compiler.CompilerErrorCollection Errors)
        {
           _winErrorList.ComilerErrors(null,Errors);
        }
        public static AutoListIcons GetIcon(IClass c)
        {
            AutoListIcons imageIndex = AutoListIcons.iClass;
            
            switch (c.ClassType)
            {
                case ClassType.Delegate:
                    imageIndex = AutoListIcons.iDelegate;
                    break;
                case ClassType.Enum:
                    imageIndex = AutoListIcons.iEnum;
                    break;
                case ClassType.Struct:
                    imageIndex = AutoListIcons.iStructure;
                    break;
                case ClassType.Interface:
                    imageIndex = AutoListIcons.iInterface;
                    break;
            }
            return  (AutoListIcons) (int)imageIndex + GetModifierOffset(c.Modifiers);
        }
        public static AutoListIcons GetIcon(IMethod method)
		{
			return (AutoListIcons)((int)AutoListIcons.iMethod + GetModifierOffset(method.Modifiers));
		}
        static int GetModifierOffset(ModifierEnum modifier)
        {
            if ((modifier & ModifierEnum.Public) == ModifierEnum.Public)
            {
                return 0;
            }
            if ((modifier & ModifierEnum.Protected) == ModifierEnum.Protected)
            {
                return 3;
            }
            if ((modifier & ModifierEnum.Internal) == ModifierEnum.Internal)
            {
                return 4;
            }
            return 2;
        }
        public static AutoListIcons GetIcon(IField field)
        {
            if (field.IsConst)
            {
                return AutoListIcons.iConstant;
            }
            else if (field.IsParameter)
            {
                return AutoListIcons.iProperties;
            }
            else if (field.IsLocalVariable)
            {
                return AutoListIcons.iField;
            }
            else
            {
                return (AutoListIcons)((int)AutoListIcons.iField + GetModifierOffset(field.Modifiers));
            }
        }
        public static AutoListIcons GetIcon(IProperty property)
        {
            if (property.IsIndexer)
                return (AutoListIcons)((int)AutoListIcons.iProperties + GetModifierOffset(property.Modifiers));
            else
                return (AutoListIcons)((int)AutoListIcons.iProperties + GetModifierOffset(property.Modifiers));
        }
        public static AutoListIcons GetIcon(IEvent evt)
        {
            return (AutoListIcons)((int) AutoListIcons.iEvent + GetModifierOffset(evt.Modifiers));
        }
        #endregion

        
        internal static IProject GetProject()
        {
            return m_AIMSProject;
        }

        private void tsbComment_Click(object sender, EventArgs e)
        {
            tsbComment.Enabled = false;
            toolStrip1.Refresh();
            EditViewControl ev = GetCurrentView();
            if (ev != null)
            {
               CommentCode(ev, true);
            }
            tsbComment.Enabled = true;
        }

        private void CommentCode(EditViewControl ev , bool comment)
        {
             
            int startRow = Math.Min(ev.Selection.Bounds.FirstRow, ev.Selection.Bounds.LastRow);
            int lastRow = Math.Max(ev.Selection.Bounds.FirstRow, ev.Selection.Bounds.LastRow);
            int lastCol = ev.Document.VisibleRows[lastRow].Count  ; // No of words
            if (lastCol >= 0)
                lastCol = Math.Max(lastCol, ev.Document.VisibleRows[lastRow].Expansion_EndChar);
            else
                lastCol = 0;
            bool found = false;
            bool Changed = false;
            TextRange tr = new TextRange(0, startRow, lastCol, lastRow);
            TextRange trFinal = new TextRange(0, startRow, lastCol+2, lastRow);

            string txt = ev.Document.GetRange(tr);
            string[] rows = txt.Split(new string[]{"\r\n"},StringSplitOptions.None);
            string[] output = new string[rows.Length];
            ev.Selection.Bounds = tr;
            for (int count = 0; count <= lastRow-startRow; count++)
            {
                found = false;
                string rowText = rows[count];
                int startIndex = rowText.Length - rowText.TrimStart(null).Length;
                string wText = rowText.Substring(startIndex);
                if (comment)
                {
                    if (wText.Length > 0)
                    {
                        wText = (_scriptLanguage == ScriptLanguage.CSharp ? "//" : "'") + wText;
                        found = true;
                    }
                }
                else
                {
                    if (_scriptLanguage == ScriptLanguage.CSharp)
                    {
                        if (wText.Length >= 2  && wText.Substring(0, 2) == "//")
                        {
                            if (wText.Length >= 3 && wText.Substring(2, 1) != "/")
                            {
                                wText = wText.Substring(2);
                                found = true;
                            }
                            else
                            {
                                if ((wText.Length > 3 && wText.Substring(0, 3) != "///") || (wText.Length > 4 && wText.Substring(0, 4) == "////"))
                                {
                                    wText = wText.Substring(2);
                                    found = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (wText.Length >= 1 && wText.Substring(0, 1) == "'")
                        {
                            wText =  wText.Substring(1);
                            found = true;
                        }
                    }
                }
                if (found && rowText.Length > 0)
                {
                    output[count] = rowText.Substring(0, startIndex) + wText;
                    Changed = true;
                }
                else
                    output[count] = rowText;
            }

            if (Changed)
            {
                string pReplacedData = string.Join("\r\n", output);
                
                ev.ReplaceSelection(pReplacedData);
                ev.Selection.Bounds = trFinal;
            }
        }

        private void tsbUnComment_Click(object sender, EventArgs e)
        {
            tsbUnComment.Enabled = false;
            toolStrip1.Refresh();
            EditViewControl ev = GetCurrentView();
            if (ev != null)
            {
                CommentCode(ev, false);
            }
            tsbUnComment.Enabled = true;
        }
        private void tsbSolutionExplorer_Click(object sender, EventArgs e)
        {
            _winProjExplorer.Show();
        }

    }
        
    public enum ScriptLanguage
    {
        CSharp,
        VBNET
    }

    
}