using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "Заполните обязательное поле")]
		public string EmailOrLogin { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		public string ReturnUrl { get; set; }
	}
}
