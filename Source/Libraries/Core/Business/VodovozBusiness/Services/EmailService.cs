using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Tools.Orders;
using Type = Vodovoz.Domain.Orders.Documents.Type;

namespace Vodovoz.Services
{
	public class EmailService : IEmailService
	{
		public bool NeedSendBillToEmail(IUnitOfWork uow, Order order, IOrderRepository orderRepository, IEmailRepository emailRepository)
		{
			var notSendedByEdo = orderRepository.GetEdoContainersByOrderId(uow, order.Id).Count(x => x.Type == Type.Bill) == 0;
			var notSendedByEmail = !emailRepository.HaveSendedEmailForBill(order.Id);
			var notSended = notSendedByEdo && notSendedByEmail;
			if((order.OrderStatus == OrderStatus.NewOrder || order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.WaitForPayment)
			   && order.PaymentType == PaymentType.Cashless
			   && notSended)
			{
				//Проверка должен ли формироваться счет для текущего заказа
				var requirementDocTypes = GetRequirementDocTypes(order);
				return requirementDocTypes.Contains(OrderDocumentType.Bill) || requirementDocTypes.Contains(OrderDocumentType.SpecialBill);
			}
			return false;
		}

		public OrderDocumentType[] GetRequirementDocTypes(Order order)
		{
			//создаём объект-ключ на основе текущего заказа. Этот ключ содержит набор свойств,
			//по которым будет происходить подбор правила для создания набора документов
			var key = new OrderStateKey(order);

			//обращение к хранилищу правил для получения массива типов документов по ключу
			return OrderDocumentRulesRepository.GetSetOfDocumets(key);
		}

		public void SendUpdToEmailOnFinish(IUnitOfWork uow, Order order, IEmailRepository emailRepository, IDeliveryScheduleParametersProvider deliveryScheduleParametersProvider)
		{
			if(emailRepository.NeedSendDocumentsByEmailOnFinish(uow, order, deliveryScheduleParametersProvider)
				&& !emailRepository.HasSendedEmailForUpd(order.Id))
			{
				SendUpdToEmail(uow, order);
			}
		}

		public void SendBillForClosingDocumentOrderToEmailOnFinish(IUnitOfWork uow, Order order, IEmailRepository emailRepository, IOrderRepository orderRepository,
			IDeliveryScheduleParametersProvider deliveryScheduleParametersProvider)
		{
			if(emailRepository.NeedSendDocumentsByEmailOnFinish(uow, order, deliveryScheduleParametersProvider)
				&& !emailRepository.HaveSendedEmailForBill(order.Id)
				&& orderRepository.GetEdoContainersByOrderId(uow, order.Id).Count(x => x.Type == Type.Bill) == 0)
			{
				SendBillToEmail(uow, order, emailRepository);
			}
		}

		public void SendUpdToEmail(IUnitOfWork uow, Order order)
		{
			var document = order.OrderDocuments.FirstOrDefault(x => x.Type == OrderDocumentType.UPD || x.Type == OrderDocumentType.SpecialUPD);

			if(document == null)
			{
				return;
			}

			var emailAddress = GetEmailAddressForBill(order);

			if(emailAddress == null)
			{
				return;
			}

			var storedEmail = new StoredEmail
			{
				SendDate = DateTime.Now,
				StateChangeDate = DateTime.Now,
				State = StoredEmailStates.PreparingToSend,
				RecipientAddress = emailAddress.Address,
				ManualSending = false,
				Subject = document.Name,
				Author = order.Author
			};

			uow.Save(storedEmail);

			var updDocumentEmail = new UpdDocumentEmail
			{
				StoredEmail = storedEmail,
				Counterparty = order.Client,
				OrderDocument = document
			};

			uow.Save(updDocumentEmail);
		}

		public void SendBillToEmail(IUnitOfWork uow, Order order, IEmailRepository emailRepository)
		{
			var document = order.OrderDocuments.FirstOrDefault(x => (x.Type == OrderDocumentType.Bill
				|| x.Type == OrderDocumentType.SpecialBill)
			&& x.Order.Id == order.Id);

			if(document == null)
			{			
				return;
			}

			try
			{
				if(emailRepository.HaveSendedEmailForBill(order.Id))
				{
					return;
				}

				var _emailAddressForBill = GetEmailAddressForBill(order);

				if(_emailAddressForBill == null)
				{
					throw new ArgumentNullException(nameof(_emailAddressForBill));
				}

				var storedEmail = new StoredEmail
				{
					SendDate = DateTime.Now,
					StateChangeDate = DateTime.Now,
					State = StoredEmailStates.PreparingToSend,
					RecipientAddress = _emailAddressForBill.Address,
					ManualSending = false,
					Subject = document.Name,
					Author = order.Author
				};

				uow.Save(storedEmail);

				BillDocumentEmail orderDocumentEmail = new BillDocumentEmail
				{
					StoredEmail = storedEmail,
					Counterparty = order.Client,
					OrderDocument = document
				};

				uow.Save(orderDocumentEmail);
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		public Email GetEmailAddressForBill(Order order) =>
			order.Client.Emails.FirstOrDefault(x => x.EmailType?.EmailPurpose == EmailPurpose.ForBills || x.EmailType == null);
	}
}
