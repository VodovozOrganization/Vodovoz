using MySqlConnector;
using QS.DomainModel.Tracking;
using System;

namespace Vodovoz.Trackers
{
	public class OrderTrackerFor1cFactory : ISingleUowEventsListnerFactory
	{
		private readonly MySqlConnectionStringBuilder _connectionStringBuilder;

		public OrderTrackerFor1cFactory(MySqlConnectionStringBuilder connectionStringBuilder)
		{
			_connectionStringBuilder = connectionStringBuilder ?? throw new ArgumentNullException(nameof(connectionStringBuilder));
		}

		public ISingleUowEventListener CreateListnerForNewUow(IUnitOfWorkTracked uow)
		{
			return new OrderTrackerFor1c(_connectionStringBuilder);
		}
	}
}
