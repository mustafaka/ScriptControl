
using System;
using System.Collections.Generic;
using System.Text;

using AIMS.Libraries.Scripting.Dom;
using AIMS.Libraries.Scripting.Dom.CSharp;
using AIMS.Libraries.CodeEditor.Syntax;
using AIMS.Libraries.CodeEditor.WinForms;
using AIMS.Libraries.CodeEditor.WinForms.InsightWindow;
using AIMS.Libraries.CodeEditor.WinForms.CompletionWindow;
using AIMS.Libraries.CodeEditor;
using AIMS.Libraries.Scripting.CodeCompletion;
using AIMS.Libraries.Scripting.ScriptControl;
using AIMS.Libraries.Scripting.ScriptControl.CodeCompletion;

namespace AIMS.Libraries.Scripting.CodeCompletion
{
	public class EventHandlerCompletitionDataProvider : AbstractCompletionDataProvider
	{
		string expression;
		ResolveResult resolveResult;
		IClass resolvedClass;
		
		public EventHandlerCompletitionDataProvider(string expression, ResolveResult resolveResult)
		{
			this.expression = expression;
			this.resolveResult = resolveResult;
			this.resolvedClass = resolveResult.ResolvedType.GetUnderlyingClass();
            if (this.resolvedClass == null && resolveResult.ResolvedType.FullyQualifiedName.Length > 0)
            {
                this.resolvedClass =  ScriptControl.Parser.ProjectParser.CurrentProjectContent.GetClass(resolveResult.ResolvedType.FullyQualifiedName);

            }
            
		}
		
		/// <summary>
		/// Generates the completion data. This method is called by the text editor control.
		/// </summary>
		public override ICompletionData[] GenerateCompletionData(string fileName, EditViewControl textArea, char charTyped)
		{
            string methodName = this.resolveResult.CallingClass.Name + "_" + this.expression.Trim().Substring(this.expression.Trim().LastIndexOf('.')+1);
			List<ICompletionData> completionData = new List<ICompletionData>();
			
			completionData.Add(new DelegateCompletionData("delegate {  };", 3,
			                                              "Insert Anonymous Method"));
			CSharpAmbience ambience = new CSharpAmbience();
			ambience.ConversionFlags = ConversionFlags.ShowParameterNames;
			IMethod invoke = resolvedClass.SearchMember("Invoke", LanguageProperties.CSharp) as IMethod;
            DomRegion r = this.resolveResult.CallingMember.BodyRegion;
            DomRegion rm = this.resolveResult.CallingMember.Region;
            TextPoint cPos = textArea.Caret.Position;
            TextRange trIntened = new TextRange(0, rm.BeginLine - 1, rm.BeginColumn - 1, rm.BeginLine - 1);
            string IntendString = textArea.Document.GetRange(trIntened);

            int curPos = textArea.Document.PointToIntPos(new TextPoint(0,r.EndLine-1));

            
            StringBuilder parambuilder = new StringBuilder("(");
			if (invoke != null) {
				StringBuilder builder = new StringBuilder("delegate(");
				for (int i = 0; i < invoke.Parameters.Count; ++i) {
					if (i > 0) {
						builder.Append(", ");
                        parambuilder.Append(", ");
					}
					builder.Append(ambience.Convert(invoke.Parameters[i]));
                    parambuilder.Append(ambience.Convert(invoke.Parameters[i]));
				}
				builder.Append(") {  };");
                parambuilder.Append(")");
                string MethodBody = "new " + resolveResult.ResolvedType.Name + "(delegate" + parambuilder + "{   });";
               
                completionData.Add(new DelegateCompletionData(MethodBody, 4,
                                                          "delegate " + resolvedClass.FullyQualifiedName + "\n" + CodeCompletionData.GetDocumentation(resolvedClass.Documentation)));
				completionData.Add(new DelegateCompletionData(builder.ToString(), 3,
				                                              "Insert Anonymous Method With Parameters"));
				IClass callingClass = resolveResult.CallingClass;
				IClass eventReturnType = invoke.ReturnType.GetUnderlyingClass();
				IClass[] eventParameters = new IClass[invoke.Parameters.Count];
				for (int i = 0; i < eventParameters.Length; i++) {
					eventParameters[i] = invoke.Parameters[i].ReturnType.GetUnderlyingClass();
					if (eventParameters[i] == null) {
						eventReturnType = null;
						break;
					}
				}
				if (callingClass != null && eventReturnType != null) {
					bool inStatic = false;
					if (resolveResult.CallingMember != null)
						inStatic = resolveResult.CallingMember.IsStatic;
					foreach (IMethod method in callingClass.DefaultReturnType.GetMethods()) {
						if (inStatic && !method.IsStatic)
							continue;
						if (!method.IsAccessible(callingClass, true))
							continue;
						if (method.Parameters.Count != invoke.Parameters.Count)
							continue;
						// check return type compatibility:
						IClass c2 = method.ReturnType.GetUnderlyingClass();
						if (c2 == null || !c2.IsTypeInInheritanceTree(eventReturnType))
							continue;
						bool ok = true;
						for (int i = 0; i < eventParameters.Length; i++) {
							c2 = method.Parameters[i].ReturnType.GetUnderlyingClass();
							if (c2 == null || !eventParameters[i].IsTypeInInheritanceTree(c2)) {
								ok = false;
								break;
							}
						}
						if (ok) {
							completionData.Add(new CodeCompletionData(method));
						}
					}
				}
			}
			return completionData.ToArray();
		}
		
		private class DelegateCompletionData : DefaultCompletionData
		{
			int cursorOffset;
			
			public DelegateCompletionData(string text, int cursorOffset, string documentation)
				: base(text, documentation, AutoListIcons.iDelegate)
			{
                //: base(text, StringParser.Parse(documentation), AutoListIcons.iDelegate)
				this.cursorOffset = cursorOffset;
			}
			
			public override bool InsertAction(EditViewControl textArea, char ch)
			{
				bool r = base.InsertAction(textArea, ch);
				textArea.Caret.Position.X -= cursorOffset;
				return r;
			}
		}
	}
}
