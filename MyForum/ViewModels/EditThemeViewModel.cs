using System;
using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class EditThemeViewModel
	{
		[Required]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(100, MinimumLength = 10, ErrorMessage = "Допустимая длина от 10 до 100 символов")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(1000, MinimumLength = 10, ErrorMessage = "Допустимая длина от 10 до 1000 символов")]
		public string Description { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		public bool IsHidden { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		public bool IsClosed { get; set; }

		public Guid? AuthorId { get; set; }

		public string AuthorUserName { get; set; }
	}
}
