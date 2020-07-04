using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyForum.Models
{
	public class Theme
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime CreatingTime { get; set; }
		public bool IsClosed { get; set; } = false;
		public bool IsHidden { get; set; } = false;

		public User Author { get; set; }
		
		public List<Message> Messages { get; set; }

		public List<ThemeSection> Sections { get; set; }

		public Theme()
		{
			Messages = new List<Message>();
			Sections = new List<ThemeSection>();
		}
	}
}
