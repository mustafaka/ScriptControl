using System;
using System.Drawing;

namespace AIMS.Libraries.Forms.Docking
{
	internal class AutoHideTabVS2003 : AutoHideTab
	{
		internal AutoHideTabVS2003(IDockableWindow content) : base(content)
		{
		}

		private int m_tabX = 0;
		protected internal int TabX
		{
			get	{	return m_tabX;	}
			set	{	m_tabX = value;	}
		}

		private int m_tabWidth = 0;
		protected internal int TabWidth
		{
			get	{	return m_tabWidth;	}
			set	{	m_tabWidth = value;	}
		}

	}
}
