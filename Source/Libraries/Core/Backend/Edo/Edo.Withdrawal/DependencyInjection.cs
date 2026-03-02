using Edo.Transport;
using Edo.Withdrawal.Consumers;
using Edo.Withdrawal.Consumers.Definitions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrueMarkApi.Client;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Infrastructure.Persistance.Edo;
using Vodovoz.Settings.Edo;
using VodovozBusiness.EntityRepositories.Edo;

namespace Edo.Withdrawal
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoWithdrawalService(this IServiceCollection services)
		{
			services
				.AddScoped<IEdoDocflowRepository, EdoDocflowRepository>()
				.AddScoped<WithdrawalTaskCreatedHandler>()
				.AddTrueMarkApiClient();

			services.AddScoped<IEdoRepository, Vodovoz.Core.Data.NHibernate.Repositories.Edo.EdoRepository>();

			return services;
		}

		public static IServiceCollection AddEdoWithdrawal(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumer<WithdrawalTaskCreatedConsumer, WithdrawalTaskCreatedConsumerDefinition>();
			});

			AddEdoWithdrawalService(services);

			return services;
		}
	}
}
