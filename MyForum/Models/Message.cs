using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyForum.Models
{
	public class Message
	{
		public Guid Id { get; set; }
		public string Text { get; set; }
		public DateTime CreatingTime { get; set; }

		public User Author { get; set; }

		public Theme Theme { get; set; }		
	}
}
