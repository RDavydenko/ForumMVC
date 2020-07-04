using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyForum.Models
{
	public class ForumContext : IdentityDbContext<User, Role, Guid>
	{
		// Category (категория) -> Sections (раздел) -> Themes (темы) -> Messages (сообщения)
		// ____________1 ко многим____________многие ко многим______1 ко многим____________
		public DbSet<Message> Messages { get; set; }
		public DbSet<Theme> Themes { get; set; }
		public DbSet<ThemeSection> ThemeSections { get; set; } // Вспомогательная таблица для связи МНОГИЕ ко МНОГИМ
		public DbSet<Section> Sections { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Report> Reports { get; set; }

		public ForumContext(DbContextOptions<ForumContext> options)
			: base(options)
		{			
			Database.EnsureCreated();
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<Message>(m =>
			{
				m.HasOne(m => m.Theme)
				.WithMany(t => t.Messages)
				.OnDelete(DeleteBehavior.Cascade); // При удалении темы - удаляются все сообщения

				m.HasOne(m => m.Author)
				.WithMany()
				.OnDelete(DeleteBehavior.SetNull); // При удалении автора - автор сообщения становится NULL

				m.Property("CreatingTime").HasDefaultValueSql("GETDATE()");
			});

			builder.Entity<Theme>(t =>
			{
				t.HasMany(t => t.Messages)
				.WithOne(m => m.Theme);

				t.HasOne(t => t.Author)
				.WithMany()
				.OnDelete(DeleteBehavior.SetNull); // При удалении автора - автор темы становится NULL

				t.Property("CreatingTime").HasDefaultValueSql("GETDATE()");
			});
			
			builder.Entity<User>(u =>
			{
				u.HasMany(u => u.Messages)
				.WithOne(m => m.Author)
				.OnDelete(DeleteBehavior.NoAction); // При удалении сообщения - с пользователем ничего не происходит

				u.HasMany(u => u.Themes)
				.WithOne(t => t.Author)
				.OnDelete(DeleteBehavior.NoAction); // При удалении темы - с пользователем ничего не происходит

				u.Property("RegistrationDate").HasDefaultValueSql("GETDATE()");
			});			

			// Связь МНОГИЕ ко МНОГИМ через вспомогательный класс ThemeSection (таблицу ThemeSections)
			// между Theme и Section
			builder.Entity<ThemeSection>(ts =>
			{
				ts.HasOne(ts => ts.Theme)
				.WithMany(ts => ts.Sections)
				.OnDelete(DeleteBehavior.Cascade); // При удалении темы - удаляется вспомогательная связь в таблице ThemeSections

				ts.HasOne(ts => ts.Section)
				.WithMany(ts => ts.Themes)
				.OnDelete(DeleteBehavior.Cascade);  // При удалении секции (раздела) - удаляется вспомогательная связь в таблице ThemeSections
			});

			builder.Entity<Category>()
				.HasMany(c => c.Sections)
				.WithOne(s => s.Category)
				.OnDelete(DeleteBehavior.SetNull); // При удалении раздела - разделId у категории становится NULL

			builder.Entity<Report>(r => {				
				r.HasOne(r => r.Sender)
				.WithMany()
				.OnDelete(DeleteBehavior.SetNull); // При удалении отправителя жалобы - жалоба не удаляется, отправитель становится NULL

				r.Property("SendingTime").HasDefaultValueSql("GETDATE()");
			});

			base.OnModelCreating(builder);
		}
	}
}
