using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

//using ICSharpCode.Core;
using AIMS.Libraries.Scripting.Dom;
using AIMS.Libraries.CodeEditor;
using AIMS.Libraries.CodeEditor.Syntax;
namespace AIMS.Libraries.Scripting.ScriptControl
{
	public class QuickClassBrowserPanel : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.ComboBox classComboBox;
		private System.Windows.Forms.ComboBox membersComboBox;
		
		ICompilationUnit            currentCompilationUnit;
		CodeEditorControl           textAreaControl;
		bool                        autoselect = true;
		
		class ComboBoxItem : System.IComparable
		{
			object item;
			string text;
			int    iconIndex;
			bool   isInCurrentPart;
			
			public int IconIndex {
				get {
					return iconIndex;
				}
			}
			
			public object Item {
				get {
					return item;
				}
			}
			
			public bool IsInCurrentPart {
				get {
					return isInCurrentPart;
				}
			}
			
			public DomRegion ItemRegion {
				get {
					IClass classItem = item as IClass;
					if (item is IClass)
						return ((IClass)item).Region;
					else if (item is IMember)
						return ((IMember)item).Region;
					else
						return DomRegion.Empty;
				}
			}
			
			public int Line {
				get {
					DomRegion r = this.ItemRegion;
					if (r.IsEmpty)
						return 0;
					else
						return r.BeginLine - 1;
				}
			}
			
			public int Column {
				get {
					DomRegion r = this.ItemRegion;
					if (r.IsEmpty)
						return 0;
					else
						return r.BeginColumn - 1;
				}
			}
			
			public int EndLine {
				get {
					DomRegion r = this.ItemRegion;
					if (r.IsEmpty)
						return 0;
					else
						return r.EndLine - 1;
				}
			}
			
			public ComboBoxItem(object item, string text, int iconIndex, bool isInCurrentPart)
			{
				this.item = item;
				this.text = text;
				this.iconIndex = iconIndex;
				this.isInCurrentPart = isInCurrentPart;
			}
			
			public bool IsInside(int lineNumber)
			{
				if (!isInCurrentPart)
					return false;
				IClass classItem = item as IClass;
				if (classItem != null) {
					if (classItem.Region.IsEmpty)
						return false;
					return classItem.Region.BeginLine - 1 <= lineNumber &&
						classItem.Region.EndLine - 1 >= lineNumber;
				}
				
				IMember member = item as IMember;
				if (member == null || member.Region.IsEmpty) {
					return false;
				}
				bool isInside = member.Region.BeginLine - 1 <= lineNumber;
				
				if (member is IMethodOrProperty) {
					if (((IMethodOrProperty)member).BodyRegion.EndLine >= 0) {
						isInside &= lineNumber <= ((IMethodOrProperty)member).BodyRegion.EndLine - 1;
					} else {
						return member.Region.BeginLine - 1 == lineNumber;
					}
				} else {
					isInside &= lineNumber <= member.Region.EndLine - 1;
				}
				return isInside;
			}
			
			public int CompareItemTo(object obj)
			{
				ComboBoxItem boxItem = (ComboBoxItem)obj;
				
				if (boxItem.Item is IComparable) {
					return ((IComparable)boxItem.Item).CompareTo(item);
				}
				if (boxItem.text != text || boxItem.Line != Line || boxItem.EndLine != EndLine || boxItem.iconIndex != iconIndex) {
					return 1;
				}
				return 0;
			}
			
			string cachedString;
			
			public override string ToString()
			{
				// ambience lookups can be expensive when the return type is
				// resolved on the fly.
				// Therefore, we need to cache the generated string because it is used
				// very often for the sorting.
				if (cachedString == null)
					cachedString = ToStringInternal();
				return cachedString;
			}
			
			string ToStringInternal()
			{
				IAmbience ambience = Parser.ProjectParser.CurrentAmbience;
				ambience.ConversionFlags = ConversionFlags.ShowParameterNames;
				if (item is IMethod) {
					return ambience.Convert((IMethod)item);
				}
				if (item is IProperty) {
					return ambience.Convert((IProperty)item);
				}
				if (item is IField) {
					return ambience.Convert((IField)item);
				}
				if (item is IProperty) {
					return ambience.Convert((IProperty)item);
				}
				if (item is IEvent) {
					return ambience.Convert((IEvent)item);
				}
				return text;
			}
			
			#region System.IComparable interface implementation
			public int CompareTo(object obj)
			{
				return ToString().CompareTo(obj.ToString());
			}
			#endregion
			
		}

        public QuickClassBrowserPanel():this(null)
        {
            
        }

		public QuickClassBrowserPanel(CodeEditorControl textAreaControl)
		{
			InitializeComponent();
			this.membersComboBox.MaxDropDownItems = 20;
			
			base.Dock = DockStyle.Top;
            if (textAreaControl != null)
                this.Editor = textAreaControl;
		}

        public CodeEditorControl Editor
        {
            get { return this.textAreaControl; }
            set{
                this.textAreaControl = value;
                this.textAreaControl.CaretChange += new EventHandler(CaretPositionChanged);
                }
        }
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
                if(this.textAreaControl != null) this.textAreaControl.CaretChange -= new EventHandler(CaretPositionChanged);
				this.membersComboBox.Dispose();
				this.classComboBox.Dispose();
			}
			base.Dispose(disposing);
		}

        public void PopulateCombo()
        {
            // ignore simple movements

            try
            {

                ParseInformation parseInfo = Parser.ProjectParser.GetParseInformation(textAreaControl.FileName);
                if (parseInfo != null)
                {
                    if (currentCompilationUnit != (ICompilationUnit)parseInfo.MostRecentCompilationUnit)
                    {
                        currentCompilationUnit = (ICompilationUnit)parseInfo.MostRecentCompilationUnit;
                        if (currentCompilationUnit != null)
                        {
                            FillClassComboBox(true);
                            FillMembersComboBox();
                        }
                    }
                    UpdateClassComboBox();
                    UpdateMembersComboBox();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
		void CaretPositionChanged(object sender, EventArgs e)
		{
            PopulateCombo();
		}
		
		bool membersComboBoxSelectedMember = false;
		void UpdateMembersComboBox()
		{
			autoselect = false;
			try {
				if (currentCompilationUnit != null) {
					for (int i = 0; i < membersComboBox.Items.Count; ++i) {
						if (((ComboBoxItem)membersComboBox.Items[i]).IsInside(textAreaControl.ActiveViewControl.Caret.Position.Y)) {
							if (membersComboBox.SelectedIndex != i) {
								membersComboBox.SelectedIndex = i;
							}
							if (!membersComboBoxSelectedMember) {
								membersComboBox.Refresh();
							}
							membersComboBoxSelectedMember = true;
							return;
						}
					}
				}
				membersComboBox.SelectedIndex = -1;
				if (membersComboBoxSelectedMember) {
					membersComboBox.Refresh();
					membersComboBoxSelectedMember = false;
				}
			} finally {
				autoselect = true;
			}
		}
		
		bool classComboBoxSelectedMember = false;
		void UpdateClassComboBox()
		{
			// Still needed ?
			if (currentCompilationUnit == null) {
				currentCompilationUnit = (ICompilationUnit)Parser.ProjectParser.GetParseInformation(Path.GetFullPath(textAreaControl.FileName)).MostRecentCompilationUnit;
			}
			
			autoselect = false;
			try {
				if (currentCompilationUnit != null) {
					//// Alex: when changing between files in different compilation units whole process must be restarted
					//// happens usually when files are opened from different project(s)
					for (int i = 0; i < classComboBox.Items.Count; ++i) {
						if (((ComboBoxItem)classComboBox.Items[i]).IsInside(textAreaControl.ActiveViewControl.Caret.Position.Y)) {
							bool innerClassContainsCaret = false;
							for (int j = i + 1; j < classComboBox.Items.Count; ++j) {
								if (((ComboBoxItem)classComboBox.Items[j]).IsInside(textAreaControl.ActiveViewControl.Caret.Position.Y)) {
									innerClassContainsCaret = true;
									break;
								}
							}
							if (!innerClassContainsCaret) {
								if (classComboBox.SelectedIndex != i) {
									classComboBox.SelectedIndex = i;
									FillMembersComboBox();
								}
								if (!classComboBoxSelectedMember) {
									classComboBox.Refresh();
								}
								classComboBoxSelectedMember = true;
								return;
							}
						}
					}
				}
				if (classComboBoxSelectedMember) {
					classComboBox.Refresh();
					classComboBoxSelectedMember = false;
				}
			} finally {
				autoselect = true;
			}
//				classComboBox.SelectedIndex = -1;
		}
		
		bool NeedtoUpdate(ArrayList items, ComboBox comboBox)
		{
			if (items.Count != comboBox.Items.Count) {
				return true;
			}
			for (int i = 0; i < items.Count; ++i) {
				ComboBoxItem oldItem = (ComboBoxItem)comboBox.Items[i];
				ComboBoxItem newItem = (ComboBoxItem)items[i];
				if (oldItem.GetType() != newItem.GetType()) {
					return true;
				}
				if (newItem.CompareItemTo(oldItem) != 0) {
					return true;
				}
			}
			return false;
		}
		
		IClass lastClassInMembersComboBox;
		
		void FillMembersComboBox()
		{
			IClass c = GetCurrentSelectedClass();
			if (c != null && lastClassInMembersComboBox != c) {
				lastClassInMembersComboBox = c;
				ArrayList items = new ArrayList();
				bool partialMode = false;
				IClass currentPart = c;
				if (c.IsPartial) {
					CompoundClass cc = c.GetCompoundClass() as CompoundClass;
					if (cc != null) {
						partialMode = true;
						c = cc;
					}
				}
				
				lock (c) {
					int lastIndex = 0;
					IComparer comparer = new Comparer(System.Globalization.CultureInfo.InvariantCulture);
					
					foreach (IMethod m in c.Methods) {
						items.Add(new ComboBoxItem(m, m.Name, (int)ScriptControl.GetIcon(m), partialMode ? currentPart.Methods.Contains(m) : true));
					}
					items.Sort(lastIndex, c.Methods.Count, comparer);
					lastIndex = items.Count;
					
					foreach (IProperty p in c.Properties) {
                        items.Add(new ComboBoxItem(p, p.Name,(int) ScriptControl.GetIcon(p), partialMode ? currentPart.Properties.Contains(p) : true));
					}
					items.Sort(lastIndex, c.Properties.Count, comparer);
					lastIndex = items.Count;
					
					foreach (IField f in c.Fields) {
                        items.Add(new ComboBoxItem(f, f.Name, (int)ScriptControl.GetIcon(f), partialMode ? currentPart.Fields.Contains(f) : true));
					}
					items.Sort(lastIndex, c.Fields.Count, comparer);
					lastIndex = items.Count;
					
					foreach (IEvent evt in c.Events) {
                        items.Add(new ComboBoxItem(evt, evt.Name, (int)ScriptControl.GetIcon(evt), partialMode ? currentPart.Events.Contains(evt) : true));
					}
					items.Sort(lastIndex, c.Events.Count, comparer);
					lastIndex = items.Count;
				}
				
				membersComboBox.BeginUpdate();
				membersComboBox.Items.Clear();
				membersComboBox.Items.AddRange(items.ToArray());
				membersComboBox.EndUpdate();
				UpdateMembersComboBox();
			}
		}
		
		void AddClasses(ArrayList items, ICollection classes)
		{
			foreach (IClass c in classes) {
                items.Add(new ComboBoxItem(c, c.FullyQualifiedName, (int)ScriptControl.GetIcon(c), true));
				AddClasses(items, c.InnerClasses);
			}
		}
		
		void FillClassComboBox(bool isUpdateRequired)
		{
			ArrayList items = new ArrayList();
			AddClasses(items, currentCompilationUnit.Classes);
			if (isUpdateRequired) {
				classComboBox.BeginUpdate();
			}
			classComboBox.Items.Clear();
			membersComboBox.Items.Clear();
			classComboBox.Items.AddRange(items.ToArray());
			if (items.Count == 1) {
				try {
					autoselect = false;
					classComboBox.SelectedIndex = 0;
					FillMembersComboBox();
				} finally {
					autoselect = true;
				}
			}
			if (isUpdateRequired) {
				classComboBox.EndUpdate();
			}
			UpdateClassComboBox();
		}
		
		
		// THIS METHOD IS MAINTAINED BY THE FORM DESIGNER
		// DO NOT EDIT IT MANUALLY! YOUR CHANGES ARE LIKELY TO BE LOST
		void InitializeComponent() {
            this.membersComboBox = new System.Windows.Forms.ComboBox();
            this.classComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // membersComboBox
            // 
            this.membersComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.membersComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.membersComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.membersComboBox.Location = new System.Drawing.Point(199, 4);
            this.membersComboBox.Name = "membersComboBox";
            this.membersComboBox.Size = new System.Drawing.Size(316, 21);
            this.membersComboBox.TabIndex = 1;
            this.membersComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ComboBoxDrawItem);
            this.membersComboBox.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSelectedIndexChanged);
            this.membersComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.MeasureComboBoxItem);
            // 
            // classComboBox
            // 
            this.classComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.classComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.classComboBox.Location = new System.Drawing.Point(4, 4);
            this.classComboBox.Name = "classComboBox";
            this.classComboBox.Size = new System.Drawing.Size(189, 21);
            this.classComboBox.Sorted = true;
            this.classComboBox.TabIndex = 0;
            this.classComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ComboBoxDrawItem);
            this.classComboBox.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSelectedIndexChanged);
            this.classComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.MeasureComboBoxItem);
            // 
            // QuickClassBrowserPanel
            // 
            this.Controls.Add(this.membersComboBox);
            this.Controls.Add(this.classComboBox);
            this.Name = "QuickClassBrowserPanel";
            this.Size = new System.Drawing.Size(520, 29);
            this.Resize += new System.EventHandler(this.QuickClassBrowserPanelResize);
            this.ResumeLayout(false);

		}
		
		public IClass GetCurrentSelectedClass()
		{
			if (classComboBox.SelectedIndex >= 0) {
				return (IClass)((ComboBoxItem)classComboBox.Items[classComboBox.SelectedIndex]).Item;
			}
			return null;
		}
		
		void ComboBoxSelectedIndexChanged(object sender, System.EventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
            if (textAreaControl == null) return;
			if (autoselect) {
				ComboBoxItem item = (ComboBoxItem)comboBox.Items[comboBox.SelectedIndex];
				if (item.IsInCurrentPart) {
					textAreaControl.ActiveViewControl.Caret.Position = new TextPoint(item.Column, item.Line);
                    textAreaControl.ActiveViewControl.ScrollIntoView();
                    textAreaControl.ActiveViewControl.Focus();
				} else {
					IMember m = item.Item as IMember;
					if (m != null) {
						string fileName = m.DeclaringType.CompilationUnit.FileName;
                        if (fileName == this.textAreaControl.FileName)
                        {
                            this.textAreaControl.ActiveViewControl.Caret.Position = new TextPoint(item.Column, item.Line);
                            textAreaControl.ActiveViewControl.ScrollIntoView();
                            textAreaControl.ActiveViewControl.Focus();
                        }
                        else
                        {
                            Document doc = (Document)this.textAreaControl.Parent;
                            ScriptControl sc = (ScriptControl)doc.ParentScriptControl;
                            Document docnew = sc.ShowFile(fileName);
                            if (docnew != null)
                            {
                                docnew.ParseContentsNow();
                                docnew.Editor.ActiveViewControl.Caret.Position = new TextPoint(item.Column, item.Line);
                                docnew.Editor.ActiveViewControl.ScrollIntoView();
                                docnew.Editor.ActiveViewControl.Focus();
                            }
                            else
                            {
                                docnew = ((Document)this.textAreaControl.Parent);
                                docnew.ParseContentsNow();
                                docnew.Editor.ActiveViewControl.Caret.Position = new TextPoint(item.Column, item.Line);
                                docnew.Editor.ActiveViewControl.ScrollIntoView();
                                docnew.Editor.ActiveViewControl.Focus();
                            }

                        }
					}
				}
				if (comboBox == classComboBox) {
					FillMembersComboBox();
					UpdateMembersComboBox();
				}
			}
		}
		
		// font - has to be static - don't create on each draw
		static Font font = font = new Font("Arial", 8.25f);
		static StringFormat drawStringFormat = new StringFormat(StringFormatFlags.NoWrap);
		
		void ComboBoxDrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			e.DrawBackground();
            if (this.textAreaControl == null) return;
			if (e.Index >= 0 ) {
				ComboBoxItem item = (ComboBoxItem)comboBox.Items[e.Index];
				
				e.Graphics.DrawImageUnscaled(this.textAreaControl.AutoListIcons.Images[item.IconIndex],
				                             new Point(e.Bounds.X, e.Bounds.Y + (e.Bounds.Height - this.textAreaControl.AutoListIcons.ImageSize.Height) / 2));
                Rectangle drawingRect = new Rectangle(e.Bounds.X + this.textAreaControl.AutoListIcons.ImageSize.Width,
				                                      e.Bounds.Y,
                                                      e.Bounds.Width - this.textAreaControl.AutoListIcons.ImageSize.Width,
				                                      e.Bounds.Height);
				
				Brush drawItemBrush = SystemBrushes.WindowText;
				if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
					drawItemBrush = SystemBrushes.HighlightText;
				}
				if (!item.IsInCurrentPart) {
					drawItemBrush = SystemBrushes.ControlDark;
				} else if (e.State == DrawItemState.ComboBoxEdit && !item.IsInside(textAreaControl.ActiveViewControl.Caret.Position.Y)) {
					drawItemBrush = SystemBrushes.ControlDark;
				}
				e.Graphics.DrawString(item.ToString(),
				                      font,
				                      drawItemBrush,
				                      drawingRect,
				                      drawStringFormat);
			}
			e.DrawFocusRectangle();
		}
		
		void QuickClassBrowserPanelResize(object sender, System.EventArgs e)
		{
			Size comboBoxSize = new Size(Width / 2 - 4 * 3, 21);
            classComboBox.Location = new Point(8, classComboBox.Bounds.Top);
			classComboBox.Size = comboBoxSize;
			membersComboBox.Location = new Point(classComboBox.Bounds.Right + 8, classComboBox.Bounds.Top);
			membersComboBox.Size = comboBoxSize;
		}
		
		void MeasureComboBoxItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			if (e.Index >= 0) {
				ComboBoxItem item = (ComboBoxItem)comboBox.Items[e.Index];
				SizeF size = e.Graphics.MeasureString(item.ToString(), font);
				e.ItemWidth  = (int)size.Width;
                if(this.textAreaControl== null)
                    e.ItemHeight = (int)Math.Max(size.Height, 16);
                else
                    e.ItemHeight = (int)Math.Max(size.Height, this.textAreaControl.AutoListIcons.ImageSize.Height);
			}
		}
	}
}
