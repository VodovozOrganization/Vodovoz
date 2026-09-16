using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.SmsNotifications;
using Vodovoz.Services;

namespace Vodovoz.Domain.Sms
{
	/// <summary>
	/// Формирует СМС-уведомления по заказам и недовозам.
	/// </summary>
	public class SmsNotifier : ISmsNotifier
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ISmsNotifierSettings _smsNotifierSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly ISmsNotificationRepository _smsNotificationRepository;

		/// <summary>
		/// Создаёт сервис формирования СМС-уведомлений.
		/// </summary>
		/// <param name="uowFactory">Фабрика единиц работы для проверки и сохранения уведомлений.</param>
		/// <param name="smsNotifierSettings">Настройки СМС-уведомлений.</param>
		/// <param name="orderRepository">Репозиторий истории заказов.</param>
		/// <param name="smsNotificationRepository">Репозиторий СМС-уведомлений.</param>
		public SmsNotifier(IUnitOfWorkFactory uowFactory, ISmsNotifierSettings smsNotifierSettings,
			IOrderRepository orderRepository, ISmsNotificationRepository smsNotificationRepository)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			this._smsNotifierSettings = smsNotifierSettings ?? throw new ArgumentNullException(nameof(smsNotifierSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_smsNotificationRepository = smsNotificationRepository ?? throw new ArgumentNullException(nameof(smsNotificationRepository));
		}

		/// <inheritdoc/>
		public void CreateNewClientSmsNotification(Order order)
		{
			if(!_smsNotifierSettings.IsSmsNotificationsEnabled) {
				return;
			}

			if(order == null || order.Id == 0 || order.OrderStatus == OrderStatus.NewOrder || !order.DeliveryDate.HasValue) {
				return;
			}
			if(!order.OrderItems.Any(item => item.Count > 0
				&& item.Nomenclature.IsWater19L
				&& item.Nomenclature.Id != _smsNotifierSettings.FullDisposable19LNomenclatureId)) {
				return;
			}
			//проверка даты без времени
			if(order.DeliveryDate < DateTime.Today) {
				return;
			}

			using(var uow = _uowFactory.CreateWithoutRoot()) {
				var notifications = _smsNotificationRepository.GetNewClientSmsNotifications(uow, order.Client.Id);
				if(notifications.Any(notification => notification.Order?.Id == order.Id)) {
					return;
				}

				var orderStatuses = _orderRepository.GetValidStatusesToUseActionBottle();
				if(_orderRepository.HasAnotherOrderWithWater19L(uow, order.Client.Id, order.Id, orderStatuses)) {
					return;
				}
			}

			//формирование номера мобильного телефона
			string mobilePhoneNumber = GetMobilePhoneNumberForOrder(order);
			if(string.IsNullOrWhiteSpace(mobilePhoneNumber)) {
				return;
			}

			//получение текста сообщения
			string messageText = _smsNotifierSettings.NewClientSmsTextTemplate;

			//формирование текста сообщения
			const string orderIdVariable = "$order_id$";
			const string deliveryDateVariable = "$delivery_date$";
			const string deliveryTimeVariable = "$delivery_time$";
			messageText = messageText.Replace(orderIdVariable, $"{order.Id}");
			string orderScheduleTimeString = order.DeliverySchedule != null ? $"c {order.DeliverySchedule.From.Hours}:{order.DeliverySchedule.From.Minutes:D2} по {order.DeliverySchedule.To.Hours}:{order.DeliverySchedule.To.Minutes:D2}" : "";
			messageText = messageText.Replace(deliveryDateVariable, $"{order.DeliveryDate.Value.ToString("dd.MM.yyyy")}");
			messageText = messageText.Replace(deliveryTimeVariable, $"{orderScheduleTimeString}");

			//создание нового уведомления для отправки
			using(var uow = _uowFactory.CreateWithNewRoot<NewClientSmsNotification>()) {
				uow.Root.Order = order;
				uow.Root.Counterparty = order.Client;
				uow.Root.NotifyTime = DateTime.Now;
				uow.Root.MobilePhone = mobilePhoneNumber;
				uow.Root.Status = SmsNotificationStatus.New;
				uow.Root.MessageText = messageText;
				uow.Root.ExpiredTime = new DateTime(
					order.DeliveryDate.Value.Year,
					order.DeliveryDate.Value.Month,
					order.DeliveryDate.Value.Day,
					23, 59, 59
				);

				uow.Save();
			}
		}
		
		/// <summary>
		/// Создает новое смс-уведомление для недовоза, если до клиента не дозвонились.
		/// Все равно происходит перенос заказа на следующий день.
		/// </summary>
		/// <param name="undeliveredOrder"></param>
		/// <param name="externalUow">Используется, если надо создать <see cref="UndeliveryNotApprovedSmsNotification"/>
		/// до сохранения самого недовоза в базу</param>
		public void NotifyUndeliveryAutoTransferNotApproved(UndeliveredOrder undeliveredOrder, IUnitOfWork externalUow = null)
		{
			if(!_smsNotifierSettings.IsSmsNotificationsEnabled)
			{
				return;
			}

			//необходимые проверки именно НОВОГО заказа
			if(undeliveredOrder.NewOrder == null || undeliveredOrder.NewOrder.Id == 0
			                                     || undeliveredOrder.NewOrder.OrderStatus == OrderStatus.NewOrder
			                                     || !undeliveredOrder.NewOrder.DeliveryDate.HasValue)
			{
				return;
			}
			if(undeliveredOrder.NewOrder.Client.FirstOrder == null
			   || undeliveredOrder.OrderTransferType != TransferType.AutoTransferNotApproved)
			{
				return;
			}
			
			//проверка уже существующих ранее уведомлений по недовозу
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var existsNotifications = uow.Session.QueryOver<UndeliveryNotApprovedSmsNotification>()
					.Where(x => x.UndeliveredOrder.Id == undeliveredOrder.Id)
					.List();
				if(existsNotifications.Any())
				{
					return;
				}
			}
			
			//формирование номера мобильного телефона
			var mobilePhoneNumber = GetMobilePhoneNumberForOrder(undeliveredOrder.OldOrder);
			if(string.IsNullOrWhiteSpace(mobilePhoneNumber))
			{
				return;
			}
			
			//получение текста сообщения
			var msgToSend = _smsNotifierSettings.UndeliveryAutoTransferNotApprovedTextTemplate;

			//формирование текста сообщения
			//метки для замены в тексте сообщения из базы
			const string deliveryDateVariable = "$delivery_date$";
			const string deliveryTimeVariable = "$delivery_time$";
			//формирование времени доставки и проверка на Null exception
			var orderScheduleTimeString = undeliveredOrder.NewOrder.DeliverySchedule == null
				? ""
				: $"c {undeliveredOrder.NewOrder.DeliverySchedule.From.Hours}-{undeliveredOrder.NewOrder.DeliverySchedule.To.Hours}";
			if(string.IsNullOrWhiteSpace(orderScheduleTimeString))
			{
				return;
			}
			//замена метки на дату доставки
			msgToSend = msgToSend.Replace(deliveryDateVariable, $"{undeliveredOrder.NewOrder.DeliveryDate.Value:dd.MM}");
			//замена метки на время доставки
			msgToSend = msgToSend.Replace(deliveryTimeVariable, $"{orderScheduleTimeString}");

			void FillNotification(UndeliveryNotApprovedSmsNotification notification)
			{
				notification.UndeliveredOrder = undeliveredOrder;
				notification.Counterparty = undeliveredOrder.NewOrder.Client;
				notification.NotifyTime = DateTime.Now;
				notification.MobilePhone = mobilePhoneNumber;
				notification.Status = SmsNotificationStatus.New;
				notification.MessageText = msgToSend;
				notification.ExpiredTime = DateTime.Now.AddMinutes(30);
			}
			//создание нового уведомления для отправки
			if(externalUow != null)
			{
				var notification = new UndeliveryNotApprovedSmsNotification();
				FillNotification(notification);
				externalUow.Save(notification);
				return;
			}

			using(var uow = _uowFactory.CreateWithNewRoot<UndeliveryNotApprovedSmsNotification>())
			{
				FillNotification(uow.Root);
				uow.Save();
			}
		}

		private string GetMobilePhoneNumberForOrder(Order order)
		{
			Phone phone = null;
			if(order.DeliveryPoint != null && order.DeliveryPoint.Phones.Any())
			{
				phone = order.DeliveryPoint.Phones.FirstOrDefault();
			}
			else
			{
				phone = order.Client.Phones.FirstOrDefault();
			}

			if(phone == null)
			{
				return null;
			}

			var stringPhoneNumber = phone.DigitsNumber.TrimStart('+').TrimStart('7').TrimStart('8');
			if(stringPhoneNumber.Length == 0 || stringPhoneNumber.First() != '9' || stringPhoneNumber.Length != 10)
			{
				return null;
			}
			return $"+7{stringPhoneNumber}";
		}
	}
}
