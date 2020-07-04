using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class UserEditProfileViewModel
	{
		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(16, MinimumLength = 4, ErrorMessage = "Допустимая длина от 4 до 16 символов")]
		public string Login { get; set; }

		[MaxLength(64, ErrorMessage = "Максимальная длина - 64 символа")]
		public string Status { get; set; }
	}
}
