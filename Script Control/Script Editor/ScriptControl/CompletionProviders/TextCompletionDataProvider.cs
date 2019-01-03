

using System;
using AIMS.Libraries.Scripting.Dom;
using AIMS.Libraries.CodeEditor;
using AIMS.Libraries.CodeEditor.WinForms;
using AIMS.Libraries.CodeEditor.WinForms.CompletionWindow;
using AIMS.Libraries.CodeEditor.Syntax;
using AIMS.Libraries.Scripting.Dom.CSharp;
using AIMS.Libraries.Scripting.NRefactory;
using AIMS.Libraries.Scripting.Dom.NRefactoryResolver;
using AIMS.Libraries.Scripting.Dom.Refactoring;
namespace AIMS.Libraries.Scripting.ScriptControl.CodeCompletion
{
	/// <summary>
	/// Data provider for code completion.
	/// </summary>
	public class TextCompletionDataProvider : AbstractCompletionDataProvider
	{
		string[] texts;
		
		public TextCompletionDataProvider(params string[] texts)
		{
			this.texts = texts;
		}
		
		public override ICompletionData[] GenerateCompletionData(string fileName, EditViewControl textArea, char charTyped)
		{
			ICompletionData[] data = new ICompletionData[texts.Length];
			for (int i = 0; i < data.Length; i++) {
				data[i] = new DefaultCompletionData(texts[i], null, AutoListIcons.iClassShortCut);
			}
			return data;
		}
	}
}
