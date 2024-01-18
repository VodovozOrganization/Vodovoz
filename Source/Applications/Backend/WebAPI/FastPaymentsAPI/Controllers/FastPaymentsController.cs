using System;
using System.Linq;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Requests;
using FastPaymentsAPI.Library.DTO_s.Responses;
using FastPaymentsAPI.Library.Managers;
using FastPaymentsAPI.Library.Models;
using FastPaymentsAPI.Library.Notifications;
using FastPaymentsAPI.Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FastPaymentsController : Controller
	{
		private readonly ILogger<FastPaymentsController> _logger;
		private readonly IFastPaymentOrderModel _fastPaymentOrderModel;
		private readonly IFastPaymentModel _fastPaymentModel;
		private readonly IDriverAPIService _driverApiService;
		private readonly IResponseCodeConverter _responseCodeConverter;
		private readonly IErrorHandler _errorHandler;
		private readonly SiteNotifier _siteNotifier;
		private readonly MobileAppNotifier _mobileAppNotifier;

		public FastPaymentsController(
			ILogger<FastPaymentsController> logger,
			IFastPaymentOrderModel fastPaymentOrderModel,
			IFastPaymentModel fastPaymentModel,
			IDriverAPIService driverApiService,
			IResponseCodeConverter responseCodeConverter,
			IErrorHandler errorHandler,
			SiteNotifier siteNotifier,
			MobileAppNotifier mobileAppNotifier)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastPaymentOrderModel = fastPaymentOrderModel ?? throw new ArgumentNullException(nameof(fastPaymentOrderModel));
			_fastPaymentModel = fastPaymentModel ?? throw new ArgumentNullException(nameof(fastPaymentModel));
			_driverApiService = driverApiService ?? throw new ArgumentNullException(nameof(driverApiService));
			_responseCodeConverter = responseCodeConverter ?? throw new ArgumentNullException(nameof(responseCodeConverter));
			_errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
			_siteNotifier = siteNotifier ?? throw new ArgumentNullException(nameof(siteNotifier));
			_mobileAppNotifier = mobileAppNotifier ?? throw new ArgumentNullException(nameof(mobileAppNotifier));
		}

		/// <summary>
		/// Эндпойнт для регистрации заказа и получения QR-кода для оплаты с мобильного приложения водителей
		/// </summary>
		/// <param name="orderDto">Dto с номером заказа</param>
		/// <returns></returns>
		[HttpPost("/api/RegisterOrderForGetQR")]
		public async Task<QRResponseDTO> RegisterOrderForGetQR([FromBody] OrderDTO orderDto)
		{
			var orderId = orderDto.OrderId;
			_logger.LogInformation($"Поступил запрос отправки QR-кода для заказа №{orderId}");
			
			var response = new QRResponseDTO();
			var paramsValidationResult = _fastPaymentOrderModel.ValidateParameters(orderId);
			
			if(paramsValidationResult != null)
			{
				response.ErrorMessage = paramsValidationResult;
				return response;
			}

			try
			{
				var fastPayments = _fastPaymentModel.GetAllPerformedOrProcessingFastPaymentsByOrder(orderId);

				var order = _fastPaymentOrderModel.GetOrder(orderId);
				var orderValidationResult = _fastPaymentOrderModel.ValidateOrder(order, orderId);

				if(orderValidationResult != null)
				{
					response.ErrorMessage = orderValidationResult;
					return response;
				}

				if(fastPayments.Any())
				{
					var fastPayment = fastPayments[0];
					var ticket = fastPayment.Ticket;

					if(fastPayment.FastPaymentStatus == FastPaymentStatus.Performed)
					{
						response.FastPaymentStatus = FastPaymentStatus.Performed;
						return response;
					}

					if(fastPayment.FastPaymentStatus == FastPaymentStatus.Processing)
					{
						_logger.LogInformation($"Делаем запрос в банк, чтобы узнать статус оплаты сессии {ticket}");
						var orderInfoResponseDto = await _fastPaymentOrderModel.GetOrderInfo(fastPayment.Ticket, fastPayment.Organization);
						
						if(orderInfoResponseDto.ResponseCode != 0)
						{
							return _errorHandler.LogAndReturnErrorMessageFromUpdateOrderInfo(
								response, orderInfoResponseDto, ticket, _logger);
						}
						if((int)orderInfoResponseDto.Status != (int)fastPayment.FastPaymentStatus)
						{
							_fastPaymentModel.UpdateFastPaymentStatus(
								fastPayment, orderInfoResponseDto.Status, orderInfoResponseDto.StatusDate);
						}

						if(orderInfoResponseDto.Status == FastPaymentDTOStatus.Performed)
						{
							response.FastPaymentStatus = FastPaymentStatus.Performed;
							return response;
						}
						if(orderInfoResponseDto.Status == FastPaymentDTOStatus.Processing)
						{
							if(!string.IsNullOrWhiteSpace(fastPayment.QRPngBase64)
								&& fastPayment.Amount == order.OrderSum)
							{
								response.QRCode = fastPayment.QRPngBase64;
								response.FastPaymentStatus = fastPayment.FastPaymentStatus;
								return response;
							}
							
							_logger.LogInformation($"Отменяем платеж с сессией {ticket}");
							_fastPaymentModel.UpdateFastPaymentStatus(fastPayment, FastPaymentDTOStatus.Rejected, DateTime.Now);
						}
					}
				}

				var fastPaymentGuid = Guid.NewGuid();
				var requestType = RequestFromType.FromDesktopOrDriverAppByQr;
				var organization = _fastPaymentModel.GetOrganization(requestType);
				OrderRegistrationResponseDTO orderRegistrationResponseDto = null;
				
				try
				{
					_logger.LogInformation("Регистрируем заказ в системе эквайринга");
					orderRegistrationResponseDto = await _fastPaymentOrderModel.RegisterOrder(order, fastPaymentGuid, organization);

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
				_fastPaymentModel.SaveNewTicketForOrder(
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
				_logger.LogError(e, $"При регистрации заказа {orderId} с получением QR-кода произошла ошибка");
			}
			
			return response;
		}

		[HttpGet("/api/RegisterOrder")]
		public async Task<FastPaymentResponseDTO> RegisterOrder(int orderId, string phoneNumber, bool isQr)
		{
			_logger.LogInformation($"Поступил запрос на отправку платежа с данными orderId: {orderId}, phoneNumber: {phoneNumber}");

			var response = new FastPaymentResponseDTO();
			var paramsValidationResult = _fastPaymentOrderModel.ValidateParameters(orderId, ref phoneNumber);
			
			if(paramsValidationResult != null)
			{
				response.ErrorMessage = paramsValidationResult;
				return response;
			}
			
			phoneNumber = $"+7{phoneNumber}";
			try
			{
				var fastPayments = _fastPaymentModel.GetAllPerformedOrProcessingFastPaymentsByOrder(orderId);

				if(fastPayments.Any())
				{
					var fastPayment = fastPayments[0];
					var ticket = fastPayment.Ticket;

					if(fastPayment.FastPaymentStatus == FastPaymentStatus.Performed)
					{
						response.FastPaymentStatus = FastPaymentStatus.Performed;
						return response;
					}

					if(fastPayment.FastPaymentStatus == FastPaymentStatus.Processing)
					{
						_logger.LogInformation($"Делаем запрос в банк, чтобы узнать статус оплаты сессии {ticket}");
						var orderInfoResponseDto = await _fastPaymentOrderModel.GetOrderInfo(ticket, fastPayment.Organization);

						if(orderInfoResponseDto.ResponseCode != 0)
						{
							return _errorHandler.LogAndReturnErrorMessageFromUpdateOrderInfo(
								response, orderInfoResponseDto, ticket, _logger);
						}
						if((int)orderInfoResponseDto.Status != (int)fastPayment.FastPaymentStatus)
						{
							_fastPaymentModel.UpdateFastPaymentStatus(
								fastPayment, orderInfoResponseDto.Status, orderInfoResponseDto.StatusDate);
						}
						if(orderInfoResponseDto.Status == FastPaymentDTOStatus.Performed)
						{
							response.FastPaymentStatus = FastPaymentStatus.Performed;
							return response;
						}
						if(orderInfoResponseDto.Status == FastPaymentDTOStatus.Processing)
						{
							_logger.LogInformation($"Отменяем платеж с сессией {ticket}");
							_fastPaymentModel.UpdateFastPaymentStatus(fastPayment, FastPaymentDTOStatus.Rejected, DateTime.Now);
						}
					}
				}
				
				var order = _fastPaymentOrderModel.GetOrder(orderId);
				var orderValidationResult = _fastPaymentOrderModel.ValidateOrder(order, orderId);
				
				if(orderValidationResult != null)
				{
					response.ErrorMessage = orderValidationResult;
					return response;
				}

				var fastPaymentGuid = Guid.NewGuid();
				var requestType = isQr ? RequestFromType.FromDesktopOrDriverAppByQr : RequestFromType.FromDesktopByCard;
				var organization = _fastPaymentModel.GetOrganization(requestType);
				OrderRegistrationResponseDTO orderRegistrationResponseDto = null;
				
				try
				{
					_logger.LogInformation("Регистрируем заказ в системе эквайринга");
					orderRegistrationResponseDto =
						await _fastPaymentOrderModel.RegisterOrder(order, fastPaymentGuid, organization, phoneNumber, isQr);

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
				_fastPaymentModel.SaveNewTicketForOrder(
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
				_logger.LogError(e, $"При регистрации заказа {orderId} произошла ошибка");
			}

			return response;
		}

		/// <summary>
		/// Эндпойнт для регистрации заказа в системе эквайринга и получения сессии оплаты для формирования ссылки на оплату для ДВ
		/// </summary>
		/// <param name="fastPaymentRequestDto">Dto с номерами заказа и телефона</param>
		/// <returns></returns>
		[HttpPost("/api/RegisterOrder")]
		public async Task<FastPaymentResponseDTO> RegisterOrder([FromBody] FastPaymentRequestDTO fastPaymentRequestDto) =>
			await RegisterOrder(fastPaymentRequestDto.OrderId, fastPaymentRequestDto.PhoneNumber, fastPaymentRequestDto.IsQr);
		
		/// <summary>
		/// Эндпойнт для регистрации онлайн-заказа с сайта и получения ссылки на платежную страницу
		/// </summary>
		/// <param name="requestRegisterOnlineOrderDto">Dto для регистрации онлайн-заказа</param>
		/// <returns></returns>
		[HttpPost("/api/RegisterOnlineOrder")]
		public async Task<ResponseRegisterOnlineOrderDTO> RegisterOnlineOrder(
			[FromBody] RequestRegisterOnlineOrderDTO requestRegisterOnlineOrderDto)
		{
			return await RegisterNewOnlineOrder(requestRegisterOnlineOrderDto, RequestFromType.FromSiteByQr);
		}
		
		/// <summary>
		/// Эндпойнт для регистрации онлайн-заказа мобильного приложения и получения ссылки на платежную страницу
		/// </summary>
		/// <param name="requestRegisterOnlineOrderDto">Dto для регистрации онлайн-заказа</param>
		/// <returns></returns>
		[HttpPost("/api/RegisterOnlineOrderFromMobileApp")]
		public async Task<ResponseRegisterOnlineOrderDTO> RegisterOnlineOrderFromMobileApp(
			[FromBody] RequestRegisterOnlineOrderDTO requestRegisterOnlineOrderDto)
		{
			return await RegisterNewOnlineOrder(requestRegisterOnlineOrderDto, RequestFromType.FromMobileAppByQr);
		}
		
		private async Task<ResponseRegisterOnlineOrderDTO> RegisterNewOnlineOrder(
			RequestRegisterOnlineOrderDTO requestRegisterOnlineOrderDto, RequestFromType requestType)
		{
			var onlineOrderId = requestRegisterOnlineOrderDto.OrderId;
			var onlineOrderSum = requestRegisterOnlineOrderDto.OrderSum;

			_logger.LogInformation($"Поступил запрос регистрации онлайн-заказа №{onlineOrderId}");

			var response = new ResponseRegisterOnlineOrderDTO();
			var paramsValidationResult =
				_fastPaymentOrderModel.ValidateParameters(
					onlineOrderId,
					requestRegisterOnlineOrderDto.BackUrl,
					requestRegisterOnlineOrderDto.BackUrlOk,
					requestRegisterOnlineOrderDto.BackUrlFail);

			if(paramsValidationResult != null)
			{
				response.ErrorMessage = paramsValidationResult;
				return response;
			}

			try
			{
				var fastPayments =
					_fastPaymentModel.GetAllPerformedOrProcessingFastPaymentsByOnlineOrder(onlineOrderId, onlineOrderSum);

				if(fastPayments.Any())
				{
					var fastPayment = fastPayments[0];
					var ticket = fastPayment.Ticket;

					if(fastPayment.FastPaymentStatus == FastPaymentStatus.Performed)
					{
						response.ErrorMessage = "Онлайн-заказ уже оплачен";
						return response;
					}

					if(fastPayment.FastPaymentStatus == FastPaymentStatus.Processing)
					{
						_logger.LogInformation($"Делаем запрос в банк, чтобы узнать статус оплаты сессии {ticket}");
						try
						{
							var orderInfoResponseDto = await _fastPaymentOrderModel.GetOrderInfo(ticket, fastPayment.Organization);

							if(orderInfoResponseDto.ResponseCode != 0)
							{
								return _errorHandler.LogAndReturnErrorMessageFromUpdateOrderInfo(
									response, orderInfoResponseDto, ticket, _logger);
							}

							if((int)orderInfoResponseDto.Status != (int)fastPayment.FastPaymentStatus)
							{
								_fastPaymentModel.UpdateFastPaymentStatus(
									fastPayment, orderInfoResponseDto.Status, orderInfoResponseDto.StatusDate);
							}

							if(orderInfoResponseDto.Status == FastPaymentDTOStatus.Performed)
							{
								response.ErrorMessage = "Онлайн-заказ уже оплачен";
								return response;
							}

							if(orderInfoResponseDto.Status == FastPaymentDTOStatus.Processing)
							{
								_logger.LogInformation($"Отменяем платеж с сессией {ticket}");
								_fastPaymentModel.UpdateFastPaymentStatus(fastPayment, FastPaymentDTOStatus.Rejected, DateTime.Now);

								await _siteNotifier.NotifyPaymentStatusChangeAsync(fastPayment);
								await _mobileAppNotifier.NotifyPaymentStatusChangeAsync(fastPayment);
							}
						}
						catch(Exception e)
						{
							response.ErrorMessage = "При получении информации об оплате из банка или обновлении статуса платежа произошла ошибка";
							_logger.LogError(
								e,
								$"При получении информации об оплате из банка {ticket} или обновлении статуса платежа произошла ошибка");
							return response;
						}
					}
				}

				var orderValidationResult = _fastPaymentOrderModel.ValidateOnlineOrder(onlineOrderSum);

				if(orderValidationResult != null)
				{
					response.ErrorMessage = orderValidationResult;
					return response;
				}

				var fastPaymentGuid = Guid.NewGuid();
				var organization = _fastPaymentModel.GetOrganization(requestType);
				OrderRegistrationResponseDTO orderRegistrationResponseDto = null;
				var callBackUrl = requestRegisterOnlineOrderDto.CallbackUrl;

				try
				{
					_logger.LogInformation($"Регистрируем онлайн-заказ {onlineOrderId} в системе эквайринга");
					orderRegistrationResponseDto = await _fastPaymentOrderModel.RegisterOnlineOrder(
						requestRegisterOnlineOrderDto, organization);

					if(orderRegistrationResponseDto.ResponseCode != 0)
					{
						return _errorHandler.LogAndReturnErrorMessageFromRegistrationOrder(
							response, orderRegistrationResponseDto, onlineOrderId, false, _logger);
					}
				}
				catch(Exception e)
				{
					var message = $"При регистрации онлайн-заказа {onlineOrderId} в системе эквайринга произошла ошибка";
					response.ErrorMessage = message;
					_logger.LogError(e, message);
					return response;
				}

				_logger.LogInformation($"Сохраняем новую сессию оплаты для онлайн-заказа №{onlineOrderId}");
				try
				{
					_fastPaymentModel.SaveNewTicketForOnlineOrder(
						orderRegistrationResponseDto, fastPaymentGuid, onlineOrderId, onlineOrderSum, FastPaymentPayType.ByQrCode,
						organization, requestType, callBackUrl);
				}
				catch(Exception e)
				{
					var message = $"При сохранении новой сессии оплаты для онлайн-заказа {onlineOrderId} произошла ошибка";
					response.ErrorMessage = message;
					_logger.LogError(e, message);
					return response;
				}

				response.PayUrl = _fastPaymentOrderModel.GetPayUrlForOnlineOrder(fastPaymentGuid);
			}
			catch(Exception e)
			{
				response.ErrorMessage = e.Message;
				_logger.LogError(e, $"При регистрации онлайн-заказа {onlineOrderId} произошла ошибка");
			}

			return response;
		}

		/// <summary>
		/// Эндпойнт получения инфы об оплаченном заказе
		/// </summary>
		/// <param name="paidOrderDto">Dto с информацией об оплате</param>
		/// <returns>При успешном выполнении отправляем 202</returns>
		[HttpPost("/api/ReceivePayment")]
		[Consumes("application/x-www-form-urlencoded")]
		public async Task<IActionResult> ReceivePayment([FromForm] PaidOrderDTO paidOrderDto)
		{
			_logger.LogInformation("Пришел ответ об успешной оплате");
			PaidOrderInfoDTO paidOrderInfoDto = null;

			try
			{
				_logger.LogInformation("Парсим и получаем объект PaidOrderInfoDTO из ответа");
				paidOrderInfoDto = _fastPaymentOrderModel.GetPaidOrderInfo(paidOrderDto.xml);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при парсинге ответа по успешной оплате");
				return StatusCode(500);
			}

			var ticket = paidOrderInfoDto.Ticket;
			var fastPayment = _fastPaymentModel.GetFastPaymentByTicket(ticket);

			if(fastPayment == null)
			{
				_logger.LogError($"Платеж с ticket: {ticket} не найден в базе");
				return StatusCode(500);
			}

			bool fastPaymentUpdated;
			try
			{
				_logger.LogInformation("Проверяем подпись");
				if(!_fastPaymentModel.ValidateSignature(paidOrderInfoDto, out var paymentSignature))
				{
					var bankSignature = paidOrderInfoDto.Signature;
					var orderNumber = paidOrderInfoDto.OrderNumber;
					var shopId = paidOrderInfoDto.ShopId;
					
					_logger.LogError($"Ответ по оплате заказа №{orderNumber} пришел с неверной подписью {bankSignature}" +
						$" по shopId {shopId}, рассчитанная подпись {paymentSignature}");

					try
					{
						_fastPaymentOrderModel.NotifyEmployee(orderNumber, bankSignature, shopId, paymentSignature);
					}
					catch(Exception e)
					{
						_logger.LogError(e, "Не удалось уведомить пользователя о неверной подписи оплаты");
					}

					return new BadRequestResult();
				}

				_logger.LogInformation($"Обновляем статус оплаты платежа с ticket: {ticket}");
				fastPaymentUpdated = _fastPaymentModel.UpdateFastPaymentStatus(paidOrderInfoDto, fastPayment);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					$"Ошибка при обработке поступившего платежа (ticket: {ticket}, status: {paidOrderInfoDto.Status})");
				return StatusCode(500);
			}

			NotifyDriver(fastPayment, paidOrderInfoDto.OrderNumber);
			
			if(fastPaymentUpdated)
			{
				await _siteNotifier.NotifyPaymentStatusChangeAsync(fastPayment);
				await _mobileAppNotifier.NotifyPaymentStatusChangeAsync(fastPayment);
			}
			
			return new AcceptedResult();
		}

		private void NotifyDriver(FastPayment fastPayment, string orderNumber)
		{
			if(fastPayment?.Order != null)
			{
				try
				{
					_logger.LogInformation($"Уведомляем водителя о изменении статуса оплаты заказа: {orderNumber}");
					_driverApiService.NotifyOfFastPaymentStatusChangedAsync(int.Parse(orderNumber));
				}
				catch(Exception e)
				{
					_logger.LogError(e, $"Не удалось уведомить службу DriverApi об изменении статуса оплаты заказа {orderNumber}");
				}
			}
		}


		/// <summary>
		/// Эндпойнт отмены сессии оплаты/платежа
		/// </summary>
		/// <param name="cancelTicketRequestDto">Dto с сессией, которую надо отменить</param>
		/// <returns></returns>
		[HttpPost("/api/CancelPayment")]
		public async Task<CancelTicketResponseDTO> CancelPayment([FromBody] CancelTicketRequestDTO cancelTicketRequestDto)
		{
			try
			{
				var ticket = cancelTicketRequestDto.Ticket;
				_logger.LogInformation($"Пришел запрос на отмену платежа с сессией: {ticket}");
				var fastPayment = _fastPaymentModel.GetFastPaymentByTicket(ticket);
				
				if(fastPayment == null)
				{
					_logger.LogError($"Платеж с сессией: {ticket} не найден в базе");
					return new CancelTicketResponseDTO("Не найден платеж в базе");
				}

				_logger.LogInformation($"Посылаем запрос в банк на отмену сессии оплаты: {ticket}");
				var cancelPaymentResponse = await _fastPaymentOrderModel.CancelPayment(ticket, fastPayment.Organization);

				if(cancelPaymentResponse.ResponseCode == 0)
				{
					_logger.LogInformation($"Обновляем статус платежа");
					_fastPaymentModel.UpdateFastPaymentStatus(fastPayment, FastPaymentDTOStatus.Rejected, DateTime.Now);
				}
				
				return new CancelTicketResponseDTO(_responseCodeConverter.ConvertToResponseStatus(cancelPaymentResponse.ResponseCode));
			}
			catch(Exception e)
			{
				_logger.LogError(e, $"При отмене сессии оплаты произошла ошибка");
				return new CancelTicketResponseDTO(e.Message);
			}
		}
		
		[HttpGet("/api/GetOrderInfo")]
		public async Task<OrderInfoResponseDTO> GetOrderInfo(string ticket)
		{
			_logger.LogInformation($"Пришел запрос на получении инфы о платеже с сессией: {ticket}");

			FastPayment fastPayment;
			try
			{
				fastPayment = _fastPaymentModel.GetFastPaymentByTicket(ticket);

				if(fastPayment == null)
				{
					_logger.LogError($"Платеж с сессией: {ticket} не найден в базе");
					return null;
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "При получении информации о быстром платеже из базы произошла ошибка");
				return null;
			}

			try
			{
				return await _fastPaymentOrderModel.GetOrderInfo(ticket, fastPayment.Organization);
			}
			catch(Exception e)
			{
				_logger.LogError(e, $"При получении информации об оплате произошла ошибка");
				return null;
			}
		}

		/// <summary>
		/// Для проверки работы сервиса
		/// </summary>
		/// <param name="orderId">Id заказа</param>
		/// <returns></returns>
		[HttpGet("/api/GetOrderId")]
		public OrderDTO GetOrderId(int orderId)
		{
			var order = _fastPaymentOrderModel.GetOrder(orderId);
			var orderDto = new OrderDTO
			{
				OrderId = order?.Id ?? -1
			};
			return orderDto;
		}
	}
}
