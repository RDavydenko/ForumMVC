using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class CreateCategoryViewModel
	{
		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(100, MinimumLength = 3, ErrorMessage = "Допустимая длина - от 3 до 100 символов")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		public bool IsHidden { get; set; } = false;
	}
}
