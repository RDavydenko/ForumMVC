using System;
using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels.AdminPanel
{
	public class RoleEditAdminPanelViewModel
	{
		[Required]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "Заполните обзятаельное поле")]
		[StringLength(32, MinimumLength = 4, ErrorMessage = "Допустимая длина - от 4 до 32 символов")]
		public string Name { get; set; }
	}
}
