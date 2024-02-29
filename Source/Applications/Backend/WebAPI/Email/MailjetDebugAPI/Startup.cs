using MailjetDebugAPI.Endpoints;
using MailjetDebugAPI.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System.Net.Http;

namespace MailjetDebugAPI
{
	public class Startup
	{
		private const string _eventCallbackSettingsSection = "EventsReciever";

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				});

			services.AddHttpClient();

			services.AddTransient<EventsRecieverEndpoint>(sp =>
			{
				var configuration = sp.GetRequiredService<IConfiguration>();
				var recieverSection = configuration.GetSection(_eventCallbackSettingsSection);
				var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
				var apiHelper = new ApiClientProvider.ApiClientProvider(recieverSection, httpClient);
				return new EventsRecieverEndpoint(recieverSection, apiHelper);
			});

			services.AddRazorPages();
			services.AddServerSideBlazor();
			services.AddControllers();

			services.AddSignalR(hubOptions =>
			{
				hubOptions.MaximumReceiveMessageSize = long.MaxValue;
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapBlazorHub();
				endpoints.MapControllers();
				endpoints.MapFallbackToPage("/_Host");
				endpoints.MapHub<EmailsHub>(EmailsHub.HubUrl);
			});
		}
	}
}
