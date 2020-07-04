using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyForum.Models
{
	// Вспомогательный класс для организации связи МНОГИЕ ко МНОГИМ
	// для таблиц Themes и Sections
	[Table("ThemeSections")]
	public class ThemeSection
	{
		public Guid Id { get; set; }

		public Theme Theme { get; set; }

		public Section Section{ get; set; }
	}
}
