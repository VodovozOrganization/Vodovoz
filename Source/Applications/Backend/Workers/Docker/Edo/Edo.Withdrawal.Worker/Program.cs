﻿using Autofac.Extensions.DependencyInjection;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using QS.Project.Core;
using System;
using System.Text;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Persistance;

namespace Edo.Withdrawal.Worker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((ctx, builder) =>
				{
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(Vodovoz.Core.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly
						)
						.AddDatabaseConnection()
						.AddCoreDataRepositories()
						.AddCore()
						.AddTrackedUoW()
						.AddMessageTransportSettings()
						.AddEdoWithdrawal(hostContext.Configuration)
						.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

					services
						.AddOpenTelemetry()
						.ConfigureResource(resource => resource.AddService("edo.withdrawal.worker"))
						.WithTracing(tracing =>
						{
							tracing
								.AddHttpClientInstrumentation();

							tracing.AddOtlpExporter();
						});

					services.AddHostedService<InitDbConnectionOnHostStartedService>();
				});
	}
}
