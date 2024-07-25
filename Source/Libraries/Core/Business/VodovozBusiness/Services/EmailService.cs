﻿using QS.DomainModel.UoW;
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
		private readonly OrderStateKey _orderStateKey;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IEmailRepository _emailRepository;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private readonly OrderStatus[] _emailRequiredOrderStatuses = new OrderStatus[]
		{
			OrderStatus.NewOrder,
			OrderStatus.Accepted,
			OrderStatus.WaitForPayment
		};

		private readonly PaymentType[] _emailRequiredOrderPaymentTypes = new PaymentType[]
		{
			PaymentType.Cashless
		};

		private readonly OrderDocumentType[] _orderDocumentBillTypes = new OrderDocumentType[]
		{
			OrderDocumentType.Bill,
			OrderDocumentType.SpecialBill
		};

		private readonly OrderDocumentType[] _orderDocumentUpdTypes = new OrderDocumentType[]
		{
			OrderDocumentType.UPD,
			OrderDocumentType.SpecialUPD
		};

		public EmailService(OrderStateKey orderStateKey,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IEmailRepository emailRepository,
			IDeliveryScheduleSettings deliveryScheduleSettings)
		{
			_orderStateKey = orderStateKey
				?? throw new ArgumentNullException(nameof(orderStateKey));
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository
				?? throw new ArgumentNullException(nameof(orderRepository));
			_emailRepository = emailRepository
				?? throw new ArgumentNullException(nameof(emailRepository));
			_deliveryScheduleSettings = deliveryScheduleSettings
				?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
		}

		public bool NeedSendBillToEmail(
			IUnitOfWork unitOfWork,
			Order order)
		{
			var sendedByEdo = _orderRepository
				.GetEdoContainersByOrderId(unitOfWork, order.Id)
				.Count(x => x.Type == Type.Bill) > 0;

			var sendedByEmail = _emailRepository.HaveSendedEmailForBill(order.Id);
			var sended = sendedByEdo || sendedByEmail;

			return _emailRequiredOrderStatuses.Contains(order.OrderStatus)
				&& _emailRequiredOrderPaymentTypes.Contains(order.PaymentType)
				&& !sended
				&& GetRequiredDocumentTypes(order)
					.Intersect(_orderDocumentBillTypes)
					.Any();
		}

		public bool NeedResendBillToEmail(
			IUnitOfWork unitOfWork,
			Order order)
		{
			var sendedByEdo = _orderRepository
				.GetEdoContainersByOrderId(unitOfWork, order.Id)
				.Count(x => x.Type == Type.Bill) > 0;

			var sendedByEmail = _emailRepository.HaveSendedEmailForBill(order.Id);

			return _emailRequiredOrderStatuses.Contains(order.OrderStatus)
				&& _emailRequiredOrderPaymentTypes.Contains(order.PaymentType)
				&& !sendedByEdo
				&& sendedByEmail
				&& GetRequiredDocumentTypes(order)
					.Intersect(_orderDocumentBillTypes)
					.Any();
		}

		/// <summary>
		/// Получение необходимых документов для заказа
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		public OrderDocumentType[] GetRequiredDocumentTypes(Order order)
		{
			//создаём объект-ключ на основе текущего заказа. Этот ключ содержит набор свойств,
			//по которым будет происходить подбор правила для создания набора документов
			_orderStateKey.InitializeFields(order);

			//обращение к хранилищу правил для получения массива типов документов по ключу
			return OrderDocumentRulesRepository.GetSetOfDocumets(_orderStateKey);
		}

		public Result SendUpdToEmailOnFinishIfNeeded(
			IUnitOfWork unitOfWork,
			Order order)
		{
			var threeMonthsBeforeToday = DateTime.Today.AddMonths(-3);
			
			if(_emailRepository.NeedSendDocumentsByEmailOnFinish(unitOfWork, order, _deliveryScheduleSettings)
				&& !_emailRepository.HasSendedEmailForUpd(order.Id)
				&& (order.DeliveryDate is null || order.DeliveryDate >= threeMonthsBeforeToday)
				&& !_orderRepository.HasSignedUpdDocumentFromEdo(unitOfWork, order.Id))
			{
				return SendUpdToEmail(unitOfWork, order);
			}

			return Result.Success();
		}

		public Result SendBillForClosingDocumentOrderToEmailOnFinishIfNeeded(IUnitOfWork unitOfWork, Order order)
		{
			if(_emailRepository.NeedSendDocumentsByEmailOnFinish(unitOfWork, order, _deliveryScheduleSettings, true)
				&& !_emailRepository.HaveSendedEmailForBill(order.Id)
				&& _orderRepository.GetEdoContainersByOrderId(unitOfWork, order.Id).Count(x => x.Type == Type.Bill) == 0)
			{
				return SendBillToEmail(unitOfWork, order);
			}

			return Result.Success();
		}

		public Result SendUpdToEmail(IUnitOfWork unitOfWork, Order order)
		{
			var document = order.OrderDocuments.FirstOrDefault(x => _orderDocumentUpdTypes.Contains(x.Type));

			if(document is null)
			{
				return Result.Failure(Errors.Email.Email.MissingDocumentForSending);
			}

			var emailAddress = GetEmailAddressForBill(order);

			if(emailAddress is null)
			{
				return Result.Failure(Errors.Email.Email.MissingEmailForRequiredMailType);
			}

			var dateTimeNow = DateTime.Now;

			var storedEmail = new StoredEmail
			{
				SendDate = dateTimeNow,
				StateChangeDate = dateTimeNow,
				State = StoredEmailStates.PreparingToSend,
				RecipientAddress = emailAddress.Address,
				ManualSending = false,
				Subject = document.Name,
				Author = order.Author
			};

			unitOfWork.Save(storedEmail);

			var updDocumentEmail = new UpdDocumentEmail
			{
				StoredEmail = storedEmail,
				Counterparty = order.Client,
				OrderDocument = document
			};

			unitOfWork.Save(updDocumentEmail);

			return Result.Success();
		}

		public Result SendBillToEmail(IUnitOfWork unitOfWork, Order order)
		{
			var document = order.OrderDocuments
				.FirstOrDefault(x => _orderDocumentBillTypes.Contains(x.Type)
					&& x.Order.Id == order.Id);

			if(document is null)
			{			
				return Result.Failure(Errors.Email.Email.MissingDocumentForSending);
			}

			try
			{
				var _emailAddressForBill = GetEmailAddressForBill(order);

				if(_emailAddressForBill is null)
				{
					return Result.Failure(Errors.Email.Email.MissingEmailForRequiredMailType);
				}

				var dateTimeNow = DateTime.Now;

				var storedEmail = new StoredEmail
				{
					SendDate = dateTimeNow,
					StateChangeDate = dateTimeNow,
					State = StoredEmailStates.PreparingToSend,
					RecipientAddress = _emailAddressForBill.Address,
					ManualSending = false,
					Subject = document.Name,
					Author = order.Author
				};

				unitOfWork.Save(storedEmail);

				BillDocumentEmail orderDocumentEmail = new BillDocumentEmail
				{
					StoredEmail = storedEmail,
					Counterparty = order.Client,
					OrderDocument = document
				};

				unitOfWork.Save(orderDocumentEmail);
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
