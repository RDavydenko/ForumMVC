using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "Заполните обязательное поле")]
		[DataType(DataType.EmailAddress, ErrorMessage = "Некорректный адрес")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(16, MinimumLength = 4, ErrorMessage = "Допустимая длина от 4 до 16 символов")]
		public string Login { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[MinLength(6, ErrorMessage = "Минимальная длина - 6 символов")]
		[DataType(DataType.Password, ErrorMessage = "Пароль недостаточно надежный")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[Compare("Password", ErrorMessage = "Пароли не совпадают")]
		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }
	}
}
