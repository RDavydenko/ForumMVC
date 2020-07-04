using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyForum.Models;
using MyForum.ViewModels;
using MyForum.ViewModels.AdminPanel;
using MyForum.ViewModels.Ajax;
using MyForum.ViewModels.Edit;
using MyForum.ViewModels.Index;

namespace MyForum.Controllers
{
	public class ForumController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<Role> _roleManager;
		private readonly ForumContext _forumContext;
		private readonly SignInManager<User> _signInManager;

		public ForumController(UserManager<User> userManager, RoleManager<Role> roleManager, ForumContext forumContext, SignInManager<User> signInManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_forumContext = forumContext;
			_signInManager = signInManager;
			InitializeAsync().Wait();
		}


		// ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ

		private async Task InitializeAsync()
		{
			if (_forumContext.Messages.Count() == 0 && _forumContext.Themes.Count() == 0 && _forumContext.Categories.Count() == 0 && _userManager.Users.Count() == 0)
			{
				await _userManager.CreateAsync(new User { Email = "ishadows888@gmail.com", UserName = "ishadows888@gmail.com" }, "123456");
				User author = await _forumContext.Users.FirstOrDefaultAsync();

				Theme theme = new Theme { Author = author, CreatingTime = DateTime.Now };
				await _forumContext.Themes.AddAsync(theme);
				await _forumContext.SaveChangesAsync();

				Message msg1 = new Message { Text = "Ребята, помогите, срочно нужна помощь!!!", Author = author, CreatingTime = DateTime.Now, Theme = theme };
				Message msg2 = new Message { Text = "Не отчаивайся, работяга, щас помогу тебе", Author = author, CreatingTime = DateTime.Now, Theme = theme };
				await _forumContext.Messages.AddRangeAsync(msg1, msg2);
				await _forumContext.SaveChangesAsync();

				theme.Title = msg1.Text;
				theme.Description = "Тут такая проблема возникла, в общем в силу обстоятельств и из-за нелепой случайности произошла проблема, которую мне непосильно решить, и я решил спросить помощи у вас...";
				_forumContext.Themes.Update(theme);
				await _forumContext.SaveChangesAsync();

				Category category1 = new Category { Title = "Тестовая-1" };
				Category category2 = new Category { Title = "Тестовая-2" };
				await _forumContext.Categories.AddRangeAsync(category1, category2);
				await _forumContext.SaveChangesAsync();

				Section sec1 = new Section { Title = "Раздел-1", Description = "Это обычный тестовый раздел с тестовыми темами", Category = category1 };
				await _forumContext.Sections.AddAsync(sec1);
				await _forumContext.SaveChangesAsync();

				ThemeSection themeSection1 = new ThemeSection { Section = sec1, Theme = theme };
				await _forumContext.ThemeSections.AddRangeAsync(themeSection1);
				await _forumContext.SaveChangesAsync();
			}
		}

		private void InitializeFooterViewModelInViewBag()
		{
			var sections = _forumContext.Sections
				.AsNoTracking()
				.Include(s => s.Themes)
				.ThenInclude(ts => ts.Theme)
				.OrderByDescending(s => s.Themes.Count)
				.Where(s => !s.IsHidden)
				.Select(s => new FooterViewModel.SectionViewModel { Id = s.Id, Title = s.Title, ThemesCount = s.Themes.Count })
				.Take(5);

			var themes = _forumContext.Themes
				.AsNoTracking()
				.Include(t => t.Messages)
				.OrderByDescending(t => t.Messages.Count)
				.Where(t => !t.IsHidden)
				.Select(t => new FooterViewModel.ThemeViewModel { Id = t.Id, Title = t.Title, MessagesCount = t.Messages.Count })
				.Take(5);

			var authors = _forumContext.Users
				.AsNoTracking()
				.Include(u => u.Messages)
				.OrderByDescending(u => u.Messages.Count)
				.Select(u => new FooterViewModel.UserViewModel { Id = u.Id, UserName = u.UserName, MessagesCount = u.Messages.Count })
				.Take(5);

			ViewBag.FooterViewModel = new FooterViewModel { Sections = sections, Themes = themes, Authors = authors, MessagesCount = _forumContext.Messages.Count(), ThemesCount = _forumContext.Themes.Count(), UsersCount = _forumContext.Users.Count() };
		}

		private async Task CreateReportAsync(Guid id, string objectType, string text, User sender)
		{
			var newReport = new Report { ObjectType = objectType, ObjectId = id, Sender = sender, Text = text };
			if (objectType == "message")
				newReport.ObjectName = (await _forumContext.Messages.FirstOrDefaultAsync(m => m.Id == id))?.Text;
			else if (objectType == "theme")
				newReport.ObjectName = (await _forumContext.Themes.FirstOrDefaultAsync(t => t.Id == id))?.Title;
			else if (objectType == "section")
				newReport.ObjectName = (await _forumContext.Sections.FirstOrDefaultAsync(s => s.Id == id))?.Title;
			else if (objectType == "category")
				newReport.ObjectName = (await _forumContext.Categories.FirstOrDefaultAsync(c => c.Id == id))?.Title;
			else if (objectType == "user")
				newReport.ObjectName = (await _forumContext.Users.FirstOrDefaultAsync(u => u.Id == id))?.UserName;
			else if (objectType == "role")
				newReport.ObjectName = (await _forumContext.Roles.FirstOrDefaultAsync(r => r.Id == id))?.Name;
			_forumContext.Reports.Add(newReport);
			await _forumContext.SaveChangesAsync();
		}

		private async Task<string> CalculateUrlToThemeByMessageAsync(Guid messageId, Guid themeId)
		{
			int countMessagesOnPage = 20; // 20 сообщений отображается на странице
			if (messageId != Guid.Empty && themeId != Guid.Empty)
			{
				var theme = await _forumContext.Themes
					.Include(t => t.Messages)
					.FirstOrDefaultAsync(t => t.Id == themeId);

				if (theme != null)
				{
					var message = await _forumContext.Messages
						.Include(m => m.Theme)
						.FirstOrDefaultAsync(m => m.Id == messageId);

					if (message != null && message.Theme.Id == theme.Id)
					{
						uint page = 1;
						var messages = await _forumContext.Messages
							.Include(m => m.Theme)
							.Where(m => m.Theme.Id == themeId)
							.OrderByDescending(m => m.CreatingTime)
							.ToListAsync();

						// Ищем сообщение в Сообщениях (расчитываем страницу)
						for (int i = 0; i < messages.Count; i++)
						{
							if (messages[i].Id == message.Id)
							{
								break;
							}
							if (i % countMessagesOnPage == 0 && i != 0)
								page++;
						}
						return $"/Forum/{nameof(Theme)}?id=" + themeId.ToString() + "&page=" + page.ToString() + "#message-" + messageId.ToString();
					}
				}
			}
			return null;
		}


		// ГЛАВНАЯ

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var model = await _forumContext.Categories
				.AsNoTracking()
				.Where(c => !c.IsHidden)
				.Include(c => c.Sections)
				.ThenInclude(s => s.Themes)
				.ThenInclude(t => t.Theme)
				.ThenInclude(t => t.Messages)
				.Select(c => new IndexViewModel { Category = new IndexViewModel.CategoryViewModel { Id = c.Id, Title = c.Title, IsHidden = c.IsHidden, Sections = c.Sections.Where(s => !s.IsHidden).Select(s => new IndexViewModel.SectionViewModel { Id = s.Id, Title = s.Title, Description = s.Description, IsHidden = s.IsHidden, ThemesCount = s.Themes.Where(t => !t.Theme.IsHidden).Count() }).ToList() } })
				.ToListAsync();

			foreach (var m in model)
			{
				foreach (var section in m.Category.Sections)
				{
					section.LastMessage = _forumContext.Sections
						.AsNoTracking()
						.Include(s => s.Themes)
						.ThenInclude(ts => ts.Theme)
						.ThenInclude(t => t.Messages)
						.ThenInclude(m => m.Author)
						.Where(s => s.Id == section.Id)
						.FirstOrDefault()?.Themes
						.OrderByDescending(ts => ts.Theme.Messages.OrderByDescending(m => m.CreatingTime).FirstOrDefault()?.CreatingTime)
						.FirstOrDefault()?.Theme.Messages
						.OrderByDescending(m => m.CreatingTime)
						.FirstOrDefault()?.CreateLastMessage();
					section.MessagesCount = _forumContext.Sections
						.AsNoTracking()
						.Include(s => s.Themes)
						.ThenInclude(ts => ts.Theme)
						.ThenInclude(t => t.Messages)
						.Where(s => s.Id == section.Id)
						.FirstOrDefault()
						.Themes
						.Where(t => !t.Theme.IsHidden)
						.Sum(ts => ts.Theme.Messages.Count);
				}
			}
			InitializeFooterViewModelInViewBag();
			return View(model);
		}

		[HttpGet]
		public IActionResult Information() => View();

		[HttpGet]
		public IActionResult FeedBack() => View();

		// СООБЩЕНИЕ

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> SendMessage(string text, Guid themeId)
		{
			if (themeId == null || themeId == Guid.Empty)
				return BadRequest();

			var theme = await _forumContext.Themes.FirstOrDefaultAsync(t => t.Id == themeId);
			if (theme == null)
				return NotFound();
			else if (!theme.IsClosed)
			{
				var user = await _userManager.GetUserAsync(User);
				if (user == null)
					return NotFound();
				if (user.IsSilenced)
					return Forbid();
				if (text.Length < 3 || text.Length > 1000)
					return BadRequest();
				var message = new Message { Author = user, CreatingTime = DateTime.Now, Text = text, Theme = theme };
				_forumContext.Messages.Add(message);
				await _forumContext.SaveChangesAsync();
				return Ok();
			}
			return Forbid();
		}

		// Возможности по кнопке на сообщениях

		[HttpGet]
		public async Task<IActionResult> OneMessage(Guid id)
		{
			if (id != null && id != Guid.Empty)
			{
				var message = await _forumContext.Messages
					.Include(m => m.Author)
					.Include(m => m.Theme)
					.FirstOrDefaultAsync(m => m.Id == id);
				if (message == null)
					return NotFound();
				else
					return View(message);
			}
			else
				return BadRequest();
		}

		[HttpGet]
		public async Task<IActionResult> YakorLink(Guid messageId, Guid themeId)
		{
			return Json("https://" + HttpContext.Request.Host.Value + await CalculateUrlToThemeByMessageAsync(messageId, themeId));
		}

		[HttpPost]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> SilenceUser(Guid id, int minutes, bool needTimeReturn = false)
		{
			if (minutes > 0)
			{
				var silencedUser = await _forumContext.Users.FirstOrDefaultAsync(u => u.Id == id);
				if (silencedUser != null && !silencedUser.IsSilenced)
				{
					silencedUser.IsSilenced = true;
					silencedUser.SilenceStartTime = DateTime.Now;
					silencedUser.SilenceStopTime = DateTime.Now + TimeSpan.FromMinutes(minutes);
					_forumContext.Users.Update(silencedUser);
					await _forumContext.SaveChangesAsync();

					var sender = await _userManager.GetUserAsync(User);
					await CreateReportAsync(id, "user", $"Пользователь заморожен на {minutes} минут", sender);
					if (needTimeReturn)
						return Ok(silencedUser.SilenceStopTime?.ToString("dd.MM.yyyy HH:mm"));
					else
						return Ok();
				}
				return NotFound();
			}
			return BadRequest();
		}

		[HttpPost]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> UnsilenceUser(Guid id)
		{
			var silencedUser = await _forumContext.Users.FirstOrDefaultAsync(u => u.Id == id);
			if (silencedUser != null && silencedUser.IsSilenced)
			{
				silencedUser.IsSilenced = false;
				silencedUser.SilenceStartTime = null;
				silencedUser.SilenceStopTime = null;
				_forumContext.Users.Update(silencedUser);
				await _forumContext.SaveChangesAsync();

				var sender = await _userManager.GetUserAsync(User);
				await CreateReportAsync(id, "user", "Пользователь разморожен", sender);
				return Ok();
			}
			return NotFound();
		}

		[HttpPost]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> DeleteMessage(Guid id)
		{
			if (id != null && id != Guid.Empty)
			{
				var message = await _forumContext.Messages.Include(m => m.Author).Include(m => m.Theme).FirstOrDefaultAsync(m => m.Id == id);
				if (message == null)
					return NotFound();
				else
				{
					var user = await _userManager.GetUserAsync(User);
					if (User.IsInRole("admin") || User.IsInRole("moder") || message.Author.Id == user.Id)
					{
						var firstMessage = await _forumContext.Messages
							.Where(m => m.Theme.Id == message.Theme.Id)
							.OrderBy(m => m.CreatingTime)
							.FirstOrDefaultAsync();
						if (firstMessage.Id == message.Id) // Нельзя удалить первое сообщение, т.к. оно является описанием для темы (description)
							return Problem();

						_forumContext.Messages.Remove(message);
						await _forumContext.SaveChangesAsync();
						return Ok();
					}
					else
						return Forbid();
				}
			}
			return BadRequest();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> SaveMessage(EditMessageViewModel model)
		{
			if (ModelState.IsValid)
			{
				var message = await _forumContext.Messages
					.Include(m => m.Theme)
					.Where(x => x.Id == model.Id)
					.FirstOrDefaultAsync();

				var firstMessageInTheme = await _forumContext.Messages
					.Include(m => m.Theme)
					.OrderBy(m => m.CreatingTime)
					.Where(m => m.Theme.Id == message.Theme.Id)
					.FirstOrDefaultAsync();

				if (message != null)
				{
					if (message.Id == firstMessageInTheme.Id)
					{
						ModelState.AddModelError("", "Нельзя изменить первое сообщение, так как оно является описанием для темы");
						return View("EditMessage", model);
					}
					message.Text = model.Text;
					_forumContext.Messages.Update(message);
					await _forumContext.SaveChangesAsync();
					await CreateReportAsync(model.Id, "message", "Изменил(а) сообщение", await _userManager.GetUserAsync(User));
					return RedirectToAction("Theme", "Forum", new { id = message.Theme.Id });
				}
			}
			return View("EditMessage", model);
		}


		// ТЕМА

		[HttpGet]
		public async Task<IActionResult> Theme(Guid id, uint page = 1)
		{
			page = page == 0 ? 1 : page; // Не допустить страницу номер 0 
			int countMessagesOnPage = 20; // 20 сообщений отображается на странице
			if (id != Guid.Empty)
			{
				var theme = await _forumContext.Themes
					.Include(t => t.Author)
					.FirstOrDefaultAsync(t => t.Id == id);

				if (theme != null)
				{
					if (theme.IsHidden && !(User.IsInRole("moder") || User.IsInRole("admin"))) // Если тема скрыта, то у обычных пользователей нет доступа
						return RedirectToAction("Index", "Forum");

					var countMessagesInTheme = await _forumContext.Messages
						.Include(m => m.Theme)
						.Where(m => m.Theme.Id == theme.Id)
						.CountAsync();
					var maxPage = (int)Math.Ceiling(countMessagesInTheme / (double)countMessagesOnPage);
					page = (int)page > maxPage ? (uint)maxPage : page; // Если запрошенная страница больше максимальной страницы

					var messages = _forumContext.Messages
						.Include(m => m.Author)
						.ThenInclude(u => u.Messages)
						.Where(m => m.Theme.Id == theme.Id)
						.OrderBy(m => m.CreatingTime)
						.Skip((int)(page - 1) * countMessagesOnPage)
						.Take(countMessagesOnPage);

					ViewBag.Messages = messages;
					ViewBag.CurrentPage = (int?)page;
					ViewBag.MaxPage = (int?)maxPage;
					ViewBag.IsThemeClosed = (bool?)theme.IsClosed;
					ViewBag.IsHidden = (bool?)theme.IsHidden;

					if (User.Identity.IsAuthenticated)
						ViewBag.User = await _userManager.GetUserAsync(User);
					return View();
				}
			}
			return RedirectToAction("Index", "Forum");
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> CreateTheme(Guid sectionId)
		{
			var user = await _userManager.GetUserAsync(User);
			if (!user.IsSilenced)
			{
				if (sectionId == Guid.Empty && (User.IsInRole("moder") || User.IsInRole("admin")))
				{
					return View(new NewThemeViewModel());
				}

				var section = await _forumContext.Sections
					.Where(s => s.Id == sectionId)
					.FirstOrDefaultAsync();
				if (section != null)
				{
					var model = new NewThemeViewModel { SectionId = sectionId, SectionTitle = section.Title };
					return View(model);
				}
			}
			return RedirectToAction("Index", "Forum");
		}

		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateTheme(NewThemeViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.GetUserAsync(User);
				if (user.IsSilenced)
					return RedirectToAction("Index", "Forum");

				var section = await _forumContext.Sections
					.Where(s => s.Id == model.SectionId)
					.FirstOrDefaultAsync();
				if (section == null && !(User.IsInRole("moder") || User.IsInRole("admin")))
				{
					return RedirectToAction("Index", "Forum");
				}

				var theme = new Theme { Author = user, Title = model.Title, Description = model.Description };
				_forumContext.Themes.Add(theme);

				var mainMessage = new Message { Author = user, Text = model.Description, Theme = theme };
				_forumContext.Messages.Add(mainMessage);

				theme.Messages = new List<Message> { mainMessage };
				await _forumContext.SaveChangesAsync();

				if (section != null)
				{
					theme.Sections = new List<ThemeSection> { new ThemeSection { Section = section, Theme = theme } };
				}
				_forumContext.Themes.Update(theme);

				await _forumContext.SaveChangesAsync();
				return RedirectToAction("Theme", "Forum", new { id = theme.Id });
			}
			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> ThemeByMessage(Guid messageId, Guid themeId)
		{
			string path = await CalculateUrlToThemeByMessageAsync(messageId, themeId);
			if (path != null)
				return Redirect(path);
			else
				return RedirectToAction("Index", "Forum");
		}

		[HttpPost]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> CloseTheme(Guid id)
		{
			if (id != null && id != Guid.Empty)
			{
				var theme = await _forumContext.Themes.Where(t => t.Id == id).FirstOrDefaultAsync();
				if (theme != null)
				{
					theme.IsClosed = true;
					_forumContext.Themes.Update(theme);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			return BadRequest();
		}

		[HttpPost]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> OpenTheme(Guid id)
		{
			if (id != null && id != Guid.Empty)
			{
				var theme = await _forumContext.Themes.Where(t => t.Id == id).FirstOrDefaultAsync();
				if (theme != null)
				{
					theme.IsClosed = false;
					_forumContext.Themes.Update(theme);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			return BadRequest();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> SaveTheme(EditThemeViewModel model)
		{
			if (ModelState.IsValid)
			{
				var theme = await _forumContext.Themes.Include(t => t.Messages).Where(x => x.Id == model.Id).FirstOrDefaultAsync();
				if (theme != null)
				{
					var mainMessage = theme.Messages.OrderBy(m => m.CreatingTime).FirstOrDefault();
					theme.Title = model.Title;
					theme.Description = model.Description;
					mainMessage.Text = model.Description;
					theme.IsClosed = model.IsClosed;
					theme.IsHidden = model.IsHidden;
					_forumContext.Themes.Update(theme);
					_forumContext.Messages.Update(mainMessage);
					await _forumContext.SaveChangesAsync();
					await CreateReportAsync(model.Id, "theme", "Изменил(а) тему", await _userManager.GetUserAsync(User));
					return RedirectToAction("Theme", "Forum", new { id = model.Id });
				}
			}
			return View("EditTheme", model);
		}


		// РАЗДЕЛ

		[HttpGet]
		public async Task<IActionResult> Section(Guid id)
		{
			if (id != Guid.Empty)
			{
				var section = await _forumContext.Sections
					.Include(s => s.Themes)
					.ThenInclude(t => t.Theme.Messages)
					.ThenInclude(m => m.Author)
					.FirstOrDefaultAsync(s => s.Id == id);

				if (section != null)
				{
					if (section.IsHidden && !(User.IsInRole("moder") || User.IsInRole("admin"))) // Если раздел скрыт, то у обычных пользователей нет доступа
						return RedirectToAction("Index", "Forum");

					// Убираем скрытые темы из раздела
					for (int i = 0; i < section.Themes.Count; i++)
					{
						if (section.Themes[i].Theme.IsHidden)
						{
							section.Themes.RemoveAt(i);
							i--;
						}
					}

					return View(section);
				}
			}
			return RedirectToAction("Index", "Forum");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> SaveSection(Guid id, string title, string description, bool isHidden, List<Guid> themeIds, Guid categoryId)
		{
			if (ModelState.IsValid)
			{
				if (title.Trim().Length < 3 || title.Trim().Length > 100)
					return BadRequest(new { Popups = new List<Popup> { new Popup { Type = "danger", Text = "Ошибка! Название не удовлетворяет требованиям по длине" } } });

				if (description.Trim().Length < 3 || description.Trim().Length > 500)
					return BadRequest(new { Popups = new List<Popup> { new Popup { Type = "danger", Text = "Ошибка! Описание не удовлетворяет требованиям по длине" } } });

				var section = await _forumContext.Sections
					.Include(s => s.Themes)
					.ThenInclude(t => t.Theme)
					.Where(c => c.Id == id)
					.FirstOrDefaultAsync();

				if (section != null)
				{
					section.IsHidden = isHidden;
					section.Title = title;
					section.Description = description;

					List<ThemeSection> selectedThemeSections = new List<ThemeSection>();
					foreach (var themeId in themeIds)
					{
						var theme = await _forumContext.Themes
							.Where(t => t.Id == themeId)
							.FirstOrDefaultAsync();
						if (theme != null)
						{
							selectedThemeSections.Add(new ThemeSection { Theme = theme, Section = section });
						}
					}

					var themeSectionsNeedDeleted = await _forumContext.ThemeSections
						.Where(ts => ts.Section.Id == section.Id)
						.Include(ts => ts.Section)
						.Include(ts => ts.Theme)
						.ToListAsync();

					var category = await _forumContext.Categories
						.Include(c => c.Sections)
						.Where(c => c.Id == categoryId)
						.FirstOrDefaultAsync();
					if (category != null && !category.Sections.Contains(section))
					{
						category.Sections.Add(section);
						_forumContext.Categories.Update(category);
					}

					_forumContext.ThemeSections.RemoveRange(themeSectionsNeedDeleted);
					_forumContext.ThemeSections.AddRange(selectedThemeSections);
					_forumContext.Sections.Update(section);
					await _forumContext.SaveChangesAsync();
					return Ok(new { Popups = new List<Popup> { new Popup { Type = "success", Text = "Успешно! Раздел обновлен" } } });
				}
				return NotFound(new { Popups = new List<Popup> { new Popup { Type = "danger", Text = "Ошибка! Раздел не найдет, возможно он удален" } } });
			}
			return BadRequest(new { Popups = new List<Popup> { new Popup { Type = "danger", Text = "Ошибка! Перезагрузите страницу" } } });
		}

		[HttpGet]
		[Authorize(Roles = "admin, moder")]
		public IActionResult CreateSection()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> CreateSection(CreateSectionViewModel model)
		{
			if (ModelState.IsValid)
			{
				var newSection = new Section { Title = model.Title, Description = model.Description, IsHidden = model.IsHidden };
				_forumContext.Sections.Add(newSection);
				await _forumContext.SaveChangesAsync();
				await CreateReportAsync(newSection.Id, "section", "Создал(а) раздел", await _userManager.GetUserAsync(User));
				return RedirectToAction("Index", "Forum");
			}
			return View(model);
		}


		// КАТЕГОРИЯ

		[HttpGet]
		public async Task<IActionResult> Category(Guid id)
		{
			if (id != Guid.Empty)
			{
				var category = await _forumContext.Categories
					.Include(c => c.Sections)
					.ThenInclude(s => s.Themes)
					.ThenInclude(t => t.Theme.Messages)
					.ThenInclude(m => m.Author)
					.FirstOrDefaultAsync(c => c.Id == id);
				if (category != null)
				{
					if (category.IsHidden && !(User.IsInRole("moder") || User.IsInRole("admin"))) // Если категория скрыта, то у обычных пользователей нет доступа
						return RedirectToAction("Index", "Forum");

					// Убираю скрытые разделы из категории
					for (int i = 0; i < category.Sections.Count; i++)
					{
						if (category.Sections[i].IsHidden)
						{
							category.Sections.RemoveAt(i);
							i--;
						}
					}

					return View(category);
				}
			}
			return RedirectToAction("Index", "Forum");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> SaveCategory(Guid id, string title, bool isHidden, List<Guid> sectionIds)
		{
			if (ModelState.IsValid)
			{
				if (title.Trim().Length < 3 || title.Trim().Length > 100)
					return BadRequest(new { Popups = new List<Popup> { new Popup { Type = "danger", Text = "Ошибка! Название не удовлетворяет требованиям по длине" } } });

				var category = await _forumContext.Categories
					.Include(c => c.Sections)
					.Where(c => c.Id == id)
					.FirstOrDefaultAsync();

				if (category != null)
				{
					category.IsHidden = isHidden;
					category.Title = title;

					List<Section> sections = new List<Section>();
					foreach (var sectionId in sectionIds)
					{
						var section = await _forumContext.Sections
							.Where(s => s.Id == sectionId)
							.FirstOrDefaultAsync();
						if (section != null && !sections.Contains(section))
							sections.Add(section);
					}

					category.Sections = sections;
					_forumContext.Categories.Update(category);
					await _forumContext.SaveChangesAsync();
					return Ok(new { Popups = new List<Popup> { new Popup { Type = "success", Text = "Успешно! Категория обновлена" } } });
				}
				return NotFound(new { Popups = new List<Popup> { new Popup { Type = "danger", Text = "Ошибка! Категория не найдена, возможно она удалена" } } });
			}
			return BadRequest(new { Popups = new List<Popup> { new Popup { Type = "danger", Text = "Ошибка! Перезагрузите страницу" } } });
		}

		[HttpGet]
		[Authorize(Roles = "admin, moder")]
		public IActionResult CreateCategory()
		{
			return View();
		}

		[HttpPost]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> CreateCategory(CreateCategoryViewModel model)
		{
			if (ModelState.IsValid)
			{
				var newCategory = new Category { Title = model.Title, IsHidden = model.IsHidden };
				_forumContext.Categories.Add(newCategory);
				await _forumContext.SaveChangesAsync();
				await CreateReportAsync(newCategory.Id, "category", "Создал(а) категорию", await _userManager.GetUserAsync(User));
				return RedirectToAction("Index", "Forum");
			}
			return View(model);
		}


		// АДМИН-ПАНЕЛЬ и AJAX-функции

		[HttpGet]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> AdminPanel()
		{
			var users = await _userManager.Users
				.Include(u => u.Messages)
				.Include(u => u.Themes)
				.OrderByDescending(u => u.RegistrationDate)
				.Select(u => new UserAdminPanelViewModel { Id = u.Id, Email = u.Email, IsSilenced = u.IsSilenced, RegistrationDate = u.RegistrationDate, Status = u.Status, UserName = u.UserName, SilenceStopTime = u.SilenceStopTime, MessagesCount = u.Messages.Count, ThemesCount = u.Themes.Count })
				.ToListAsync();

			foreach (var user in users)
			{
				user.RoleNames = await _userManager.GetRolesAsync(await _forumContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id));
			}

			ViewBag.Users = users;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> AdminPanelRoles()
		{
			var roles = await _forumContext.Roles
				.Select(r => new RolesAdminPanelViewModel { Id = r.Id, Name = r.Name })
				.ToListAsync();
			foreach (var role in roles)
			{
				role.UsersCount = (await _userManager.GetUsersInRoleAsync(role.Name)).Count;
			}
			return Json(roles);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> AdminPanelReports()
		{
			var reports = await _forumContext.Reports
				.Include(r => r.Sender)
				.OrderByDescending(r => r.SendingTime)
				.Select(r => new ReportsAdminPanelViewModel { Id = r.Id, ObjectId = r.ObjectId, Type = r.ObjectType, ObjectName = r.ObjectName, IsChecked = r.IsChecked, InitiatorId = r.Sender.Id, InitiatorName = r.Sender.UserName, Text = r.Text, Time = r.SendingTime.ToString("dd.MM.yyyy HH:mm") })
				.ToListAsync();

			return Json(reports);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> AdminPanelCategories()
		{
			var categories = await _forumContext.Categories
				.Include(c => c.Sections)
				.Select(c => new CategoriesAdminPanelViewModel { Id = c.Id, Title = c.Title, IsHidden = c.IsHidden, Sections = c.Sections.Select(s => new CategoriesAdminPanelViewModel.SectionIdTitle { Id = s.Id, Title = s.Title }) })
				.ToListAsync();
			return Json(categories);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> AdminPanelSections()
		{
			var sections = await _forumContext.Sections
				.Include(s => s.Themes)
				.ThenInclude(ts => ts.Theme)
				.Select(s => new SectionsAdminPanelViewModel { Id = s.Id, Title = s.Title, Description = s.Description, IsHidden = s.IsHidden, ThemesCount = s.Themes.Count, Themes = s.Themes.Select(ts => new SectionsAdminPanelViewModel.ThemesIdTitle { Id = ts.Theme.Id, Title = ts.Theme.Title }) })
				.ToListAsync();
			return Json(sections);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> AdminPanelThemes()
		{
			var themes = await _forumContext.Themes
				.Include(t => t.Messages)
				.Include(t => t.Author)
				.OrderByDescending(t => t.CreatingTime)
				.Select(t => new ThemesAdminPanelViewModel { Id = t.Id, Title = t.Title, Description = t.Description, IsClosed = t.IsClosed, IsHidden = t.IsHidden, CreatingTime = t.CreatingTime.ToString("dd.MM.yyyy HH:mm"), CreatorId = t.Author.Id, CreatorName = t.Author.UserName, MessagesCount = t.Messages.Count })
				.ToListAsync();
			return Json(themes);
		}


		// Доп-функционал из Админ-панели

		[HttpPost]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> CheckReport(Guid id)
		{
			var report = await _forumContext.Reports
				.Where(r => r.Id == id)
				.FirstOrDefaultAsync();
			if (report != null)
			{
				report.IsChecked = true;
				_forumContext.Reports.Update(report);
				await _forumContext.SaveChangesAsync();
				return Ok();
			}
			return NotFound();
		}

		[HttpGet]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> ReportsRedirect(Guid id, string type)
		{
			if (type == "user")
			{
				var user = await _forumContext.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
				if (user != null)
					return RedirectToAction("AboutUser", "Forum", new { id = id });
			}
			else if (type == "role")
			{
				var role = await _forumContext.Roles.Where(r => r.Id == id).FirstOrDefaultAsync();
				if (role != null)
					return RedirectToAction("AboutRole", "Forum", new { id = id });
			}
			else if (type == "message")
			{
				var message = await _forumContext.Messages.Include(m => m.Theme).Where(x => x.Id == id).FirstOrDefaultAsync();
				if (message != null)
					return RedirectToAction("OneMessage", "Forum", new { id = id });

			}
			else if (type == "theme")
			{
				var theme = await _forumContext.Themes.Where(x => x.Id == id).FirstOrDefaultAsync();
				if (theme != null)
					return RedirectToAction("Theme", "Forum", new { id = id });
			}
			else if (type == "section")
			{
				var section = await _forumContext.Sections.Where(x => x.Id == id).FirstOrDefaultAsync();
				if (section != null)
					return RedirectToAction("Section", "Forum", new { id = id });
			}
			else if (type == "category")
			{
				var category = await _forumContext.Categories.Where(x => x.Id == id).FirstOrDefaultAsync();
				if (category != null)
					return RedirectToAction("Category", "Forum", new { id = id });
			}
			return RedirectToAction("Index", "Forum");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> EditUser(UserEditAdminPanelViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _forumContext.Users.Where(u => u.Id == model.Id).FirstOrDefaultAsync();
				if (user != null)
				{
					await CreateReportAsync(model.Id, "user", "Изменил(а) пользователя", await _userManager.GetUserAsync(User));
					user.Status = model.Status;
					_forumContext.Users.Update(user);
					await _forumContext.SaveChangesAsync();

					var allRoles = await _forumContext.Roles.Select(r => r.Name).ToListAsync();
					var removedRoles = allRoles as IEnumerable<string>;
					if (model.Roles != null)
					{
						foreach (var r in model.Roles)
						{
							var role = await _forumContext.Roles.Where(x => x.Name == r).FirstOrDefaultAsync();
							if (role != null)
								await _userManager.AddToRoleAsync(user, role.Name);
						}
						removedRoles = allRoles.Except(model.Roles);
					}
					if (removedRoles != null)
					{
						foreach (var roleName in removedRoles)
						{
							await _userManager.RemoveFromRoleAsync(user, roleName);
						}
					}

					return RedirectToAction("AdminPanel", "Forum");
				}
				else
				{
					ModelState.AddModelError("", "Запрашиваемый пользователь не найден");
				}
			}
			return View(model);
		}


		// ДОПОЛНИТЕЛЬНЫЕ ОБЩИЕ ДЕЙСТВИЯ (Однотипные с перенаправлением по типу objectType)

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Report(Guid id, string objectType, string text)
		{
			if (objectType == "message" || objectType == "theme" || objectType == "section" || objectType == "category" || objectType == "user")
			{
				var sender = await _userManager.GetUserAsync(User);
				await CreateReportAsync(id, objectType, text, sender);
				return Ok();
			}

			return BadRequest();
		}

		[HttpPost]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> Hide(Guid id, string objectType)
		{
			var initiator = await _userManager.GetUserAsync(User);
			if (objectType == "theme")
			{
				var theme = await _forumContext.Themes.Where(t => t.Id == id).FirstOrDefaultAsync();
				if (theme != null)
				{
					await CreateReportAsync(id, objectType, "Скрыл(а) тему", initiator);
					theme.IsHidden = true;
					_forumContext.Themes.Update(theme);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			else if (objectType == "section")
			{
				var section = await _forumContext.Sections.Where(t => t.Id == id).FirstOrDefaultAsync();
				if (section != null)
				{
					await CreateReportAsync(id, objectType, "Скрыл(а) раздел", initiator);
					section.IsHidden = true;
					_forumContext.Sections.Update(section);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			else if (objectType == "category")
			{
				var category = await _forumContext.Categories.Where(t => t.Id == id).FirstOrDefaultAsync();
				if (category != null)
				{
					await CreateReportAsync(id, objectType, "Скрыл(а) категорию", initiator);
					category.IsHidden = true;
					_forumContext.Categories.Update(category);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			return BadRequest();
		}

		[HttpPost]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> Unhide(Guid id, string objectType)
		{
			var initiator = await _userManager.GetUserAsync(User);
			if (objectType == "theme")
			{
				var theme = await _forumContext.Themes.Where(t => t.Id == id).FirstOrDefaultAsync();
				if (theme != null)
				{
					await CreateReportAsync(id, objectType, "Показал(а) тему", initiator);
					theme.IsHidden = false;
					_forumContext.Themes.Update(theme);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			else if (objectType == "section")
			{
				var section = await _forumContext.Sections.Where(t => t.Id == id).FirstOrDefaultAsync();
				if (section != null)
				{
					await CreateReportAsync(id, objectType, "Показал(а) раздел", initiator);
					section.IsHidden = false;
					_forumContext.Sections.Update(section);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			else if (objectType == "category")
			{
				var category = await _forumContext.Categories.Where(t => t.Id == id).FirstOrDefaultAsync();
				if (category != null)
				{
					await CreateReportAsync(id, objectType, "Показал(а) категорию", initiator);
					category.IsHidden = false;
					_forumContext.Categories.Update(category);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			return BadRequest();
		}

		[HttpPost]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> Delete(Guid id, string objectType)
		{
			var initiator = await _userManager.GetUserAsync(User);
			if (objectType == "theme")
			{
				var theme = await _forumContext.Themes.Include(t => t.Messages).Where(t => t.Id == id).FirstOrDefaultAsync();
				if (theme != null)
				{
					await CreateReportAsync(id, objectType, $"Удалил(а) тему с {theme.Messages.Count} сообщениями", initiator);
					_forumContext.Themes.Remove(theme);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			else if (objectType == "section")
			{
				var section = await _forumContext.Sections.Where(t => t.Id == id).FirstOrDefaultAsync();
				if (section != null)
				{
					await CreateReportAsync(id, objectType, "Удалил(а) раздел", initiator);
					_forumContext.Sections.Remove(section);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			else if (objectType == "category")
			{
				var category = await _forumContext.Categories.Where(t => t.Id == id).FirstOrDefaultAsync();
				if (category != null)
				{
					await CreateReportAsync(id, objectType, "Удалил(а) категорию", initiator);
					_forumContext.Categories.Remove(category);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			else if (objectType == "role" && User.IsInRole("admin")) // Роли удалять может только admin
			{
				var role = await _forumContext.Roles.Where(r => r.Id == id).FirstOrDefaultAsync();
				if (role != null)
				{
					if (role.Name == "admin" || role.Name == "moder") // Нельзя удалить эти роли
						return Forbid();
					await CreateReportAsync(id, objectType, "Удалил(а) роль", initiator);
					await _roleManager.DeleteAsync(role);
					return Ok();
				}
				return NotFound();
			}
			else if (objectType == "report") // Репорты удалять может только admin
			{
				var report = await _forumContext.Reports.Where(r => r.Id == id).FirstOrDefaultAsync();
				if (report != null)
				{
					_forumContext.Reports.Remove(report);
					await _forumContext.SaveChangesAsync();
					return Ok();
				}
				return NotFound();
			}
			return BadRequest();
		}

		[HttpGet]
		[Authorize(Roles = "admin, moder")]
		public async Task<IActionResult> Edit(Guid id, string objectType)
		{
			if (objectType == "theme")
			{
				var themeModel = await _forumContext.Themes
					.Include(t => t.Author)
					.Where(t => t.Id == id)
					.Select(t => new EditThemeViewModel { Id = t.Id, Title = t.Title, Description = t.Description, IsClosed = t.IsClosed, IsHidden = t.IsHidden, AuthorId = t.Author.Id, AuthorUserName = t.Author.UserName })
					.FirstOrDefaultAsync();
				if (themeModel != null)
				{
					return View("EditTheme", themeModel);
				}
			}
			else if (objectType == "message")
			{
				var messageModel = await _forumContext.Messages
					.Include(m => m.Author)
					.Include(m => m.Theme)
					.Where(m => m.Id == id)
					.Select(m => new EditMessageViewModel { Id = m.Id, Text = m.Text, CreatingTime = m.CreatingTime, ThemeTitle = m.Theme.Title, ThemeId = m.Theme.Id, AuthorId = m.Author.Id, AuthorUserName = m.Author.UserName })
					.FirstOrDefaultAsync();
				if (messageModel != null)
				{
					return View("EditMessage", messageModel);
				}
			}
			else if (objectType == "section")
			{
				var section = await _forumContext.Sections
					.Where(t => t.Id == id)
					.Select(s => new EditSectionViewModel { Id = id, Title = s.Title, Description = s.Description, IsHidden = s.IsHidden })
					.FirstOrDefaultAsync();
				if (section != null)
				{
					ViewBag.Themes = await _forumContext.Themes
						.Include(t => t.Sections)
						.ThenInclude(ts => ts.Section)
						.OrderByDescending(t => t.Sections.Any(ts => ts.Section.Id == id))
						.Select(t => new ThemeEditViewModel { Id = t.Id, Title = t.Title, Description = t.Description, IsHidden = t.IsHidden, Sections = t.Sections.Select(s => new ThemeEditViewModel.SectionEditViewModel { SectionId = s.Section.Id }).ToList() })
						.ToListAsync();
					ViewBag.Categories = await _forumContext.Categories
						.Include(c => c.Sections)
						.ThenInclude(s => s.Themes)
						.OrderByDescending(c => c.Sections.Any(s => s.Id == id))
						.Select(c => new CategoryEditViewModel { Id = c.Id, Title = c.Title, IsHidden = c.IsHidden, Sections = c.Sections.Select(s => new CategoryEditViewModel.SectionEditViewModel { SectionId = s.Id }).ToList() })
						.ToListAsync();
					return View("EditSection", section);
				}
			}
			else if (objectType == "category")
			{
				var categoryViewModel = await _forumContext.Categories
					.Where(c => c.Id == id)
					.Select(c => new EditCategoryViewModel { Id = c.Id, Title = c.Title, IsHidden = c.IsHidden })
					.FirstOrDefaultAsync();
				if (categoryViewModel != null)
				{
					categoryViewModel.Sections = await _forumContext.Sections
						.AsNoTracking()
						.Include(s => s.Category)
						.OrderByDescending(s => s.Category.Id == categoryViewModel.Id)
						.Select(s => new EditCategoryViewModel.SectionViewModel { Id = s.Id , Title = s.Title, Description = s.Description, IsHidden = s.IsHidden, CategoryId = s.Category.Id})
						.ToListAsync();
					return View("EditCategory", categoryViewModel);
				}
			}
			else if (objectType == "role" && User.IsInRole("admin")) // Роли могут редактировать только администраторы
			{
				var role = await _forumContext.Roles
					.Where(r => r.Id == id)
					.Select(r => new RoleEditAdminPanelViewModel { Id = r.Id, Name = r.Name })
					.FirstOrDefaultAsync();
				if (role != null)
				{
					if (role.Name == "admin" || role.Name == "moder")
						return RedirectToAction("AdminPanel", "Forum");
					return View("EditRole", role);
				}
			}
			else if (objectType == "user" && User.IsInRole("admin")) // Пользователей могут редактировать только администраторы
			{
				var user = await _forumContext.Users
					.Where(u => u.Id == id)
					.Select(u => new UserEditAdminPanelViewModel { Id = u.Id, Login = u.UserName, Status = u.Status })
					.FirstOrDefaultAsync();
				if (user != null)
				{
					user.RolesHelper = await _forumContext.Roles.Select(r => new UserEditAdminPanelViewModel.RoleNameIsAdded { Name = r.Name }).ToListAsync();
					user.Roles = await _forumContext.Roles.Select(r => r.Name).ToListAsync();
					foreach (var role in user.RolesHelper)
					{
						role.IsAdded = await _userManager.IsInRoleAsync(await _forumContext.Users.FirstAsync(u => u.Id == user.Id), role.Name);
					}
					return View("EditUser", user);
				}
			}
			return RedirectToAction("Index", "Forum");
		}


		// ПОЛЬЗОВАТЕЛЬ

		[HttpGet]
		public async Task<IActionResult> AboutUser(Guid id)
		{
			if (id == Guid.Empty)
			{
				if (User.Identity.IsAuthenticated)
				{
					var user = await _userManager.GetUserAsync(User);
					var countOfMessages = (await _forumContext.Users.Include(u => u.Messages).FirstAsync(u => u.Id == user.Id)).Messages.Count;
					var countOfThemes = (await _forumContext.Users.Include(u => u.Themes).FirstAsync(u => u.Id == user.Id)).Themes.Count;
					ViewBag.CountOfMessages = countOfMessages;
					ViewBag.CountOfThemes = countOfThemes;
					return View(user);
				}
				else
					return RedirectToAction("Index", "Forum");
			}
			else
			{
				var user = await _forumContext.Users
					.Include(u => u.Messages)
					.Include(u => u.Themes)
					.FirstOrDefaultAsync(u => u.Id == id);
				if (user != null)
				{
					var countOfMessages = user.Messages.Count;
					var countOfThemes = user.Themes.Count;
					ViewBag.CountOfMessages = countOfMessages;
					ViewBag.CountOfThemes = countOfThemes;
					return View(user);
				}
				else
					return RedirectToAction("Index", "Forum");
			}
		}

		[HttpGet]
		public async Task<IActionResult> EditProfile()
		{
			if (User.Identity.IsAuthenticated)
			{
				var user = await _userManager.GetUserAsync(User);

				var viewModel = new UserEditProfileViewModel { Login = user.UserName, Status = user.Status };
				return View(viewModel);
			}
			return RedirectToAction("Index", "Forum");
		}

		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditProfile(UserEditProfileViewModel model)
		{
			if (model.Login.Trim().Length < 4 || model.Login.Trim().Length > 16)
			{
				ModelState.AddModelError("", "Допустимая длина логина - от 4 до 16 символов");
				return View(model);
			}

			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByNameAsync(model.Login);
				if (user == null || user.Id == (await _userManager.GetUserAsync(User)).Id)
				{
					var realUser = await _userManager.GetUserAsync(User);
					realUser.UserName = model.Login;
					realUser.NormalizedUserName = model.Login.ToUpper();
					realUser.Status = model.Status;
					var result = await _userManager.UpdateAsync(realUser);

					if (result.Succeeded)
					{
						await _signInManager.SignOutAsync();
						await _signInManager.SignInAsync(realUser, true);
						return RedirectToAction("AboutUser", "Forum");
					}
					else
					{
						ModelState.AddModelError("", "Не используйте в логине спец. символы");
					}
				}
				else
				{
					if (user.Id != (await _userManager.GetUserAsync(User)).Id)
						ModelState.AddModelError("", "Этот логин занят. Попробуйте другой");
				}
			}
			return View(model);
		}


		// РОЛЬ

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> EditRole(RoleEditAdminPanelViewModel model)
		{
			if (ModelState.IsValid)
			{
				var role = await _forumContext.Roles.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
				var role2 = await _forumContext.Roles.Where(x => x.Name == model.Name).FirstOrDefaultAsync();

				if (role != null)
				{
					if (role2 != null)
					{
						ModelState.AddModelError("", "Роль с таким именем уже существует");
						return View(model);
					}

					await CreateReportAsync(model.Id, "role", "Изменил(а) роль", await _userManager.GetUserAsync(User));
					role.Name = model.Name;
					role.NormalizedName = model.Name.ToUpper();
					await _roleManager.UpdateAsync(role);
					return RedirectToAction("AdminPanel", "Forum");
				}
				else
				{
					ModelState.AddModelError("", "Роль не найдена");
				}
			}
			return View(model);
		}

		[HttpGet]
		[Authorize(Roles = "admin")]
		public IActionResult CreateRole()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> CreateRole(CreateRoleAdminPanelViewModel model)
		{
			if (ModelState.IsValid)
			{
				var role = await _forumContext.Roles.Where(r => r.Name == model.Name).FirstOrDefaultAsync();
				if (role == null)
				{
					await _roleManager.CreateAsync(new Role { Name = model.Name, NormalizedName = model.Name.ToUpper() });
					var newRole = await _roleManager.FindByNameAsync(model.Name);
					await CreateReportAsync(newRole.Id, "role", "Создал(а) роль", await _userManager.GetUserAsync(User));
					return RedirectToAction("AdminPanel", "Forum");
				}
				ModelState.AddModelError("", "Роль с таким именем уже существует");
			}
			return View(model);
		}
	}
}