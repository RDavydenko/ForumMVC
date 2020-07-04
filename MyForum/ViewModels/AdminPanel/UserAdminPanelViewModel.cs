using System;
using System.Collections.Generic;

namespace MyForum.ViewModels.AdminPanel
{
	public class UserAdminPanelViewModel
	{
		public Guid Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Status { get; set; }
		public int MessagesCount { get; set; }
		public int ThemesCount { get; set; }
		public IList<string> RoleNames { get; set; }
		public DateTime RegistrationDate { get; set; }
		public bool IsSilenced { get; set; }
		public DateTime? SilenceStopTime { get; set; }
	}
}
