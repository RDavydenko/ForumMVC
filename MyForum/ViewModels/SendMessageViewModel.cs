using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class SendMessageViewModel
	{
		[Required(ErrorMessage = "Напишите сообщение")]
		[MinLength(3, ErrorMessage = "Минимальная длина - 3 символа")]
		public string Text { get; set; }
	}
}
