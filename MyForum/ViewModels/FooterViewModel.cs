using System;
using System.Collections.Generic;

namespace MyForum.ViewModels
{
	public class FooterViewModel
	{
		public IEnumerable<SectionViewModel> Sections;
		public IEnumerable<ThemeViewModel> Themes;
		public IEnumerable<UserViewModel> Authors;
		public int MessagesCount { get; set; }
		public int ThemesCount { get; set; }
		public int UsersCount { get; set; }

		public class SectionViewModel
		{
			public Guid Id { get; set; }
			public string Title { get; set; }
			public int ThemesCount { get; set; }
		}
		public class ThemeViewModel
		{
			public Guid Id { get; set; }
			public string Title { get; set; }
			public int MessagesCount { get; set; }
		}
		public class UserViewModel
		{
			public Guid Id { get; set; }
			public string UserName { get; set; }
			public int MessagesCount { get; set; }
		}
	}
}
