using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyForum.Models
{
	public class Section
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public bool IsHidden { get; set; } = false;

		public List<ThemeSection> Themes { get; set; }
		public Category Category { get; set; }

		public Section()
		{
			Themes = new List<ThemeSection>();
		}
	}
}
