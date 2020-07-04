using System;
using System.Collections.Generic;

namespace MyForum.ViewModels.Index
{
	public class IndexViewModel
	{
		public CategoryViewModel Category { get; set; }

		public class CategoryViewModel
		{
			public Guid Id { get; set; }
			public string Title { get; set; }
			public bool IsHidden { get; set; }

			public List<SectionViewModel> Sections { get; set; }
		}

		public class SectionViewModel
		{
			public Guid Id { get; set; }
			public string Title { get; set; }
			public string Description { get; set; }
			public bool IsHidden { get; set; }

			public int ThemesCount { get; set; }
			public int MessagesCount { get; set; }

			public MessageViewModel LastMessage { get; set; }
		}

		public class MessageViewModel
		{
			public Guid MessageId { get; set; }
			public string Text { get; set; }
			public Guid ThemeId { get; set; }
			public Guid AuthorId { get; set; }
			public string AuthorName { get; set; }
			public DateTime CreatingTime { get; set; }
		}
	}
}
