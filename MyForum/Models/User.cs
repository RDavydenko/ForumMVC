using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MyForum.Models
{
	public class User : IdentityUser<Guid>
	{
		public string Status { get; set; }
		public DateTime RegistrationDate { get; set; }

		public bool IsSilenced { get; set; } = false;
		public DateTime? SilenceStartTime { get; set; } = null;
		public DateTime? SilenceStopTime { get; set; } = null;

		public int Year { get; set; }
		public List<Message> Messages { get; set; }
		public List<Theme> Themes { get; set; }

		public User()
		{
			Messages = new List<Message>();
			Themes = new List<Theme>();
		}
	}
}
