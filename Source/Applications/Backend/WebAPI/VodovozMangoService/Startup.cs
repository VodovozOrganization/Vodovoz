using MangoService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using VodovozMangoService.HostedServices;
using VodovozMangoService.Services;

namespace VodovozMangoService
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			var connectionStringBuilder = new MySqlConnectionStringBuilder();
			connectionStringBuilder.Server = Configuration["Mysql:mysql_server_host_name"];
			connectionStringBuilder.Port = uint.Parse(Configuration["Mysql:mysql_server_port"]);
			connectionStringBuilder.Database = Configuration["Mysql:mysql_database"];;
			connectionStringBuilder.UserID = Configuration["Mysql:mysql_user"];;
			connectionStringBuilder.Password = Configuration["Mysql:mysql_password"];;
			connectionStringBuilder.SslMode = MySqlSslMode.None;
			connectionStringBuilder.DefaultCommandTimeout = 5;
			
			services.AddSingleton(x =>
				new MySqlConnection(connectionStringBuilder.ConnectionString));
	 
			services.AddSingleton(x =>
				new MangoController(Configuration["Mango:vpbx_api_key"], Configuration["Mango:vpbx_api_salt"]));
			
			services.AddSingleton<CallsHostedService>();
			services.AddHostedService<CallsHostedService>(provider => provider.GetService<CallsHostedService>());
			
			services.AddSingleton<PhonebookHostedService>();
			services.AddHostedService<PhonebookHostedService>(provider => provider.GetService<PhonebookHostedService>());
			
			services.AddSingleton<NotificationHostedService>();
			services.AddHostedService<NotificationHostedService>(provider => provider.GetService<NotificationHostedService>());

			services.AddSingleton<ICallerService, CallerService>();

			services.AddControllers();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// app.UseHttpsRedirection();
			#if DEBUG
			app.UseMiddleware<PerformanceMiddleware>();
			#endif
			app.UseRouting();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}
