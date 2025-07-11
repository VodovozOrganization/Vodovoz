using System;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using QS.Project.Core;
using RabbitMQ.EmailSending.Contracts;
using RabbitMQ.MailSending;
using SecureCodeSenderApi.Services;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;

namespace SecureCodeSenderApi
{
	public static class SecureCodeSenderExtensions
	{
		public static IServiceCollection AddSecureCodeSenderApi(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()
				.AddInfrastructure()
				.AddDependencyGroup()
				.AddRabbitConfig(configuration)
				.AddMessageTransportSettings()
				.AddMassTransit(busConf =>
				{
					busConf.AddRequestClient<SentEmailResponse>(
						new Uri($"exchange:{configuration.GetValue<string>("RabbitOptions:AuthorizationCodesExchange")}"));
					busConf.ConfigureRabbitMq();
				});
			
			return services;
		}
		
		private static IServiceCollection AddDependencyGroup(this IServiceCollection services)
		{
			services
				.AddScoped(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot())
				.AddScoped<ISecureCodeHandler, SecureCodeHandler>()
				.AddScoped<IEmailSecureCodeSender, EmailSecureCodeSender>()
				;
			
			return services;
		}
	}
}
