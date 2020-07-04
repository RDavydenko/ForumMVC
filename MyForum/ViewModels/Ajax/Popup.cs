using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyForum.ViewModels.Ajax
{
	public class Popup
	{
		public string Type { get; set; } // success or danger and etc. (LOWERCASE!!!)
		public string Text { get; set; } // For example: Раздел успешно <strong>обновлен</strong>
	}
}
