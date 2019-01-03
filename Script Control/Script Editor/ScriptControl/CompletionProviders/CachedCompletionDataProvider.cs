using System;
using System.Windows.Forms;

using AIMS.Libraries.Scripting.Dom;
using AIMS.Libraries.CodeEditor;
using AIMS.Libraries.CodeEditor.WinForms;
using AIMS.Libraries.CodeEditor.WinForms.CompletionWindow;
using AIMS.Libraries.CodeEditor.Syntax;
using AIMS.Libraries.Scripting.Dom.CSharp;

namespace AIMS.Libraries.Scripting.ScriptControl.CodeCompletion
{
	public class CachedCompletionDataProvider : AbstractCompletionDataProvider
	{
		ICompletionDataProvider baseProvider;
		
		public CachedCompletionDataProvider(ICompletionDataProvider baseProvider)
		{
			this.baseProvider = baseProvider;
		}
		
		ICompletionData[] completionData;
		
		public ICompletionData[] CompletionData {
			get {
				return completionData;
			}
			set {
				completionData = value;
			}
		}
		
		public override ImageList ImageList {
			get {
				return baseProvider.ImageList;
			}
		}
		
		public override CompletionDataProviderKeyResult ProcessKey(char key)
		{
			return baseProvider.ProcessKey(key);
		}
		
		public override bool InsertAction(ICompletionData data, EditViewControl textArea, int insertionOffset, char key)
		{
			return baseProvider.InsertAction(data, textArea, insertionOffset, key);
		}
		
		public override ICompletionData[] GenerateCompletionData(string fileName, EditViewControl textArea, char charTyped)
		{
			if (completionData == null) {
				completionData = baseProvider.GenerateCompletionData(fileName, textArea, charTyped);
				preSelection = baseProvider.PreSelection;
				this.DefaultIndex = baseProvider.DefaultIndex;
			}
			return completionData;
		}
		
		[Obsolete("Cannot use InsertSpace on CachedCompletionDataProvider, please set it on the underlying provider!")]
		public new bool InsertSpace {
			get {
				return false;
			}
			set {
				throw new NotSupportedException();
			}
		}
	}
}
