using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels.AdminPanel
{
	public class UserEditAdminPanelViewModel
	{
		[Required]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(16, MinimumLength = 4, ErrorMessage = "Допустимая длина от 4 до 16 символов")]
		public string Login { get; set; }

		[MaxLength(64, ErrorMessage = "Максимальная длина - 64 символа")]
		public string Status { get; set; }

		public IEnumerable<string> Roles { get; set; }
		public IEnumerable<RoleNameIsAdded> RolesHelper { get; set; }

		public class RoleNameIsAdded
		{
			public string Name { get; set; }
			public bool IsAdded { get; set; } = false;
		}
	}
}
