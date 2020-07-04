using System;
using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class NewThemeViewModel
	{
		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(100, MinimumLength = 10, ErrorMessage = "Допустимая длина от 10 до 100 символов")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(1000, MinimumLength = 10, ErrorMessage = "Допустимая длина от 10 до 1000 символов")]
		public string Description { get; set; }

		public Guid? SectionId { get; set; }
		public string SectionTitle { get; set; }
	}
}
