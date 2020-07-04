using System;
using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class EditSectionViewModel
	{
		[Required]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(100, MinimumLength = 3, ErrorMessage = "Допустимая длина от 3 до 100 символов")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(200, MinimumLength = 3, ErrorMessage = "Допустимая длина от 3 до 200 символов")]
		public string Description { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]		
		public bool IsHidden { get; set; }
	}
}
