using CustomerNotifications.Contracts;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sms;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.SmsNotifications;
using Vodovoz.Services;
using VodovozBusiness.Services.Logistics;

namespace CustomerNotifications.Application.SmsNotificationCreators
{
	/// <summary>
	/// Создаёт смс уведомление о том, что курьер в пути,
	/// для клиентов, не пользующихся мобильным приложением
	/// </summary>
	public class CourierOnTheWaySmsNotificationCreator : ISmsNotificationCreator<CustomerNotificationDomainEvent>
	{
		/// <summary>
		/// Время, в течение которого отправка смс уведомления остаётся актуальной
		/// </summary>
		private static readonly TimeSpan _notificationLifetime = TimeSpan.FromHours(1);

		private readonly ILogger<CourierOnTheWaySmsNotificationCreator> _logger;
		private readonly ISmsNotifierSettings _smsNotifierSettings;
		private readonly ISmsNotificationRepository _smsNotificationRepository;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IDriverContactNumberService _driverContactNumberService;

		public CourierOnTheWaySmsNotificationCreator(
			ILogger<CourierOnTheWaySmsNotificationCreator> logger,
			ISmsNotifierSettings smsNotifierSettings,
			ISmsNotificationRepository smsNotificationRepository,
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IOrderRepository orderRepository,
			IEmployeeRepository employeeRepository,
			IDriverContactNumberService driverContactNumberService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_smsNotifierSettings = smsNotifierSettings ?? throw new ArgumentNullException(nameof(smsNotifierSettings));
			_smsNotificationRepository = smsNotificationRepository ?? throw new ArgumentNullException(nameof(smsNotificationRepository));
			_externalCounterpartyRepository = externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_driverContactNumberService = driverContactNumberService ?? throw new ArgumentNullException(nameof(driverContactNumberService));
		}

		/// <inheritdoc/>
		public bool CanCreate(CustomerNotificationDomainEvent domainEvent) =>
			domainEvent?.CustomerNotificationEventType == CustomerNotificationEventType.CourierOnTheWay;

		/// <inheritdoc/>
		public async Task CreateAsync(
			IUnitOfWork unitOfWork,
			CustomerNotificationDomainEvent domainEvent,
			CancellationToken cancellationToken = default)
		{
			if(domainEvent?.OrderId is null)
			{
				_logger.LogWarning(
					"В событии {CustomerNotificationEventType} не заполнен код заказа, "
					+ "смс уведомление о том, что курьер в пути, не создаётся",
					domainEvent.CustomerNotificationEventType);

				return;
			}

			var orderId = domainEvent.OrderId.Value;

			var order =
				await _orderRepository.GetOrderByIdAsync(unitOfWork, orderId, cancellationToken);

			if(order is null)
			{
				_logger.LogWarning(
					"Заказ {OrderId} не найден, смс уведомление о том, что курьер в пути, не создаётся",
					orderId);

				return;
			}

			if(order.SelfDelivery)
			{
				_logger.LogInformation(
					"Заказ {OrderId} является самовывозом, смс уведомление о том, что курьер в пути, не создаётся",
					orderId);

				return;
			}

			if(order.Client is null)
			{
				_logger.LogWarning(
					"У заказа {OrderId} не указан контрагент, смс уведомление о том, что курьер в пути, не создаётся",
					orderId);

				return;
			}

			if(_externalCounterpartyRepository.HasActiveMobileAppUser(unitOfWork, order.Client.Id))
			{
				_logger.LogInformation(
					"Контрагент {CounterpartyId} пользуется мобильным приложением, "
					+ "смс уведомление о том, что курьер в пути, по заказу {OrderId} не создаётся",
					order.Client.Id,
					orderId);

				return;
			}

			var mobilePhoneNumber = GetMobilePhoneNumber(order);

			if(string.IsNullOrWhiteSpace(mobilePhoneNumber))
			{
				_logger.LogInformation(
					"У заказа {OrderId} не указан корректный мобильный номер для связи, "
					+ "смс уведомление о том, что курьер в пути, не создаётся",
					orderId);

				return;
			}

			var driver = await _employeeRepository.GetDriverByOrderId(unitOfWork, orderId, cancellationToken);

			if(driver is null)
			{
				_logger.LogWarning(
					"По заказу {OrderId} не найден назначенный курьер, смс уведомление о том, что курьер в пути, не создаётся",
					orderId);
				return;
			}

			if(_smsNotificationRepository.HasCourierOnTheWaySmsNotification(unitOfWork, orderId, driver.Id))
			{
				_logger.LogInformation(
					"По заказу {OrderId} (водитель {DriverId}) смс уведомление о том, что курьер в пути, уже создавалось, повторное не создаётся",
					orderId,
					driver.Id);

				return;
			}

			var messageText = _smsNotifierSettings.CourierOnTheWaySmsTextTemplate;

			if(string.IsNullOrWhiteSpace(messageText))
			{
				_logger.LogWarning(
					"Не заполнен шаблон текста смс уведомления о том, что курьер в пути, "
					+ "смс уведомление по заказу {OrderId} не создаётся",
					orderId);

				return;
			}

			var driverPhone =
				await _driverContactNumberService.GetDriverContactNumberAsync(unitOfWork, orderId, cancellationToken);

			const string orderIdVariable = "$order_id$";
			const string driverPhoneVariable = "$driver_phone$";

			messageText = messageText
				.Replace(orderIdVariable, orderId.ToString())
				.Replace(driverPhoneVariable, driverPhone);

			var notifyTime = DateTime.Now;

			var smsNotification = new CourierOnTheWaySmsNotification
			{
				Order = order,
				Counterparty = order.Client,
				Driver = driver,
				MobilePhone = mobilePhoneNumber,
				MessageText = messageText,
				Status = SmsNotificationStatus.New,
				NotifyTime = notifyTime,
				ExpiredTime = notifyTime.Add(_notificationLifetime)
			};

			await unitOfWork.SaveAsync(smsNotification, cancellationToken: cancellationToken);

			_logger.LogInformation(
				"Создано смс уведомление о том, что курьер в пути, по заказу {OrderId} на номер {MobilePhoneNumber}",
				orderId,
				mobilePhoneNumber);
		}

		/// <summary>
		/// Возвращает номер для связи из заказа в формате +7XXXXXXXXXX,
		/// либо <c>null</c>, если номер не заполнен или не является мобильным
		/// </summary>
		private string GetMobilePhoneNumber(Order order)
		{
			var digitsNumber = order.ContactPhone?.DigitsNumber;

			if(string.IsNullOrWhiteSpace(digitsNumber)
				|| digitsNumber.Length != 10
				|| digitsNumber.First() != '9')
			{
				return null;
			}

			return $"+7{digitsNumber}";
		}
	}
}
