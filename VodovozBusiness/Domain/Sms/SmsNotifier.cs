using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;

namespace Vodovoz.Domain.Sms
{
	public class SmsNotifier
	{
		private readonly ISmsNotifierParametersProvider smsNotifierParametersProvider;

		public SmsNotifier(ISmsNotifierParametersProvider smsNotifierParametersProvider)
		{
			this.smsNotifierParametersProvider = smsNotifierParametersProvider ?? throw new ArgumentNullException(nameof(smsNotifierParametersProvider));
		}

		/// <summary>
		/// Создает новое смс уведомление для заказа, если этот заказ является первым заказом у контрагента
		/// и если не было создано других смс уведомлений по этому заказу
		/// </summary>
		public void NotifyIfNewClient(Order order)
		{
			if(!smsNotifierParametersProvider.IsSmsNotificationsEnabled) {
				return;
			}

			if(order == null || order.Id == 0 || order.OrderStatus == OrderStatus.NewOrder || !order.DeliveryDate.HasValue) {
				return;
			}
			if(order.Client.FirstOrder == null || order.Client.FirstOrder.Id != order.Id) {
				return;
			}
			//проверка даты без времени
			if(order.DeliveryDate < DateTime.Today) {
				return;
			}

			//проверка уже существующих ранее уведомлений
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var existsNotifications = uow.Session.QueryOver<NewClientSmsNotification>()
					.Where(x => x.Counterparty.Id == order.Client.Id)
					.List();
				if(existsNotifications.Any()) {
					return;
				}
			}

			//формирование номера мобильного телефона
			string mobilePhoneNumber = GetMobilePhoneNumberForOrder(order);
			if(string.IsNullOrWhiteSpace(mobilePhoneNumber)) {
				return;
			}

			//получение текста сообщения
			string messageText = smsNotifierParametersProvider.GetNewClientSmsTextTemplate();

			//формирование текста сообщения
			const string orderIdVariable = "$order_id$";
			const string deliveryDateVariable = "$delivery_date$";
			const string deliveryTimeVariable = "$delivery_time$";
			messageText = messageText.Replace(orderIdVariable, $"{order.Id}");
			string orderScheduleTimeString = order.DeliverySchedule != null ? $"c {order.DeliverySchedule.From.Hours}:{order.DeliverySchedule.From.Minutes:D2} по {order.DeliverySchedule.To.Hours}:{order.DeliverySchedule.To.Minutes:D2}" : "";
			messageText = messageText.Replace(deliveryDateVariable, $"{order.DeliveryDate.Value.ToString("dd.MM.yyyy")}");
			messageText = messageText.Replace(deliveryTimeVariable, $"{orderScheduleTimeString}");

			//создание нового уведомления для отправки
			using(var uow = UnitOfWorkFactory.CreateWithNewRoot<NewClientSmsNotification>()) {
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
		public void NotifyUndeliveryAutoTransferNotApproved(UndeliveredOrder undeliveredOrder)
		{
			if(!smsNotifierParametersProvider.IsSmsNotificationsEnabled) 
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
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) 
			{
				var existsNotifications = uow.Session.QueryOver<UndeliveryNotApprovedSmsNotification>()
					.Where(x => x.UndeliveredOrder.Id == undeliveredOrder.Id) //
					.List();
				if(existsNotifications.Any()) 
				{
					return;
				}
			}
			
			//формирование номера мобильного телефона
			string mobilePhoneNumber = GetMobilePhoneNumberForUndeliveryOrderId(undeliveredOrder.Id);
			if(string.IsNullOrWhiteSpace(mobilePhoneNumber)) 
			{
				return;
			}
			
			//получение текста сообщения
			var msgToSend = smsNotifierParametersProvider.GetUndeliveryAutoTransferNotApprovedTextTemplate();

			//формирование текста сообщения
			//метки для замены в тексте сообщения из базы
			const string deliveryDateVariable = "$delivery_date$";
			const string deliveryTimeVariable = "$delivery_time$";
			//формирование времени доставки и проверка на Null exception
			string orderScheduleTimeString = undeliveredOrder.NewOrder.DeliverySchedule 
			                                 != null ? $"c {undeliveredOrder.NewOrder.DeliverySchedule.From.Hours}" +
			                                           $":{undeliveredOrder.NewOrder.DeliverySchedule.From.Minutes:D2} " +
			                                           $"по {undeliveredOrder.NewOrder.DeliverySchedule.To.Hours}" +
			                                           $":{undeliveredOrder.NewOrder.DeliverySchedule.To.Minutes:D2}" : "";
			if(string.IsNullOrWhiteSpace(orderScheduleTimeString))
			{
				return;
			}
			//замена метки на дату доставки
			msgToSend = msgToSend.Replace(deliveryDateVariable, $"{undeliveredOrder.NewOrder.DeliveryDate.Value.ToString("dd.MM.yyyy")}");
			//замена метки на время доставки
			msgToSend = msgToSend.Replace(deliveryTimeVariable, $"{orderScheduleTimeString}");

			//создание нового уведомления для отправки
			using(var uow = UnitOfWorkFactory.CreateWithNewRoot<UndeliveryNotApprovedSmsNotification>()) 
			{
				uow.Root.UndeliveredOrder = undeliveredOrder;
				uow.Root.Counterparty = undeliveredOrder.NewOrder.Client;
				uow.Root.NotifyTime = DateTime.Now;
				uow.Root.MobilePhone = mobilePhoneNumber;
				uow.Root.Status = SmsNotificationStatus.New;
				uow.Root.MessageText = msgToSend;
				uow.Root.ExpiredTime = DateTime.Now.AddMinutes(30);
				
				uow.Save();
			}

		}

		private string GetMobilePhoneNumberForOrder(Order order)
		{
			Phone phone = null;
			if(order.DeliveryPoint != null && !order.DeliveryPoint.Phones.Any()) {
				phone = order.DeliveryPoint.Phones.FirstOrDefault();
			} else {
				phone = order.Client.Phones.FirstOrDefault();
			}
			if(phone == null) {
				return null;
			}

			string stringPhoneNumber = phone.DigitsNumber.TrimStart('+').TrimStart('7').TrimStart('8');
			if(stringPhoneNumber.Length == 0 || stringPhoneNumber.First() != '9' || stringPhoneNumber.Length != 10) {
				return null;
			}
			return $"+7{stringPhoneNumber}";
		}
		
		private string GetMobilePhoneNumberForUndeliveryOrderId(int id)
		{
			var uow = UnitOfWorkFactory.CreateForRoot<UndeliveredOrder>(id);
			var order = uow.Root.OldOrder;
			Phone phone = null;
			if(order.DeliveryPoint != null && order.DeliveryPoint.Phones.Count > 0) 
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

			//федеральный мобильный номер
			string stringPhoneNumber = phone.DigitsNumber.TrimStart('+').TrimStart('7').TrimStart('8');
			if(stringPhoneNumber.Length == 0 || stringPhoneNumber.First() != '9' || stringPhoneNumber.Length != 10) 
			{
				return null;
			}
			return $"+7{stringPhoneNumber}";
		}
	}
}
