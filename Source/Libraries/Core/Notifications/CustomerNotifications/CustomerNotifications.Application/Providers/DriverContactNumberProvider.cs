using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Mango;

namespace CustomerNotifications.Application.Providers
{
	/// <inheritdoc/>
	public class DriverContactNumberProvider : IDriverContactNumberProvider
	{
		private readonly ILogger<DriverContactNumberProvider> _logger;
		private readonly IOrderRepository _orderRepository;
		private readonly IMangoSettings _mangoSettings;

		public DriverContactNumberProvider(
			ILogger<DriverContactNumberProvider> logger,
			IOrderRepository orderRepository,
			IMangoSettings mangoSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
		}

		/// <inheritdoc/>
		public async Task<string> GetDriverContactNumberAsync(
			IUnitOfWork unitOfWork,
			int orderId,
			CancellationToken cancellationToken = default)
		{
			if(unitOfWork is null)
			{
				throw new ArgumentNullException(nameof(unitOfWork));
			}

			var driversCallsLineNumber = _mangoSettings.DriversCallsLineNumber;

			var driverMangoExtensionNumber =
				await _orderRepository.GetDriversMangoExtensionNumberByOrderId(unitOfWork, orderId, cancellationToken);

			if(driverMangoExtensionNumber?.ExtensionNumber is null)
			{
				_logger.LogWarning(
					"Не найден активный добавочный номер Манго водителя, доставляющего заказ {OrderId}, "
					+ "номер для связи с водителем будет содержать только номер линии Манго",
					orderId);

				return driversCallsLineNumber;
			}

			return $"{driversCallsLineNumber},,{driverMangoExtensionNumber.ExtensionNumber}";
		}
	}
}
