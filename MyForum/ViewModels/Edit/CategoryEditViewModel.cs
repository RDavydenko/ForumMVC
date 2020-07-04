using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyForum.ViewModels.Edit
{
	public class CategoryEditViewModel
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public bool IsHidden { get; set; }

		public List<SectionEditViewModel> Sections{ get; set; }

		public class SectionEditViewModel
		{
			public Guid SectionId { get; set; }
		}
	}
}
