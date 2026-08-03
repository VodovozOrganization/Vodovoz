using CustomerNotifications.Application.Providers;
using CustomerNotifications.Contracts;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sms;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.SmsNotifications;
using Vodovoz.Services;

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

		/// <summary>
		/// Заказы, по которым смс уведомление уже создано в рамках Unit Of Work.
		/// Нужен потому, что сессия работает в режиме FlushMode.Commit
		/// и запрос в базу не видит ещё не закоммиченные уведомления,
		/// а событие по одному заказу может публиковаться несколько раз в одной транзакции
		/// </summary>
		private readonly ConditionalWeakTable<IUnitOfWork, HashSet<int>> _ordersWithCreatedNotification =
			new ConditionalWeakTable<IUnitOfWork, HashSet<int>>();

		private readonly ILogger<CourierOnTheWaySmsNotificationCreator> _logger;
		private readonly ISmsNotifierSettings _smsNotifierSettings;
		private readonly ISmsNotificationRepository _smsNotificationRepository;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IDriverContactNumberProvider _driverContactNumberProvider;

		public CourierOnTheWaySmsNotificationCreator(
			ILogger<CourierOnTheWaySmsNotificationCreator> logger,
			ISmsNotifierSettings smsNotifierSettings,
			ISmsNotificationRepository smsNotificationRepository,
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IDriverContactNumberProvider driverContactNumberProvider)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_smsNotifierSettings = smsNotifierSettings ?? throw new ArgumentNullException(nameof(smsNotifierSettings));
			_smsNotificationRepository = smsNotificationRepository ?? throw new ArgumentNullException(nameof(smsNotificationRepository));
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_driverContactNumberProvider =
				driverContactNumberProvider ?? throw new ArgumentNullException(nameof(driverContactNumberProvider));
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
			if(domainEvent.OrderId is null)
			{
				_logger.LogWarning(
					"В событии {CustomerNotificationEventType} не заполнен код заказа, "
					+ "смс уведомление о том, что курьер в пути, не создаётся",
					domainEvent.CustomerNotificationEventType);

				return;
			}

			var orderId = domainEvent.OrderId.Value;

			var order = unitOfWork.GetById<Order>(orderId);

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

			if(mobilePhoneNumber is null)
			{
				_logger.LogInformation(
					"У заказа {OrderId} не указан корректный мобильный номер для связи, "
					+ "смс уведомление о том, что курьер в пути, не создаётся",
					orderId);

				return;
			}

			var ordersWithCreatedNotification = _ordersWithCreatedNotification.GetOrCreateValue(unitOfWork);

			if(ordersWithCreatedNotification.Contains(orderId)
				|| _smsNotificationRepository.HasCourierOnTheWaySmsNotification(unitOfWork, orderId))
			{
				_logger.LogInformation(
					"По заказу {OrderId} смс уведомление о том, что курьер в пути, уже создавалось, повторное не создаётся",
					orderId);

				return;
			}

			//получение текста сообщения
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
				await _driverContactNumberProvider.GetDriverContactNumberAsync(unitOfWork, orderId, cancellationToken);

			//формирование текста сообщения
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
				MobilePhone = mobilePhoneNumber,
				MessageText = messageText,
				Status = SmsNotificationStatus.New,
				NotifyTime = notifyTime,
				ExpiredTime = notifyTime.Add(_notificationLifetime)
			};

			await unitOfWork.SaveAsync(smsNotification, cancellationToken: cancellationToken);

			ordersWithCreatedNotification.Add(orderId);

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
