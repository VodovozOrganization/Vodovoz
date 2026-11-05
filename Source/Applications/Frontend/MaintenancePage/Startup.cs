using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace MaintenancePage
{
	public class Startup
	{
		private const string _pageFileName = "index.html";

		public void ConfigureServices(IServiceCollection services)
		{
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			// Ресурсы страницы из wwwroot (logo.jpg и т.п.)
			app.UseStaticFiles();

			var page = env.WebRootFileProvider.GetFileInfo(_pageFileName);

			// На любой другой запрос (любой путь и метод) отдаём страницу обслуживания
			app.Run(async context =>
			{
				context.Response.StatusCode = StatusCodes.Status200OK;
				context.Response.ContentType = "text/html; charset=utf-8";
				context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";

				if(HttpMethods.IsHead(context.Request.Method))
				{
					context.Response.ContentLength = page.Length;
					return;
				}

				await context.Response.SendFileAsync(page);
			});
		}
	}
}
