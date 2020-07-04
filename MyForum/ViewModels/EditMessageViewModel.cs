using System;
using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class EditMessageViewModel
	{
		[Required]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[MinLength(3, ErrorMessage = "Минимальная длина - 3 символа")]
		[MaxLength(1000, ErrorMessage = "Максимальная длина - 1000 символов")]
		public string Text { get; set; }

		public DateTime? CreatingTime { get; set; }

		public Guid? AuthorId { get; set; }
		public string AuthorUserName { get; set; }
		public Guid? ThemeId { get; set; }
		public string ThemeTitle { get; set; }
	}
}
