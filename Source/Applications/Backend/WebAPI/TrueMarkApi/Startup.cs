﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Repositories;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using TrueMarkApi.Services;
using TrueMarkApi.Services.Authorization;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace TrueMarkApi
{
	public class Startup
	{
		private ILogger<Startup> _logger;
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			var apiSection = Configuration.GetSection("Api");

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "TrueMarkApi", Version = "v1" });
			});

			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
				});

			_logger = new Logger<Startup>(LoggerFactory.Create(logging =>
				logging.AddNLogWeb(NLogBuilder.ConfigureNLog("NLog.config").Configuration)));

			services.AddControllers();

			services.AddHostedService<DocumentService>();
			services.AddSingleton<IParametersProvider, ParametersProvider>();
			services.AddSingleton<IAuthorizationService, AuthorizationService>();
			services.AddSingleton<IOrderRepository, OrderRepository>();
			services.AddSingleton<IOrganizationRepository, OrganizationRepository>();
			services.AddSingleton<IUnitOfWorkFactory, DefaultUnitOfWorkFactory>();
			services.AddSingleton<ISessionProvider, DefaultSessionProvider>();
			services.AddSingleton<IEdoSettings, EdoSettings>();
			services.AddHttpClient();

			// Авторизация
			services.AddAuthorization();
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = false,
						ValidateAudience = false,
						ValidateLifetime = false,
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiSection.GetValue<string>("SecurityKey")))
					};
				});

			// Конфигурация Nhibernate
			try
			{
				CreateBaseConfig();
			}
			catch(Exception e)
			{
				_logger.LogCritical(e, e.Message);
				throw;
			}
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrueMarkApi v1"));
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private void CreateBaseConfig()
		{
			var conStrBuilder = new MySqlConnectionStringBuilder();

			var domainDbConfig = Configuration.GetSection("DomainDB");

			conStrBuilder.Server = domainDbConfig.GetValue<string>("Server");
			conStrBuilder.Port = domainDbConfig.GetValue<uint>("Port");
			conStrBuilder.Database = domainDbConfig.GetValue<string>("Database");
			conStrBuilder.UserID = domainDbConfig.GetValue<string>("UserID");
			conStrBuilder.Password = domainDbConfig.GetValue<string>("Password");
			conStrBuilder.SslMode = MySqlSslMode.None;

			var connectionString = conStrBuilder.GetConnectionString(true);

			var dbConfig = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString);

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				dbConfig,
				new[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.Organizations.OrganizationMap)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment))
				}
			);

			string userLogin = domainDbConfig.GetValue<string>("UserID");
			int serviceUserId = 0;

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Получение пользователя"))
			{
				serviceUserId = unitOfWork.Session.Query<Vodovoz.Domain.Employees.User>()
					.Where(u => u.Login == userLogin)
					.Select(u => u.Id)
					.FirstOrDefault();
			}

			UserRepository.GetCurrentUserId = () => serviceUserId;
			HistoryMain.Enable();
		}
	}
}
