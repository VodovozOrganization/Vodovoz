using System;
using QS.DomainModel.UoW;
using QS.Contacts;
using System.Linq;
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
			const string deliveryDateTimeVariable = "$delivery_date_time$";
			messageText = messageText.Replace(orderIdVariable, $"{order.Id}");
			string orderScheduleTimeString = order.DeliverySchedule != null ? $"c {order.DeliverySchedule.From.Hours}:{order.DeliverySchedule.From.Minutes:D2} по {order.DeliverySchedule.To.Hours}:{order.DeliverySchedule.To.Minutes:D2}" : "";
			messageText = messageText.Replace(deliveryDateTimeVariable, $"{order.DeliveryDate.Value.ToString("dd.MM.yyyy")} {orderScheduleTimeString}");

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
	}
}
