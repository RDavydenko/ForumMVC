using System;

namespace MyForum.ViewModels.AdminPanel
{
	public class ThemesAdminPanelViewModel
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public Guid CreatorId { get; set; }
		public string CreatorName { get; set; }
		public string CreatingTime { get; set; }
		public int MessagesCount { get; set; }
		public bool IsClosed{ get; set; }
		public bool IsHidden { get; set; }		
	}
}
