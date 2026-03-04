using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using QS.DomainModel.Tracking;
using QS.Project.Core;


namespace Vodovoz.Trackers
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddOrderTrackerFor1c(this IServiceCollection services)
		{
			services.AddSingleton<OnDatabaseInitialization>((provider) =>
			{
				var connectionStringBuilder = provider.GetRequiredService<MySqlConnectionStringBuilder>();
				SingleUowEventsTracker.RegisterSingleUowListnerFactory(new OrderTrackerFor1cFactory(connectionStringBuilder));
				return new OnDatabaseInitialization();
			});

			return services;
		}
	}
}
