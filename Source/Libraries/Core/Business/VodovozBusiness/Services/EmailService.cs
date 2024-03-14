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
using Vodovoz.Errors;
using Vodovoz.Settings.Delivery;
using Vodovoz.Tools.Orders;
using Email = Vodovoz.Domain.Contacts.Email;
using Order = Vodovoz.Domain.Orders.Order;
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

		public Result SendUpdToEmailOnFinishIfNeeded(IUnitOfWork uow, Order order, IEmailRepository emailRepository, IDeliveryScheduleSettings deliveryScheduleSettings)
		{
			if(emailRepository.NeedSendDocumentsByEmailOnFinish(uow, order, deliveryScheduleSettings)
				&& !emailRepository.HasSendedEmailForUpd(order.Id))
			{
				return SendUpdToEmail(uow, order);
			}

			return Result.Success();
		}

		public Result SendBillForClosingDocumentOrderToEmailOnFinishIfNeeded(IUnitOfWork uow, Order order, IEmailRepository emailRepository, IOrderRepository orderRepository,
			IDeliveryScheduleSettings deliveryScheduleSettings)
		{
			if(emailRepository.NeedSendDocumentsByEmailOnFinish(uow, order, deliveryScheduleSettings, true)
				&& !emailRepository.HaveSendedEmailForBill(order.Id)
				&& orderRepository.GetEdoContainersByOrderId(uow, order.Id).Count(x => x.Type == Type.Bill) == 0)
			{
				return SendBillToEmail(uow, order);								
			}

			return Result.Success();
		}

		public Result SendUpdToEmail(IUnitOfWork uow, Order order)
		{
			var document = order.OrderDocuments.FirstOrDefault(x => x.Type == OrderDocumentType.UPD || x.Type == OrderDocumentType.SpecialUPD);

			if(document == null)
			{
				return Result.Failure(Errors.Email.Email.MissingDocumentForSending);
			}

			var emailAddress = GetEmailAddressForBill(order);

			if(emailAddress == null)
			{
				return Result.Failure(Errors.Email.Email.MissingEmailForRequiredMailType);
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

			return Result.Success();
		}

		public Result SendBillToEmail(IUnitOfWork uow, Order order)
		{
			var document = order.OrderDocuments.FirstOrDefault(x => (x.Type == OrderDocumentType.Bill
				|| x.Type == OrderDocumentType.SpecialBill)
			&& x.Order.Id == order.Id);

			if(document == null)
			{			
				return Result.Failure(Errors.Email.Email.MissingDocumentForSending);
			}

			try
			{
				var _emailAddressForBill = GetEmailAddressForBill(order);

				if(_emailAddressForBill == null)
				{
					return Result.Failure(Errors.Email.Email.MissingEmailForRequiredMailType);
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

			return Result.Success();
		}

		public Email GetEmailAddressForBill(Order order) =>
			order.Client.Emails.FirstOrDefault(x => x.EmailType?.EmailPurpose == EmailPurpose.ForBills || x.EmailType == null);
	}
}
