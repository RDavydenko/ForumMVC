using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class CreateSectionViewModel
	{
		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(100, MinimumLength = 3, ErrorMessage = "Допустимая длина - от 3 до 100 символов")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(500, MinimumLength = 3, ErrorMessage = "Допустимая длина - от 3 до 500 символов")]
		public string Description { get; set; }

		public bool IsHidden { get; set; } = false;
	}
}
