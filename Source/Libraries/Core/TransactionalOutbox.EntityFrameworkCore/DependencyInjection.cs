using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySqlConnector;
using System;
using System.Linq;

namespace TransactionalOutbox.EntityFrameworkCore
{
	public static class DependencyInjection
	{
		/// <summary>
		/// EF Core: контекст таблиц outbox шины <paramref name="busName"/> (провайдер Pomelo MySQL)
		/// для штатной службы отправки MassTransit.
		/// Вызывается из <see cref="OutboxBusRegistrationExtensions.AddVodovozOutbox"/>.
		/// После перехода на один vhost RabbitMQ проверка «один сервис — одна шина отправки» и имя шины не понадобятся
		/// </summary>
		internal static IServiceCollection AddOutboxDbContext(this IServiceCollection services, string busName)
		{
			if(string.IsNullOrWhiteSpace(busName))
			{
				throw new ArgumentException("Не указано имя шины", nameof(busName));
			}

			var registered = services
				.Where(x => x.ServiceType == typeof(OutboxDeliveryBusName))
				.Select(x => ((OutboxDeliveryBusName)x.ImplementationInstance).Name)
				.FirstOrDefault();

			if(registered != null)
			{
				if(registered == busName)
				{
					return services;
				}

				throw new InvalidOperationException(
					$"Сервис уже отправляет сообщения из таблиц outbox шины '{registered}' ({registered}_outbox_*). "
					+ $"Отправлять ещё и сообщения шины '{busName}' нельзя: служба отправки в сервисе одна. "
					+ $"Сообщения в шину '{busName}' можно только записывать (AddExplicitUnitOfWorkOutbox)");
			}

			services.AddSingleton(new OutboxDeliveryBusName(busName));
			services.TryAddSingleton<OutboxDatabaseServerVersion>();

			services.AddDbContext<OutboxDbContext>((serviceProvider, options) => options.UseMySql(
				serviceProvider.GetRequiredService<MySqlConnectionStringBuilder>().ConnectionString,
				serviceProvider.GetRequiredService<OutboxDatabaseServerVersion>().ServerVersion));

			return services;
		}
	}
}
