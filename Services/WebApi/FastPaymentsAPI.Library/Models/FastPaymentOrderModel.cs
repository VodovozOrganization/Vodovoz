using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s.Responses;
using FastPaymentsAPI.Library.Managers;
using FastPaymentsAPI.Library.Validators;
using Mailjet.Api.Abstractions;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.DomainModel.UoW;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Parameters;
using VodovozInfrastructure.Configuration;

namespace FastPaymentsAPI.Library.Models
{
	public class FastPaymentOrderModel : IFastPaymentOrderModel
	{
		private readonly IUnitOfWork _uow;
		private readonly IOrderRepository _orderRepository;
		private readonly IFastPaymentValidator _fastPaymentValidator;
		private readonly IEmailParametersProvider _emailParametersProvider;
		private readonly IOrderRequestManager _orderRequestManager;

		public FastPaymentOrderModel(
			IUnitOfWork uow,
			IOrderRepository orderRepository,
			IFastPaymentValidator fastPaymentValidator,
			IEmailParametersProvider emailParametersProvider,
			IOrderRequestManager orderRequestManager)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_fastPaymentValidator = fastPaymentValidator ?? throw new ArgumentNullException(nameof(fastPaymentValidator));
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));
			_orderRequestManager = orderRequestManager ?? throw new ArgumentNullException(nameof(orderRequestManager));
		}

		public Order GetOrder(int orderId)
		{
			return _orderRepository.GetOrder(_uow, orderId);
		}

		public string ValidateParameters(int orderId) => _fastPaymentValidator.Validate(orderId);
		public string ValidateParameters(int orderId, ref string phoneNumber) => _fastPaymentValidator.Validate(orderId, ref phoneNumber);
		public string ValidateOrder(Order order, int orderId) => _fastPaymentValidator.Validate(order, orderId);

		public Task<OrderRegistrationResponseDTO> RegisterOrder(Order order, string phoneNumber = null)
		{
			return _orderRequestManager.RegisterOrder(order, phoneNumber);
		}

		public Task<OrderInfoResponseDTO> GetOrderInfo(string ticket)
		{
			return _orderRequestManager.GetOrderInfo(ticket);
		}
		
		public Task<CancelPaymentResponseDTO> CancelPayment(string ticket)
		{
			return _orderRequestManager.CancelPayment(ticket);
		}

		public void NotifyEmployee(string orderNumber, string signature)
		{
			var configuration = _uow.GetAll<InstanceMailingConfiguration>().FirstOrDefault();

			string messageText = $"Оповещение о пришедшей оплате с неверной подписью: {signature}" +
								$"для платежа по заказу №{orderNumber}";

			var sendEmailMessage = new SendEmailMessage
			{
				From = new EmailContact
				{
					Name = _emailParametersProvider.DocumentEmailSenderName,
					Email = _emailParametersProvider.DocumentEmailSenderAddress
				},

				To = new List<EmailContact>
				{
					new EmailContact
					{
						Name = "Уважаемый пользователь",
						Email = _emailParametersProvider.InvalidSignatureNotificationEmailAddress
					}
				},

				Subject = $"Неккоректная подпись успешной оплаты заказа №{orderNumber}",

				TextPart = messageText,
				HTMLPart = messageText,
				Payload = new EmailPayload
				{
					Id = 0,
					Trackable = false
				}
			};

			var serializedMessage = JsonSerializer.Serialize(sendEmailMessage);
			var sendingBody = Encoding.UTF8.GetBytes(serializedMessage);

			var Logger = new Logger<RabbitMQConnectionFactory>(new NLogLoggerFactory());

			var connectionFactory = new RabbitMQConnectionFactory(Logger);
			var connection = connectionFactory.CreateConnection(
				configuration.MessageBrokerHost,
				configuration.MessageBrokerUsername,
				configuration.MessageBrokerPassword,
				configuration.MessageBrokerVirtualHost);
			var channel = connection.CreateModel();

			var properties = channel.CreateBasicProperties();
			properties.Persistent = true;

			channel.BasicPublish(configuration.EmailSendExchange, configuration.EmailSendKey, false, properties, sendingBody);
		}
	}
}
