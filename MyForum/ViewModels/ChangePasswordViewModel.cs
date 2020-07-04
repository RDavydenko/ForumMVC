using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class ChangePasswordViewModel
	{
		[Required(ErrorMessage = "Заполните обязательное поле")]
		public string OldPassword { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[MinLength(6, ErrorMessage = "Минимальная длина - 6 символов")]
		[DataType(DataType.Password, ErrorMessage = "Пароль недостаточно надежный")]
		public string NewPassword { get; set; }
	}
}
