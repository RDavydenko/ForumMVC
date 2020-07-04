using System;
using System.Collections.Generic;

namespace MyForum.ViewModels.AdminPanel
{
	public class CategoriesAdminPanelViewModel
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public bool IsHidden { get; set; }
		public IEnumerable<SectionIdTitle> Sections { get; set; }


		public class SectionIdTitle
		{
			public Guid Id { get; set; }
			public string Title { get; set; }
		}
	}
}
