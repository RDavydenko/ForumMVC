using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyForum.Models
{
	public class Report
	{
		public Guid Id { get; set; }

		public string ObjectType { get; set; }
		public Guid ObjectId { get; set; }
		public string ObjectName { get; set; }
		public string Text { get; set; }
		public bool IsChecked { get; set; } = false;
		public DateTime SendingTime { get; set; }
		public User Sender { get; set; }
	}
}
