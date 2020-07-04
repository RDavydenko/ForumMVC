using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyForum.ViewModels
{
	public class EditCategoryViewModel
	{
		[Required]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		[StringLength(100, MinimumLength = 3, ErrorMessage = "Допустимая длина - от 3 до 100 символов")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Заполните обязательное поле")]
		public bool IsHidden { get; set; } = false;

		public List<SectionViewModel> Sections { get; set; }

		public class SectionViewModel
		{
			public Guid Id { get; set; }
			public string Title { get; set; }
			public string Description { get; set; }
			public bool IsHidden { get; set; }

			public Guid? CategoryId { get; set; }
		}
	}
}
