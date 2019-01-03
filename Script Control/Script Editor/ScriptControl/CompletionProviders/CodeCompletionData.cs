

using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using AIMS.Libraries.Scripting.Dom;
using AIMS.Libraries.CodeEditor;
using AIMS.Libraries.CodeEditor.WinForms;
using AIMS.Libraries.CodeEditor.WinForms.CompletionWindow;
using AIMS.Libraries.CodeEditor.Syntax;
using AIMS.Libraries.Scripting.Dom.CSharp;

namespace AIMS.Libraries.Scripting.ScriptControl.CodeCompletion

{
	public class CodeCompletionData : ICompletionData
	{
		IAmbience ambience;
		AutoListIcons      imageIndex;
		int      overloads;
		string   text;
		string   description;
		string   documentation;
		IClass   c;
		IMember  member;
		bool     convertedDocumentation = false;
		double priority;
		
		/// <summary>
		/// Gets the class this CodeCompletionData object was created for.
		/// Returns null if the CodeCompletionData object was created for a method/property etc.
		/// </summary>
       
		public IClass Class {
			get {
				return c;
			}
		}
		
		/// <summary>
		/// Gets the member this CodeCompletionData object was created for.
		/// Returns null if the CodeCompletionData object was created for a class or namespace.
		/// </summary>
		public IMember Member {
			get {
				return member;
			}
		}
		
		public int Overloads {
			get {
				return overloads;
			}
			set {
				overloads = value;
			}
		}
		
		public double Priority {
			get {
				return priority;
			}
			set {
				priority = value;
			}
		}
		
		public AutoListIcons ImageIndex {
			get {
				return imageIndex;
			}
			set {
				imageIndex = value;
			}
		}
		
		public string Text {
			get {
				return text;
			}
			set {
				text = value;
			}
		}
		
		public string Description {
			get {
				// don't give a description string, if no documentation or description is provided
				if (description.Length == 0 && (documentation == null || documentation.Length == 0)) {
					return "";
				}
				if (!convertedDocumentation && documentation != null) {
					convertedDocumentation = true;
					documentation = GetDocumentation(documentation);
				}
				
				return description + (overloads > 0 ? " " + "Overloads " + overloads.ToString() : String.Empty) + "\n" + documentation;
			}
			set {
				description = value;
			}
		}
		
		string dotnetName;
		
		void GetPriority(string dotnetName)
		{
			this.dotnetName = dotnetName;
			priority = CodeCompletionDataUsageCache.GetPriority(dotnetName, true);
		}
		
		
        public CodeCompletionData(string s,string desc, AutoListIcons imageIndex)
        {
            ambience = Parser.ProjectParser.CurrentAmbience;
            description =  desc;
            documentation = string.Empty;
            text = s;
            this.imageIndex = imageIndex;
            GetPriority(s);
        }

        
		public CodeCompletionData(IClass c)
		{
            ambience = Parser.ProjectParser.CurrentAmbience;
			// save class (for the delegate description shortcut)
			this.c = c;
            imageIndex = ScriptControl.GetIcon(c);
			ambience.ConversionFlags = ConversionFlags.None;
			text = ambience.Convert(c);
			ambience.ConversionFlags = ConversionFlags.UseFullyQualifiedNames | ConversionFlags.ShowReturnType | ConversionFlags.ShowModifiers;
			description = ambience.Convert(c);
			documentation = c.Documentation;
			GetPriority(c.DotNetName);
		}
		
		public CodeCompletionData(IMethod method)
		{
			member = method;
            ambience = Parser.ProjectParser.CurrentAmbience;
			ambience.ConversionFlags = ConversionFlags.ShowReturnType | ConversionFlags.ShowParameterNames | ConversionFlags.ShowModifiers;
            imageIndex = ScriptControl.GetIcon(method);
			text        = method.Name;
			description = ambience.Convert(method);
			documentation = method.Documentation;
			GetPriority(method.DotNetName);
		}
		
		public CodeCompletionData(IField field)
		{
			member = field;
            ambience = Parser.ProjectParser.CurrentAmbience;
			ambience.ConversionFlags = ConversionFlags.ShowReturnType | ConversionFlags.ShowParameterNames | ConversionFlags.ShowModifiers;
            imageIndex = ScriptControl.GetIcon(field);
			text        = field.Name;
			description = ambience.Convert(field);
			documentation = field.Documentation;
			GetPriority(field.DotNetName);
		}
		
		public CodeCompletionData(IProperty property)
		{
			member = property;
            ambience = Parser.ProjectParser.CurrentAmbience;
			ambience.ConversionFlags = ConversionFlags.ShowReturnType | ConversionFlags.ShowParameterNames | ConversionFlags.ShowModifiers;
            imageIndex = ScriptControl.GetIcon(property);
			text        = property.Name;
			description = ambience.Convert(property);
			documentation = property.Documentation;
			GetPriority(property.DotNetName);
		}
		
		public CodeCompletionData(IEvent e)
		{
			member = e;
            ambience = Parser.ProjectParser.CurrentAmbience;
			ambience.ConversionFlags = ConversionFlags.ShowReturnType | ConversionFlags.ShowParameterNames | ConversionFlags.ShowModifiers;
            imageIndex = ScriptControl.GetIcon(e);
			text        = e.Name;
			description = ambience.Convert(e);
			documentation = e.Documentation;
			GetPriority(e.DotNetName);
		}
		
		public bool InsertAction(EditViewControl textArea, char ch)
		{
			if (dotnetName != null) {
				CodeCompletionDataUsageCache.IncrementUsage(dotnetName);
			}
			if (c != null && text.Length > c.Name.Length) {
				textArea.InsertText(text.Substring(0, c.Name.Length + 1));
				TextPoint start = textArea.Caret.Position;
				TextPoint end;
				int pos = text.IndexOf(',');
				if (pos < 0) {
					textArea.InsertText(text.Substring(c.Name.Length + 1));
					end = textArea.Caret.Position;
					end.X -= 1;
				} else {
                    textArea.InsertText(text.Substring(c.Name.Length + 1, pos - c.Name.Length - 1));
					end = textArea.Caret.Position;
                    textArea.InsertText(text.Substring(pos));
				}
				textArea.Caret.Position = start;
                textArea.Selection.SelStart = start.X;
                textArea.Selection.SelEnd = end.X;
				//textArea.Selection.se.SelectionManager.SetSelection(start, end);
				if (!char.IsLetterOrDigit(ch)) {
					return true;
				}
			} else {
                textArea.InsertText(text);
			}
			return false;
		}
		
		internal static Regex whitespace = new Regex(@"\s+");
		
		/// <summary>
		/// Converts the xml documentation string into a plain text string.
		/// </summary>
		public static string GetDocumentation(string doc)
		{
			System.IO.StringReader reader = new System.IO.StringReader("<docroot>" + doc + "</docroot>");
			XmlTextReader xml   = new XmlTextReader(reader);
			StringBuilder ret   = new StringBuilder();
			////Regex whitespace    = new Regex(@"\s+");
			
			try {
				xml.Read();
				do {
					if (xml.NodeType == XmlNodeType.Element) {
						string elname = xml.Name.ToLowerInvariant();
						switch (elname) {
							case "filterpriority":
								xml.Skip();
								break;
							case "remarks":
								ret.Append(Environment.NewLine);
								ret.Append("Remarks:");
								ret.Append(Environment.NewLine);
								break;
							case "example":
								ret.Append(Environment.NewLine);
								ret.Append("Example:");
								ret.Append(Environment.NewLine);
								break;
							case "exception":
								ret.Append(Environment.NewLine);
								ret.Append(GetCref(xml["cref"]));
								ret.Append(": ");
								break;
							case "returns":
								ret.Append(Environment.NewLine);
								ret.Append("Returns: ");
								break;
							case "see":
								ret.Append(GetCref(xml["cref"]));
								ret.Append(xml["langword"]);
								break;
							case "seealso":
								ret.Append(Environment.NewLine);
								ret.Append("See also: ");
								ret.Append(GetCref(xml["cref"]));
								break;
							case "paramref":
								ret.Append(xml["name"]);
								break;
							case "param":
								ret.Append(Environment.NewLine);
								ret.Append(whitespace.Replace(xml["name"].Trim()," "));
								ret.Append(": ");
								break;
							case "value":
								ret.Append(Environment.NewLine);
								ret.Append("Value: ");
								ret.Append(Environment.NewLine);
								break;
							case "br":
							case "para":
								ret.Append(Environment.NewLine);
								break;
						}
					} else if (xml.NodeType == XmlNodeType.Text) {
						ret.Append(whitespace.Replace(xml.Value, " "));
					}
				} while(xml.Read());
			} catch  {
				//LoggingService.Debug("Invalid XML documentation: " + ex.Message);
				return doc;
			}
			return ret.ToString();
		}
		
		static string GetCref(string cref)
		{
			if (cref == null || cref.Trim().Length==0) {
				return "";
			}
			if (cref.Length < 2) {
				return cref;
			}
			if (cref.Substring(1, 1) == ":") {
				return cref.Substring(2, cref.Length - 2);
			}
			return cref;
		}
		
		#region System.IComparable interface implementation
		public int CompareTo(object obj)
		{
			if (obj == null || !(obj is CodeCompletionData)) {
				return -1;
			}
			return text.CompareTo(((CodeCompletionData)obj).text);
		}
		#endregion
	}
}
