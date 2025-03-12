using Edo.Transport;
using Edo.Withdrawal.Consumers;
using Edo.Withdrawal.Consumers.Definitions;
using Edo.Withdrawal.Options;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Edo.Withdrawal
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoWithdrawal(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumer<WithdrawalTaskCreatedConsumer, WithdrawalTaskCreatedConsumerDefinition>();
			});

			services
				.AddScoped<WithdrawalTaskCreatedHandler>()
				.ConfigureBusinessOptions(configuration);

			return services;
		}

		public static IServiceCollection ConfigureBusinessOptions(this IServiceCollection services, IConfiguration configuration) => services
			.Configure<TrueMarkSettings>(trueMarkSettings =>
				configuration.GetSection(nameof(TrueMarkSettings)).Bind(trueMarkSettings));
	}
}
