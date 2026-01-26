using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Requests;
using FastPaymentsApi.Contracts.Responses;
using FastPaymentsAPI.Library.Managers;
using FastPaymentsAPI.Library.Validators;
using Mailjet.Api.Abstractions;
using MassTransit;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Common;

namespace FastPaymentsAPI.Library.Models
{
	public class FastPaymentOrderService : IFastPaymentOrderService
	{
		private readonly IUnitOfWork _uow;
		private readonly IOrderRepository _orderRepository;
		private readonly IFastPaymentValidator _fastPaymentValidator;
		private readonly IEmailSettings _emailSettings;
		private readonly IOrderRequestManager _orderRequestManager;

		public FastPaymentOrderService(
			IUnitOfWork uow,
			IOrderRepository orderRepository,
			IFastPaymentValidator fastPaymentValidator,
			IEmailSettings emailSettings,
			IOrderRequestManager orderRequestManager)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_fastPaymentValidator = fastPaymentValidator ?? throw new ArgumentNullException(nameof(fastPaymentValidator));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_orderRequestManager = orderRequestManager ?? throw new ArgumentNullException(nameof(orderRequestManager));
		}

		public Order GetOrder(int orderId)
		{
			return _orderRepository.GetOrder(_uow, orderId);
		}

		public string ValidateParameters(int orderId) => _fastPaymentValidator.Validate(orderId);
		public string ValidateParameters(RequestRegisterOnlineOrderDTO registerOnlineOrderDto, FastPaymentRequestFromType fastPaymentRequestFromType) =>
			_fastPaymentValidator.Validate(registerOnlineOrderDto, fastPaymentRequestFromType);
		public string ValidateParameters(int orderId, ref string phoneNumber) => _fastPaymentValidator.Validate(orderId, ref phoneNumber);
		public string ValidateOrder(Order order, int orderId) => _fastPaymentValidator.Validate(order, orderId);
		public string ValidateOnlineOrder(decimal onlineOrderSum) => _fastPaymentValidator.ValidateOnlineOrder(onlineOrderSum);
		public string GetPayUrlForOnlineOrder(Guid fastPaymentGuid) => _orderRequestManager.GetVodovozFastPayUrl(fastPaymentGuid);

		public Task<OrderRegistrationResponseDTO> RegisterOrder(
			Order order, Guid fastPaymentGuid, Organization organization, string phoneNumber = null, bool isQr = true)
		{
			return _orderRequestManager.RegisterOrder(order, fastPaymentGuid, organization, phoneNumber, isQr);
		}
		
		public Task<OrderRegistrationResponseDTO> RegisterOnlineOrder(
			RequestRegisterOnlineOrderDTO registerOnlineOrderDto,
			Organization organization,
			FastPaymentRequestFromType fastPaymentRequestFromType)
		{
			return _orderRequestManager.RegisterOnlineOrder(registerOnlineOrderDto, organization, fastPaymentRequestFromType);
		}

		public Task<OrderInfoResponseDTO> GetOrderInfo(string ticket, Organization organization)
		{
			return _orderRequestManager.GetOrderInfo(ticket, organization);
		}
		
		public Task<CancelPaymentResponseDTO> CancelPayment(string ticket, Organization organization)
		{
			return _orderRequestManager.CancelPayment(ticket, organization);
		}
		
		public PaidOrderInfoDTO GetPaidOrderInfo(string data)
		{
			using TextReader reader = new StringReader(data);
			return (PaidOrderInfoDTO)new XmlSerializer(typeof(PaidOrderInfoDTO)).Deserialize(reader);
		}
	}
}
