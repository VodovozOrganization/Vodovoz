using System;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Utilities.Numeric;
using RabbitMQ.EmailSending.Contracts;
using RabbitMQ.MailSending;
using SecureCodeSenderApi.Services;
using SecureCodeSenderApi.Services.Validators;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Presentation.WebApi;

namespace SecureCodeSenderApi
{
	/// <summary>
	/// Расширения для апи
	/// </summary>
	public static class SecureCodeSenderExtensions
	{
		/// <summary>
		/// Добавление всех необходимых зависимостей и регистраций
		/// </summary>
		/// <param name="services">Контейнер</param>
		/// <param name="configuration">Конфигурация</param>
		/// <returns></returns>
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
					busConf.ConfigureRabbitMq((rabbitMq, context) => rabbitMq.AddSendAuthorizationCodesByEmailTopology(context));
				})
				.AddVersioning();

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
			services.AddStaticHistoryTracker();

			return services;
		}
		
		/// <summary>
		/// Добавление зависимостей
		/// </summary>
		/// <param name="services">Контейнер</param>
		/// <returns></returns>
		private static IServiceCollection AddDependencyGroup(this IServiceCollection services)
		{
			services
				.AddScoped(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot())
				.AddScoped<ISecureCodeHandler, SecureCodeHandler>()
				.AddScoped<IEmailSecureCodeSender, EmailSecureCodeSender>()
				.AddScoped<ISecureCodeServiceValidator, SecureCodeServiceValidator>()
				.AddScoped<IIpValidator, IpValidator>()
				.AddScoped<IEmailMethodValidator, EmailMethodValidator>()
				.AddScoped<IUserPhoneValidator, UserPhoneValidator>()
				.AddScoped<ISecureCodeValidator, SecureCodeValidator>()
				.AddScoped(s => new PhoneValidator(PhoneFormat.RussiaOnlyShort))
				;
			
			return services;
		}
	}
}
