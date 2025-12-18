using System;
using Microsoft.Extensions.Hosting;
using Autofac.Extensions.DependencyInjection;
using FastPaymentEventsSender.ApiClients;
using FastPaymentEventsSender.Notifications;
using FastPaymentEventsSender.Options;
using FastPaymentEventsSender.Services;
using FastPaymentsAPI.Library.Settings;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using RabbitMQ.MailSending;
using Vodovoz;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Settings.FastPayments;

namespace FastPaymentEventsSender
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureLogging((ctx, builder) => {
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(QS.Attachments.Domain.Attachment).Assembly,
							typeof(EmployeeWithLoginMap).Assembly,
							typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
						)
						.AddDatabaseConnection()
						.AddCore()
						.AddTrackedUoW()
						.AddBusiness(hostContext.Configuration)
						.Configure<SenderOptions>(hostContext.Configuration.GetSection(SenderOptions.Path))
						.Configure<DriverApiOptions>(hostContext.Configuration.GetSection(DriverApiOptions.Path))
						.AddInfrastructure()
						.AddScoped<ISiteSettings, SiteSettings>()
						.AddScoped<IFastPaymentStatusUpdatedNotifier, FastPaymentStatusUpdatedNotifier>()
						.AddHostedService<FastPaymentEventsProcessor>()
						.AddMessageTransportSettings()
						
						.AddMassTransit(busConf =>
						{
							busConf.ConfigureRabbitMq((rabbitMq, context) =>
								{
									rabbitMq.AddSendEmailMessageTopology(context);
								});
						});
						
						services.AddHttpClient<IWebSiteClient, WebSiteClient>((sp, client) =>
						{
							var siteSettings = sp.GetRequiredService<ISiteSettings>();
							client.BaseAddress = new Uri(siteSettings.BaseUrl);
							client.DefaultRequestHeaders.Add("Accept", "application/json");
						});
						
						services.AddHttpClient<IMobileAppClient, MobileAppClient>(client =>
						{
							client.DefaultRequestHeaders.Add("Accept", "application/json");
						});
						
						services.AddHttpClient<IAiBotClient, AiBotClient>(client =>
						{
							client.DefaultRequestHeaders.Add("Accept", "application/json");
						});

						services.AddHttpClient<IDriverAPIService, DriverAPIService>((sp, client) =>
						{
							var baseAddress = sp.GetRequiredService<IOptionsSnapshot<DriverApiOptions>>().Value.BaseUrl;
							client.BaseAddress = new Uri(baseAddress);
							client.DefaultRequestHeaders.Add("Accept", "application/json");
						});

					services.AddStaticScopeForEntity();
					services.AddStaticHistoryTracker();
				});
	}
}
