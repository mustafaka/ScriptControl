using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using AIMS.Libraries.Forms.Docking;
using AIMS.Libraries.CodeEditor.SyntaxFiles;
using AIMS.Libraries.Scripting.NRefactory;
using AIMS.Libraries.CodeEditor.Syntax;
using AIMS.Libraries.Scripting.ScriptControl.Converter;
using AIMS.Libraries.Scripting.CodeCompletion;
using AIMS.Libraries.Scripting.ScriptControl.CodeCompletion;
using AIMS.Libraries.Scripting.Dom;
namespace AIMS.Libraries.Scripting.ScriptControl
{
    public partial class Document : DockableWindow
    {
        string _FileName = "";
        string _contents = "";
        ScriptLanguage _scriptLanguage;
        ScriptControl _Parent = null;
        QuickClassBrowserPanel quickClassBrowserPanel = null;
        Word lastErrorWord = null;
        public ScriptControl ParentScriptControl
        {
            get { return _Parent; }
        }

        public Document(ScriptControl Parent):this()
        {
            _Parent = Parent;
        }

        public CodeEditor.CodeEditorControl Editor
        {
            get { return this.CodeEditorCtrl; }
        }

        public Document()
        {
            InitializeComponent();

            this.CodeEditorCtrl.Document = this.syntaxDocument1;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.CodeEditorCtrl.Indent = AIMS.Libraries.CodeEditor.WinForms.IndentStyle.LastRow;
            this.CodeEditorCtrl.LineNumberForeColor = Color.FromArgb(50, this.CodeEditorCtrl.LineNumberForeColor);
            this.CodeEditorCtrl.LineNumberBorderColor = Color.FromArgb(50, this.CodeEditorCtrl.LineNumberBorderColor);
            this.CodeEditorCtrl.TextChanged += new EventHandler(ActiveViewControl_TextChanged);
            this.CodeEditorCtrl.ActiveViewControl.KeyDown += new KeyEventHandler(ActiveViewControl_KeyDown);
            this.CodeEditorCtrl.ActiveViewControl.KeyPress += new KeyPressEventHandler(ActiveViewControl_KeyPress);
            this.CodeEditorCtrl.CaretChange += new EventHandler(CodeEditorCtrl_CaretChange);
            HostCallBackRegister();
            ShowQuickClassBrowserPanel();
            
        }

        

        public QuickClassBrowserPanel QuickClassBrowserPanel
        {
            get
            {
                return quickClassBrowserPanel;
            }
        }

        public event EventHandler<EventArgs> CaretChange;
        void CodeEditorCtrl_CaretChange(object sender, EventArgs e)
        {
            OnCaretChange(e);
        }

        protected virtual void OnCaretChange(EventArgs e)
        {
            if (CaretChange != null)
            {
                CaretChange(this, e);
            }
        }

        public void JumpToFilePosition(string fileName, int Line, int Column)
        {
            this.CodeEditorCtrl.ActiveViewControl.Caret.Position = new TextPoint(Column, Line);
        }

        void ActiveViewControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = HandleKeyPress(e.KeyChar);
            
            if (e.Handled == false && e.KeyChar == '.' &&  this.CodeEditorCtrl.AutoListVisible == false )
            {
                string SearchWord = this.Editor.ActiveViewControl.Caret.CurrentWord.Text;
                string FoundWord="";
                

                AIMS.Libraries.CodeEditor.WinForms.CompletionWindow.ICompletionDataProvider cdp = new CtrlSpaceCompletionDataProvider();
                AIMS.Libraries.CodeEditor.WinForms.CompletionWindow.ICompletionData[] completionData = cdp.GenerateCompletionData(this.FileName, this.Editor.ActiveViewControl,(char) e.KeyChar);
                
                foreach (AIMS.Libraries.CodeEditor.WinForms.CompletionWindow.ICompletionData data in completionData)
                {
                    if (SearchWord.ToLower() == data.Text.ToLower())
                    {
                        FoundWord = data.Text;
                        break;
                    }
                    
                }

                if (FoundWord.Length > 0)
                {

                    this.Editor.ActiveViewControl.Caret.CurrentWord.Text = FoundWord;
                    e.Handled = HandleKeyPress(e.KeyChar);
                    return ;
                }
            }

        }

        bool IsVariable(string varName)
        {
            return false;
        }
        void ActiveViewControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control & e.KeyCode == Keys.Space)
            {
                this.CodeEditorCtrl.ActiveViewControl.ShowCompletionWindow(new CtrlSpaceCompletionDataProvider(), '\0');
                e.Handled = true;
            }
        }
        private ICodeCompletionBinding[] GetCompletionBinding()
        {
            ICodeCompletionBinding[] bindings = null;
            if (_scriptLanguage == ScriptLanguage.CSharp)
                bindings = new ICodeCompletionBinding[] { new CSharpCompletionBinding() };
            else
                bindings = new ICodeCompletionBinding[] { new VBNetCompletionBinding() };
            return bindings;
        }

        private ICodeCompletionBinding[] codeCompletionBindings;
        public ICodeCompletionBinding[] CodeCompletionBindings
        {
            get
            {
                if (codeCompletionBindings == null)
                {
                    try
                    {
                        codeCompletionBindings = GetCompletionBinding();
                    }
                    catch
                    {
                        codeCompletionBindings = new ICodeCompletionBinding[] { };
                    }
                }
                return codeCompletionBindings;
            }
        }

        bool inHandleKeyPress;
        bool HandleKeyPress(char ch)
        {
            if (inHandleKeyPress)
                return false;
            inHandleKeyPress = true;
            try
            {

                if (CodeCompletionOptions.EnableCodeCompletion && Editor.AutoListVisible == false)
                {
                    foreach (ICodeCompletionBinding ccBinding in CodeCompletionBindings)
                    {
                        if (ccBinding.HandleKeyPress(CodeEditorCtrl, ch))
                            return false;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                inHandleKeyPress = false;
            }
            return false;
        }

        bool IsInComment(CodeEditor.WinForms.EditViewControl editor)
        {
            Dom.CSharp.CSharpExpressionFinder ef = new Dom.CSharp.CSharpExpressionFinder(this.FileName);
            int cursor = editor.Caret.Offset - 1;
            return ef.FilterComments(this.Contents, ref cursor) == null;
        }
        public string Contents
        {
            get { return _contents; }
            set { this.CodeEditorCtrl.ActiveViewControl.Document.Text = value; }
        }
        void ActiveViewControl_TextChanged(object sender, EventArgs e)
        {
            _contents = this.CodeEditorCtrl.ActiveViewControl.Document.Text;
            ParseContentEventArgs eInfo = new ParseContentEventArgs(this.FileName, _contents);
            OnParseContent(ref eInfo);
        }

        #region Events
        public event EventHandler<ParseContentEventArgs> ParseContent;

        protected virtual void OnParseContent(ref ParseContentEventArgs e)
        {
            if (ParseContent != null)
            {
                ParseContent(this,e);
            }
        }

        # endregion

        public string FileName
        {
            get { return _FileName; }
            set { 
                
                _FileName = value;
                this.CodeEditorCtrl.ActiveViewControl.FileName = value;
                this.CodeEditorCtrl.FileName = value;

            }
        }
        
        public ScriptLanguage ScriptLanguage
        {
            get { return _scriptLanguage; }
            set
            {
                
                if (value == ScriptLanguage.CSharp)
                {
                    CodeEditorSyntaxLoader.SetSyntax(this.CodeEditorCtrl, SyntaxLanguage.CSharp);
                }
                else
                {
                    CodeEditorSyntaxLoader.SetSyntax(this.CodeEditorCtrl, SyntaxLanguage.VBNET);
                }
                
                
                _scriptLanguage = value;
                //Reset The code Completion bindings
                GetCompletionBinding();
                this.CodeEditorCtrl.ScrollIntoView(0);
                OnCaretChange(null);
            }
        }

        public void HighlightRemove(int LineNo, int ColNo)
        {
            SyntaxDocument Doc = this.CodeEditorCtrl.ActiveViewControl.Document;
            Word curWord = Doc.GetWordFromPos(new TextPoint(ColNo, LineNo));
            if (curWord != null)
            {
                curWord.HasError = false;
                curWord.HasWarning = false;
                curWord.InfoTip = "";
            }
            if (lastErrorWord != null)
            {
                lastErrorWord.HasError = false;
                lastErrorWord.HasWarning = false;
                lastErrorWord.InfoTip = "";
            }

        }

        public void HighlightError(int LineNo, int ColNo, bool IsWarning, string error)
        {
            SyntaxDocument Doc = this.CodeEditorCtrl.ActiveViewControl.Document;
            Word curWord = Doc.GetWordFromPos(new TextPoint(ColNo, LineNo));
            if (curWord != null)
            {
                if (IsWarning)
                    curWord.HasWarning = true;
                else
                    curWord.HasError = true;
                curWord.InfoTip = error;
            }
            lastErrorWord = curWord;
        }

        public CodeEditor.WinForms.EditViewControl CurrentView
        {

            get { return this.CodeEditorCtrl.ActiveViewControl; }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            this.CodeEditorCtrl.Focus();
            ParseContentsNow();
            
        }

        public void ParseContentsNow()
        {
            ActiveViewControl_TextChanged(null,null);
            quickClassBrowserPanel.PopulateCombo();
        }
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.CodeEditorCtrl.Focus();
        }

        public void HostCallBackRegister()
        {
            // Must be implemented. Gets the parse information for the specified file.
            HostCallback.GetParseInformation = delegate(string fileName)
            {
                //if (fileName != this.FileName)
                //    throw new Exception("Unknown file");
                return Parser.ProjectParser.GetParseInformation(this.FileName);
            };

            // Must be implemented. Gets the project content of the active project.
            HostCallback.GetCurrentProjectContent = delegate
            {
                return Parser.ProjectParser.CurrentProjectContent;
            };

            // The default implementation just logs to Log4Net. We want to display a MessageBox.
            // Note that we use += here - in this case, we want to keep the default Log4Net implementation.
            HostCallback.ShowError += delegate(string message, Exception ex)
            {
                MessageBox.Show(message + Environment.NewLine + ex.ToString());
            };
            HostCallback.ShowMessage += delegate(string message)
            {
                MessageBox.Show(message);
            };
            HostCallback.ShowAssemblyLoadError += delegate(string fileName, string include, string message)
            {
                MessageBox.Show("Error loading code-completion information for "
                                + include + " from " + fileName
                                + ":\r\n" + message + "\r\n");
            };
        }

        public void HostCallBackUnRegister()
        {
            // Must be implemented. Gets the parse information for the specified file.
            HostCallback.GetParseInformation = null;
            // Must be implemented. Gets the project content of the active project.
            HostCallback.GetCurrentProjectContent = null;
            // The default implementation just logs to Log4Net. We want to display a MessageBox.
            // Note that we use += here - in this case, we want to keep the default Log4Net implementation.
            HostCallback.ShowError = null;
            HostCallback.ShowMessage = null;
            HostCallback.ShowAssemblyLoadError = null;
        }

        void ShowQuickClassBrowserPanel()
        {
            if (quickClassBrowserPanel == null)
            {
                quickClassBrowserPanel = new QuickClassBrowserPanel(this.CodeEditorCtrl);
                Controls.Add(quickClassBrowserPanel);
                quickClassBrowserPanel.BackColor = this.CodeEditorCtrl.GutterMarginColor;
                this.CodeEditorCtrl.BorderStyle = AIMS.Libraries.CodeEditor.WinForms.ControlBorderStyle.None;
                this.CodeEditorCtrl.ActiveViewControl.BorderColor = this.CodeEditorCtrl.GutterMarginBorderColor;
                this.CodeEditorCtrl.ActiveViewControl.BorderStyle = AIMS.Libraries.CodeEditor.WinForms.ControlBorderStyle.FixedSingle;

                //CodeEditorCtrl.BorderStyle = AIMS.Libraries.CodeEditor.WinForms.ControlBorderStyle.FixedSingle;
            }
        }
        void RemoveQuickClassBrowserPanel()
        {
            if (quickClassBrowserPanel != null)
            {
                Controls.Remove(quickClassBrowserPanel);
                quickClassBrowserPanel.Dispose();
                quickClassBrowserPanel = null;
                //CodeEditorCtrl.BorderStyle = AIMS.Libraries.CodeEditor.WinForms.ControlBorderStyle.None;
            }
        }
    }
    public class ParseContentEventArgs : EventArgs
    {
        public string FileName = "";
        public string Content = "";
        public int Column = 0;
        public int Line = 0;
        public ParseContentEventArgs(string fileName, string content)
        {
            FileName = fileName;
            Content = content;
        }
    }
}