using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Mango;

namespace VodovozBusiness.Services.Logistics
{
	/// <inheritdoc/>
	public class DriverContactNumberService : IDriverContactNumberService
	{
		private readonly ILogger<DriverContactNumberService> _logger;
		private readonly IOrderRepository _orderRepository;
		private readonly IMangoSettings _mangoSettings;

		public DriverContactNumberService(
			ILogger<DriverContactNumberService> logger,
			IOrderRepository orderRepository,
			IMangoSettings mangoSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
		}

		/// <inheritdoc/>
		public async Task<string> GetDriverContactNumberForCustomersApiAsync(
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

			if(!_mangoSettings.DriverMangoEmployeeRegistrationEnabled
				|| driverMangoExtensionNumber?.ExtensionNumber is null)
			{
				_logger.LogWarning(
					"Сервис регистрации карточек сотрудников Манго для водителей отключен, "
					+ "либо не найден активный добавочный номер Манго водителя, доставляющего заказ {OrderId}, "
					+ "номер для связи с водителем будет содержать только номер линии Манго",
					orderId);

				return driversCallsLineNumber;
			}

			return $"{driversCallsLineNumber},,{driverMangoExtensionNumber.ExtensionNumber}";
		}

		/// <inheritdoc/>
		public async Task<string> GetDriverContactNumberForSmsNotificationAsync(
			IUnitOfWork unitOfWork,
			int orderId,
			CancellationToken cancellationToken = default)
		{
			if(unitOfWork is null)
			{
				throw new ArgumentNullException(nameof(unitOfWork));
			}

			var formatter = new PhoneFormatter(PhoneFormat.DigitsTen);
			var driversCallsLineNumber = "8" + formatter.FormatString(_mangoSettings.DriversCallsLineNumber);

			var driverMangoExtensionNumber =
				await _orderRepository.GetDriversMangoExtensionNumberByOrderId(unitOfWork, orderId, cancellationToken);

			if(!_mangoSettings.DriverMangoEmployeeRegistrationEnabled
				|| driverMangoExtensionNumber?.ExtensionNumber is null)
			{
				_logger.LogWarning(
					"Сервис регистрации карточек сотрудников Манго для водителей отключен, "
					+ "либо не найден активный добавочный номер Манго водителя, доставляющего заказ {OrderId}, "
					+ "номер для связи с водителем будет содержать только номер линии Манго",
					orderId);

				return driversCallsLineNumber;
			}

			return $"{driversCallsLineNumber},,{driverMangoExtensionNumber.ExtensionNumber} (доб. {driverMangoExtensionNumber.ExtensionNumber})";
		}
	}
}
