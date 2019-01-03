

using System;
using System.Diagnostics;
using AIMS.Libraries.Scripting.Dom;
using AIMS.Libraries.CodeEditor;
using AIMS.Libraries.CodeEditor.WinForms;
using AIMS.Libraries.CodeEditor.WinForms.CompletionWindow;
using AIMS.Libraries.CodeEditor.Syntax;
using AIMS.Libraries.Scripting.Dom.CSharp;
using AIMS.Libraries.Scripting.NRefactory;
using AIMS.Libraries.Scripting.Dom.NRefactoryResolver;
namespace AIMS.Libraries.Scripting.ScriptControl.CodeCompletion
{
	/// <summary>
	/// Data provider for code completion.
	/// </summary>
	public class CodeCompletionDataProvider : AbstractCodeCompletionDataProvider
	{
        /// <summary>
        /// Initialize a CodeCompletionDataProvider that reads the expression from the text area.
        /// </summary>
        public CodeCompletionDataProvider()
        {
        }
        
		/// <summary>
		/// Initalize a CodeCompletionDataProvider with a fixed expression.
		/// </summary>
		public CodeCompletionDataProvider(ExpressionResult expression)
		{
			this.fixedExpression = expression;
		}
		
		ExpressionResult fixedExpression;
		
		protected override void GenerateCompletionData(EditViewControl textArea, char charTyped)
		{
			preSelection = null;
			if (fixedExpression.Expression == null)
				GenerateCompletionData(textArea, GetExpression(textArea));
			else
				GenerateCompletionData(textArea, fixedExpression);
		}
		
		
		protected void GenerateCompletionData(EditViewControl textArea, ExpressionResult expressionResult)
		{
			// allow empty string as expression (for VB 'With' statements)
			if (expressionResult.Expression == null) {
				return;
			}
            string textContent = Parser.ProjectParser.GetFileContents(fileName);
            NRefactoryResolver rr = Parser.ProjectParser.GetResolver();
            ResolveResult r1 = rr.Resolve(expressionResult, caretLineNumber, caretColumn, textArea.FileName, Parser.ProjectParser.GetFileContents(fileName)); 
            AddResolveResults(r1,
			                  expressionResult.Context);
		}
	}
}
