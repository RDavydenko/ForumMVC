using System;
using System.Collections.Generic;

namespace MyForum.ViewModels.AdminPanel
{
	public class SectionsAdminPanelViewModel
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public int ThemesCount { get; set; }
		public bool IsHidden { get; set; }
		public IEnumerable<ThemesIdTitle> Themes { get; set; }

		public class ThemesIdTitle
		{
			public Guid Id { get; set; }
			public string Title { get; set; }
		}
	}
}
