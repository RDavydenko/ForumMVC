using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyForum.Models;

namespace MyForum.Middlewares
{
	public class CheckingSilencedUsersMiddleware
	{
		private readonly RequestDelegate _next;

		public CheckingSilencedUsersMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{

			async void Check()
			{
				while (true)
				{					
					using (var db = context.RequestServices.GetService(typeof(ForumContext)) as ForumContext)
					{
						var silencedUsers = db.Users.Where(u => u.IsSilenced).ToList();
						foreach (var user in silencedUsers)
						{
							if (user.SilenceStopTime <= DateTime.Now)
							{
								user.IsSilenced = false;
								user.SilenceStartTime = null;
								user.SilenceStopTime = null;
								db.Users.Update(user);
							}
						}
						await db.SaveChangesAsync();
					}
					await Task.Delay(30 * 1000);
				}
			}			

			Thread thread = new Thread(new ThreadStart(Check));
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.Lowest;
			//thread.Start();

			await _next.Invoke(context);
		}
	}
}
