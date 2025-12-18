using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Requests;
using FastPaymentsApi.Contracts.Responses;
using FastPaymentsApi.Contracts.Responses.OnlineOrderRegistration;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.Managers;
using FastPaymentsAPI.Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.FastPayments;
using ZXing;

namespace FastPaymentsAPI.Controllers
{
	[ApiController]
	[Route("api/[action]")]
	public class FastPaymentsController : Controller
	{
		private const string _serviceUnavailableError = "Сервис отключен, пользуйтесь другими видами оплат!";
		private readonly ILogger<FastPaymentsController> _logger;
		private readonly IFastPaymentOrderService _fastPaymentOrderService;
		private readonly IFastPaymentService _fastPaymentService;
		private readonly IResponseCodeConverter _responseCodeConverter;
		private readonly IErrorHandler _errorHandler;
		private readonly FastPaymentStatusManagerFromDesktop _fastPaymentStatusManagerFromDesktop;
		private readonly FastPaymentStatusManagerFromDriverApp _fastPaymentStatusManagerFromDriverApp;
		private readonly FastPaymentStatusManagerFromOnline _fastPaymentStatusManagerFromOnline;

		public FastPaymentsController(
			ILogger<FastPaymentsController> logger,
			IFastPaymentOrderService fastPaymentOrderService,
			IFastPaymentService fastPaymentService,
			IResponseCodeConverter responseCodeConverter,
			IErrorHandler errorHandler,
			FastPaymentStatusManagerFromDesktop fastPaymentStatusManagerFromDesktop,
			FastPaymentStatusManagerFromDriverApp fastPaymentStatusManagerFromDriverApp,
			FastPaymentStatusManagerFromOnline fastPaymentStatusManagerFromOnline)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastPaymentOrderService = fastPaymentOrderService ?? throw new ArgumentNullException(nameof(fastPaymentOrderService));
			_fastPaymentService = fastPaymentService ?? throw new ArgumentNullException(nameof(fastPaymentService));
			_responseCodeConverter = responseCodeConverter ?? throw new ArgumentNullException(nameof(responseCodeConverter));
			_errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
			_fastPaymentStatusManagerFromDesktop =
				fastPaymentStatusManagerFromDesktop ?? throw new ArgumentNullException(nameof(fastPaymentStatusManagerFromDesktop));
			_fastPaymentStatusManagerFromDriverApp =
				fastPaymentStatusManagerFromDriverApp ?? throw new ArgumentNullException(nameof(fastPaymentStatusManagerFromDriverApp));
			_fastPaymentStatusManagerFromOnline =
				fastPaymentStatusManagerFromOnline ?? throw new ArgumentNullException(nameof(fastPaymentStatusManagerFromOnline));
		}

		/// <summary>
		/// Эндпойнт для регистрации заказа и получения QR-кода для оплаты с мобильного приложения водителей
		/// </summary>
		/// <param name="orderDto">Dto с номером заказа</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<QRResponseDTO> RegisterOrderForGetQR([FromBody] OrderDTO orderDto)
		{
			var orderId = orderDto.OrderId;
			_logger.LogInformation($"Поступил запрос отправки QR-кода для заказа №{orderId}");
			
			var response = new QRResponseDTO();
			var paramsValidationResult = _fastPaymentOrderService.ValidateParameters(orderId);
			
			if(paramsValidationResult != null)
			{
				response.ErrorMessage = paramsValidationResult;
				return response;
			}

			try
			{
				var fastPayments = _fastPaymentService.GetAllPerformedOrProcessingFastPaymentsByOrder(orderId);

				var order = _fastPaymentOrderService.GetOrder(orderId);
				var orderValidationResult = _fastPaymentOrderService.ValidateOrder(order, orderId);

				if(orderValidationResult != null)
				{
					response.ErrorMessage = orderValidationResult;
					return response;
				}
				
				var statusCheck = await _fastPaymentStatusManagerFromDriverApp.CheckAllOrderFastPayments(fastPayments, response);

				if(statusCheck.NeedReturnResult)
				{
					return statusCheck.Result as QRResponseDTO;
				}

				var fastPaymentGuid = Guid.NewGuid();
				var requestType = FastPaymentRequestFromType.FromDriverAppByQr;
				var organizationResult = _fastPaymentService.GetOrganization(DateTime.Now.TimeOfDay, requestType, order);

				if(organizationResult.IsFailure)
				{
					var errorMessage = organizationResult.Errors.First().Message;
					_logger.LogWarning("Не удалось получить организацию для быстрого платежа по заказу {OrderId}: {ErrorMessage}",
						orderId,
						errorMessage);
					response.ErrorMessage = errorMessage;
					return response;
				}
				
				var organization = organizationResult.Value;
				OrderRegistrationResponseDTO orderRegistrationResponseDto = null;
				
				try
				{
					_logger.LogInformation("Регистрируем заказ в системе эквайринга");
					orderRegistrationResponseDto = await _fastPaymentOrderService.RegisterOrder(order, fastPaymentGuid, organization);

					if(orderRegistrationResponseDto.ResponseCode != 0)
					{
						return _errorHandler.LogAndReturnErrorMessageFromRegistrationOrder(
							response, orderRegistrationResponseDto, orderId, false, _logger);
					}
				}
				catch(Exception e)
				{
					var message = $"При регистрации заказа {orderId} для отправки QR-кода в системе эквайринга произошла ошибка";
					response.ErrorMessage = message;
					_logger.LogError(e, message);
					return response;
				}

				_logger.LogInformation("Сохраняем новую сессию оплаты");
				_fastPaymentService.SaveNewTicketForOrder(
					orderRegistrationResponseDto,
					orderId,
					fastPaymentGuid,
					FastPaymentPayType.ByQrCode,
					organization,
					requestType,
					PaymentType.DriverApplicationQR);

				response.QRCode = orderRegistrationResponseDto.QRPngBase64;
				response.FastPaymentStatus = FastPaymentStatus.Processing;
				return response;
			}
			catch(Exception e)
			{
				response.ErrorMessage = e.Message;
				_logger.LogError(e, "При регистрации заказа {OrderId} с получением QR-кода произошла ошибка", orderId);
			}
			
			return response;
		}

		/// <summary>
		/// Эндпойнт для регистрации заказа в системе эквайринга и получения сессии оплаты для формирования ссылки на оплату для ДВ
		/// </summary>
		/// <param name="fastPaymentRequestDto">Dto с номерами заказа и телефона</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<FastPaymentResponseDTO> RegisterOrder([FromBody] FastPaymentRequestDTO fastPaymentRequestDto)
		{
			var orderId = fastPaymentRequestDto.OrderId;
			var phoneNumber = fastPaymentRequestDto.PhoneNumber;
			var isQr = fastPaymentRequestDto.IsQr;
			
			_logger.LogInformation(
				"Поступил запрос на отправку платежа с данными orderId: {OrderId}, phoneNumber: {PhoneNumber}",
				orderId,
				phoneNumber);

			var response = new FastPaymentResponseDTO();
			var paramsValidationResult = _fastPaymentOrderService.ValidateParameters(orderId, ref phoneNumber);
			
			if(paramsValidationResult != null)
			{
				response.ErrorMessage = paramsValidationResult;
				return response;
			}
			
			phoneNumber = $"+7{phoneNumber}";
			try
			{
				var fastPayments = _fastPaymentService.GetAllPerformedOrProcessingFastPaymentsByOrder(orderId);
				
				var statusCheck = await _fastPaymentStatusManagerFromDesktop.CheckAllOrderFastPayments(fastPayments, response);

				if(statusCheck.NeedReturnResult)
				{
					return statusCheck.Result as FastPaymentResponseDTO;
				}
				
				var order = _fastPaymentOrderService.GetOrder(orderId);
				var orderValidationResult = _fastPaymentOrderService.ValidateOrder(order, orderId);
				
				if(orderValidationResult != null)
				{
					response.ErrorMessage = orderValidationResult;
					return response;
				}

				var fastPaymentGuid = Guid.NewGuid();
				var requestType = isQr ? FastPaymentRequestFromType.FromDesktopByQr : FastPaymentRequestFromType.FromDesktopByCard;
				var organizationResult = _fastPaymentService.GetOrganization(DateTime.Now.TimeOfDay, requestType, order);

				if(organizationResult.IsFailure)
				{
					var errorMessage = organizationResult.Errors.First().Message;
					_logger.LogWarning("Не удалось получить организацию для быстрого платежа по заказу {OrderId}: {ErrorMessage}",
						orderId,
						errorMessage);
					response.ErrorMessage = errorMessage;
					return response;
				}
				
				var organization = organizationResult.Value;
				OrderRegistrationResponseDTO orderRegistrationResponseDto = null;
				
				try
				{
					_logger.LogInformation("Регистрируем заказ в системе эквайринга");
					orderRegistrationResponseDto =
						await _fastPaymentOrderService.RegisterOrder(order, fastPaymentGuid, organization, phoneNumber, isQr);

					if(orderRegistrationResponseDto.ResponseCode != 0)
					{
						return _errorHandler.LogAndReturnErrorMessageFromRegistrationOrder(
							response, orderRegistrationResponseDto, orderId, false, _logger);
					}
				}
				catch(Exception e)
				{
					var message = $"При регистрации заказа {orderId} в системе эквайринга произошла ошибка";
					response.ErrorMessage = message;
					_logger.LogError(e, message);
					return response;
				}

				_logger.LogInformation("Сохраняем новую сессию оплаты");
				var payType = isQr ? FastPaymentPayType.ByQrCode : FastPaymentPayType.ByCard;
				var paymentType = isQr ? PaymentType.SmsQR : PaymentType.PaidOnline;
				_fastPaymentService.SaveNewTicketForOrder(
					orderRegistrationResponseDto,
					orderId,
					fastPaymentGuid,
					payType,
					organization,
					requestType,
					paymentType,
					phoneNumber);

				response.Ticket = orderRegistrationResponseDto.Ticket;
				response.FastPaymentGuid = fastPaymentGuid;
				response.FastPaymentStatus = FastPaymentStatus.Processing;
			}
			catch(Exception e)
			{
				response.ErrorMessage = e.Message;
				_logger.LogError(e, "При регистрации заказа {OrderId} произошла ошибка", orderId);
			}

			return response;
		}

		/// <summary>
		/// Эндпойнт для регистрации онлайн-заказа с сайта и получения ссылки на платежную страницу
		/// </summary>
		/// <param name="requestRegisterOnlineOrderDto">Dto для регистрации онлайн-заказа</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<ResponseRegisterOnlineOrder> RegisterOnlineOrder(
			[FromBody] RequestRegisterOnlineOrderDTO requestRegisterOnlineOrderDto)
		{
			return await RegisterNewOnlineOrder(requestRegisterOnlineOrderDto, FastPaymentRequestFromType.FromSiteByQr);
		}
		
		/// <summary>
		/// Эндпойнт для регистрации онлайн-заказа мобильного приложения и получения QR в виде base64 строки
		/// </summary>
		/// <param name="requestRegisterOnlineOrderDto">Dto для регистрации онлайн-заказа</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<ResponseRegisterOnlineOrder> RegisterOnlineOrderFromMobileApp(
			[FromBody] RequestRegisterOnlineOrderDTO requestRegisterOnlineOrderDto)
		{
			return await RegisterNewOnlineOrder(requestRegisterOnlineOrderDto, FastPaymentRequestFromType.FromMobileAppByQr);
		}
		
		/// <summary>
		/// Эндпойнт для регистрации онлайн-заказа с ИИ Бота
		/// </summary>
		/// <param name="requestRegisterOnlineOrderDto">Dto для регистрации онлайн-заказа</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<ResponseRegisterOnlineOrder> RegisterOnlineOrderFromAiBot(
			[FromBody] RequestRegisterOnlineOrderDTO requestRegisterOnlineOrderDto)
		{
			return await RegisterNewOnlineOrder(requestRegisterOnlineOrderDto, FastPaymentRequestFromType.FromAiBotByQr);
		}
		
		private async Task<ResponseRegisterOnlineOrder> RegisterNewOnlineOrder(
			RequestRegisterOnlineOrderDTO requestRegisterOnlineOrderDto, FastPaymentRequestFromType fastPaymentRequestType)
		{
			var onlineOrderId = requestRegisterOnlineOrderDto.OrderId;
			var onlineOrderSum = requestRegisterOnlineOrderDto.OrderSum;

			_logger.LogInformation("Поступил запрос регистрации онлайн-заказа №{OnlineOrderId}", onlineOrderId);

			var response = new ResponseRegisterOnlineOrder();
			var paramsValidationResult =
				_fastPaymentOrderService.ValidateParameters(requestRegisterOnlineOrderDto, fastPaymentRequestType);

			if(!string.IsNullOrWhiteSpace(paramsValidationResult))
			{
				response.ErrorMessage = paramsValidationResult;
				return response;
			}

			try
			{
				var fastPayments =
					_fastPaymentService.GetAllPerformedOrProcessingFastPaymentsByOnlineOrder(onlineOrderId, onlineOrderSum);
				
				try
				{
					var statusCheck =
						await _fastPaymentStatusManagerFromOnline.CheckAllOrderFastPayments(fastPayments, response);
					
					if(statusCheck.NeedReturnResult)
					{
						return statusCheck.Result as ResponseRegisterOnlineOrder;
					}
				}
				catch(Exception e)
				{
					response.ErrorMessage = "При получении информации об оплате из банка или обновлении статуса платежа произошла ошибка";
					_logger.LogError(
						e,
						"При получении информации об оплате из банка {Ticket} или обновлении статуса платежа произошла ошибка",
						fastPayments.FirstOrDefault()?.Ticket);
					return response;
				}

				var orderValidationResult = _fastPaymentOrderService.ValidateOnlineOrder(onlineOrderSum);

				if(orderValidationResult != null)
				{
					response.ErrorMessage = orderValidationResult;
					return response;
				}

				var fastPaymentGuid = Guid.NewGuid();
				var organizationResult = _fastPaymentService.GetOrganization(DateTime.Now.TimeOfDay, fastPaymentRequestType);

				if(organizationResult.IsFailure)
				{
					var errorMessage = organizationResult.Errors.First().Message;
					_logger.LogWarning("Не удалось получить организацию для быстрого платежа онлайн-заказа {OnlineOrderId}: {ErrorMessage}",
						onlineOrderId,
						errorMessage);
					response.ErrorMessage = errorMessage;
					return response;
				}
				
				var organization = organizationResult.Value;
				OrderRegistrationResponseDTO orderRegistrationResponseDto = null;
				var callBackUrl = requestRegisterOnlineOrderDto.CallbackUrl;

				try
				{
					_logger.LogInformation("Регистрируем онлайн-заказ {OnlineOrderId} в системе эквайринга", onlineOrderId);
					orderRegistrationResponseDto = await _fastPaymentOrderService.RegisterOnlineOrder(
						requestRegisterOnlineOrderDto, organization, fastPaymentRequestType);

					if(orderRegistrationResponseDto.ResponseCode != 0)
					{
						return _errorHandler.LogAndReturnErrorMessageFromRegistrationOrder(
							response, orderRegistrationResponseDto, onlineOrderId, false, _logger);
					}
				}
				catch(Exception e)
				{
					var messageTemplate = "При регистрации онлайн-заказа {0} в системе эквайринга произошла ошибка";
					response.ErrorMessage = string.Format(messageTemplate, onlineOrderId);
					_logger.LogError(e, string.Format(messageTemplate, "{OnlineOrderId}"), onlineOrderId);
					return response;
				}

				_logger.LogInformation("Сохраняем новую сессию оплаты для онлайн-заказа №{OnlineOrderId}", onlineOrderId);
				try
				{
					_fastPaymentService.SaveNewTicketForOnlineOrder(
						orderRegistrationResponseDto, fastPaymentGuid, onlineOrderId, onlineOrderSum, FastPaymentPayType.ByQrCode,
						organization, fastPaymentRequestType, callBackUrl);
				}
				catch(Exception e)
				{
					var messageTemplate = "При сохранении новой сессии оплаты для онлайн-заказа {0} произошла ошибка";
					response.ErrorMessage = string.Format(messageTemplate, onlineOrderId);
					_logger.LogError(e, string.Format(messageTemplate, "{OnlineOrderId}"), onlineOrderId);
					return response;
				}

				FillOnlineResponseData(response, fastPaymentRequestType, orderRegistrationResponseDto.QRPngBase64, fastPaymentGuid);
				return response;
			}
			catch(Exception e)
			{
				response.ErrorMessage = e.Message;
				_logger.LogError(e, "При регистрации онлайн-заказа {OnlineOrderId} произошла ошибка", onlineOrderId);
			}

			return response;
		}

		/// <summary>
		/// Эндпойнт получения инфы об оплаченном заказе
		/// </summary>
		/// <param name="paidOrderDto">Dto с информацией об оплате</param>
		/// <returns>При успешном выполнении отправляем 202</returns>
		[HttpPost]
		[Consumes("application/x-www-form-urlencoded")]
		public async Task<IActionResult> ReceivePayment([FromForm] PaidOrderDTO paidOrderDto)
		{
			_logger.LogInformation("Пришел ответ об успешной оплате");
			PaidOrderInfoDTO paidOrderInfoDto = null;

			try
			{
				_logger.LogInformation("Парсим и получаем объект PaidOrderInfoDTO из ответа");
				paidOrderInfoDto = _fastPaymentOrderService.GetPaidOrderInfo(paidOrderDto.xml);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при парсинге ответа по успешной оплате");
				return Problem();
			}

			var ticket = paidOrderInfoDto.Ticket;
			var fastPayment = _fastPaymentService.GetFastPaymentByTicket(ticket);

			if(fastPayment == null)
			{
				_logger.LogError("Платеж с ticket: {Ticket} не найден в базе", ticket);
				return Problem();
			}

			try
			{
				_logger.LogInformation("Проверяем подпись");
				if(!_fastPaymentService.ValidateSignature(paidOrderInfoDto, out var paymentSignature))
				{
					var bankSignature = paidOrderInfoDto.Signature;
					var orderNumber = paidOrderInfoDto.OrderNumber;
					var shopId = paidOrderInfoDto.ShopId;
					
					_logger.LogError("Ответ по оплате заказа №{OrderNumber} пришел с неверной подписью {BankSignature}" +
						" по shopId {ShopId}, рассчитанная подпись {PaymentSignature}",
						orderNumber,
						bankSignature,
						shopId,
						paymentSignature);

					try
					{
						_fastPaymentService.CreateWrongFastPaymentEvent(orderNumber, bankSignature, (int)shopId, paymentSignature);
					}
					catch(Exception e)
					{
						_logger.LogError(e, "Не удалось уведомить пользователя о неверной подписи оплаты с сессией {Ticket}", ticket);
					}

					return Accepted();
				}

				_logger.LogInformation("Обновляем статус оплаты платежа с ticket: {Ticket}", ticket);
				_fastPaymentService.UpdateFastPaymentStatus(paidOrderInfoDto, fastPayment);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при обработке поступившего платежа (ticket: {Ticket}, status: {PaymentStatus})",
					ticket,
					paidOrderInfoDto.Status);
				
				return Problem();
			}

			return Accepted();
		}

		/// <summary>
		/// Эндпойнт отмены сессии оплаты/платежа
		/// </summary>
		/// <param name="cancelTicketRequestDto">Dto с сессией, которую надо отменить</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<CancelTicketResponseDTO> CancelPayment([FromBody] CancelTicketRequestDTO cancelTicketRequestDto)
		{
			var ticket = cancelTicketRequestDto.Ticket;

			try
			{
				_logger.LogInformation("Пришел запрос на отмену платежа с сессией: {Ticket}", ticket);
				var fastPayment = _fastPaymentService.GetFastPaymentByTicket(ticket);
				
				if(fastPayment == null)
				{
					_logger.LogError("Платеж с сессией: {Ticket} не найден в базе", ticket);
					return new CancelTicketResponseDTO("Не найден платеж в базе");
				}

				_logger.LogInformation("Посылаем запрос в банк на отмену сессии оплаты: {Ticket}", ticket);
				var cancelPaymentResponse = await _fastPaymentOrderService.CancelPayment(ticket, fastPayment.Organization);

				if(cancelPaymentResponse.ResponseCode == 0)
				{
					_logger.LogInformation("Обновляем статус платежа");
					_fastPaymentService.UpdateFastPaymentStatus(fastPayment, FastPaymentDTOStatus.Rejected, DateTime.Now);
				}
				
				return new CancelTicketResponseDTO(_responseCodeConverter.ConvertToResponseStatus(cancelPaymentResponse.ResponseCode));
			}
			catch(Exception e)
			{
				_logger.LogError(e, "При отмене сессии оплаты {Ticket} произошла ошибка", ticket);
				return new CancelTicketResponseDTO(e.Message);
			}
		}
		
		/// <summary>
		/// Получение информации об оплате
		/// </summary>
		/// <param name="ticket">Номер тикета/сессии платежа</param>
		/// <returns>Информации об оплате <see cref="OrderInfoResponseDTO"/></returns>
		[HttpGet]
		public async Task<OrderInfoResponseDTO> GetOrderInfo(string ticket)
		{
			_logger.LogInformation("Пришел запрос на получении инфы о платеже с сессией: {Ticket}", ticket);

			FastPayment fastPayment;
			try
			{
				fastPayment = _fastPaymentService.GetFastPaymentByTicket(ticket);

				if(fastPayment == null)
				{
					_logger.LogError("Платеж с сессией: {Ticket} не найден в базе", ticket);
					return null;
				}
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"При получении информации о быстром платеже с сессией {Ticket} из базы произошла ошибка",
					ticket);
				
				return null;
			}

			try
			{
				return await _fastPaymentOrderService.GetOrderInfo(ticket, fastPayment.Organization);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "При получении информации об оплате с сессией {Ticket} произошла ошибка", ticket);
				return null;
			}
		}

		/// <summary>
		/// Для проверки работы сервиса
		/// </summary>
		/// <param name="orderId">Id заказа</param>
		/// <returns></returns>
		[HttpGet]
		public OrderDTO GetOrderId(int orderId)
		{
			var order = _fastPaymentOrderService.GetOrder(orderId);
			var orderDto = new OrderDTO
			{
				OrderId = order?.Id ?? -1
			};
			return orderDto;
		}
		
		private void FillOnlineResponseData(
			ResponseRegisterOnlineOrder response,
			FastPaymentRequestFromType fastPaymentRequestFromType,
			string qrPngBase64,
			Guid fastPaymentGuid)
		{
			if(fastPaymentRequestFromType == FastPaymentRequestFromType.FromMobileAppByQr)
			{
				response.QrCode = qrPngBase64;
			}
			else if(fastPaymentRequestFromType == FastPaymentRequestFromType.FromAiBotByQr)
			{
				using var stream = new MemoryStream(Convert.FromBase64String(qrPngBase64));
				using var bitmap = new Bitmap(stream);
				var reader = new BarcodeReader();
				var result = reader.Decode(bitmap);
				
				response.QrCode = qrPngBase64;
				response.PayUrl = result.Text;
			}
			else
			{
				response.PayUrl = _fastPaymentOrderService.GetPayUrlForOnlineOrder(fastPaymentGuid);
			}
		}
	}
}
