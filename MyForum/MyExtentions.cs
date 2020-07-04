using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyForum.Models;

namespace MyForum
{
	public static class MyExtensions
	{

		public static ViewModels.Index.IndexViewModel.MessageViewModel CreateLastMessage(this Message m)
		{
			if (m == null)
				return null;
			if (m.Theme.IsHidden)
				return null;
			return new ViewModels.Index.IndexViewModel.MessageViewModel { MessageId = m.Id, Text = m.Text, AuthorId = m.Author.Id, AuthorName = m.Author.UserName, ThemeId = m.Theme.Id, CreatingTime = m.CreatingTime };
		}
	}
}
