

using System;
using System.Drawing;

namespace AIMS.Libraries.Forms.Docking
{
	/// <include file='CodeDoc/DockPaneTab.xml' path='//CodeDoc/Class[@name="DockPaneTab"]/ClassDef/*'/>
	public class DockPaneTab : IDisposable
	{
		private IDockableWindow m_content;

		/// <include file='CodeDoc/DockPaneTab.xml' path='//CodeDoc/Class[@name="DockPaneTab"]/Construct[@name="(IDockContent)"]/*'/>
		public DockPaneTab(IDockableWindow content)
		{
			m_content = content;
		}

		/// <exclude/>
		~DockPaneTab()
		{
			Dispose(false);
		}

		/// <include file='CodeDoc/DockPaneTab.xml' path='//CodeDoc/Class[@name="DockPaneTab"]/Property[@name="Content"]/*'/>
		public IDockableWindow Content
		{
			get	{	return m_content;	}
		}

		/// <include file='CodeDoc/DockPaneTab.xml' path='//CodeDoc/Class[@name="DockPaneTab"]/Method[@name="Dispose"]/*'/>
		/// <include file='CodeDoc/DockPaneTab.xml' path='//CodeDoc/Class[@name="DockPaneTab"]/Method[@name="Dispose()"]/*'/>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <include file='CodeDoc/DockPaneTab.xml' path='//CodeDoc/Class[@name="DockPaneTab"]/Method[@name="Dispose(bool)"]/*'/>
		protected virtual void Dispose(bool disposing)
		{
		}
	}
}
