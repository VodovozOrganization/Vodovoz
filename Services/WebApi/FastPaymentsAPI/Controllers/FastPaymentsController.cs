using System;
using System.Linq;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Requests;
using FastPaymentsAPI.Library.DTO_s.Responses;
using FastPaymentsAPI.Library.Models;
using FastPaymentsAPI.Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

		public FastPaymentsController(
			ILogger<FastPaymentsController> logger,
			IFastPaymentOrderModel fastPaymentOrderModel,
			IFastPaymentModel fastPaymentModel,
			IDriverAPIService driverApiService,
			IResponseCodeConverter responseCodeConverter)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastPaymentOrderModel = fastPaymentOrderModel ?? throw new ArgumentNullException(nameof(fastPaymentOrderModel));
			_fastPaymentModel = fastPaymentModel ?? throw new ArgumentNullException(nameof(fastPaymentModel));
			_driverApiService = driverApiService ?? throw new ArgumentNullException(nameof(driverApiService));
			_responseCodeConverter = responseCodeConverter ?? throw new ArgumentNullException(nameof(responseCodeConverter));
		}

		/// <summary>
		/// Эндпойнт для регистрации заказа и получения QR-кода для оплаты
		/// </summary>
		/// <param name="orderDto">Dto с номером заказа</param>
		/// <returns></returns>
		[HttpPost]
		[Route("/api/RegisterOrderForGetQR")]
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

				if(fastPayments.Any())
				{
					var fastPayment = fastPayments[0];

					if(fastPayment.FastPaymentStatus == FastPaymentStatus.Performed)
					{
						response.FastPaymentStatus = FastPaymentStatus.Performed;
						return response;
					}

					if(fastPayment.FastPaymentStatus == FastPaymentStatus.Processing)
					{
						_logger.LogInformation($"Делаем запрос в банк, чтобы узнать статус оплаты сессии {fastPayment.Ticket}");
						var orderInfoResponseDto = await _fastPaymentOrderModel.GetOrderInfo(fastPayment.Ticket);
						
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
							response.QRCode = fastPayment.QRPngBase64;
							response.FastPaymentStatus = fastPayment.FastPaymentStatus;
							return response;
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
				_logger.LogInformation("Регистрируем заказ в системе эквайринга");
				var orderRegistrationResponseDto = await _fastPaymentOrderModel.RegisterOrder(order, fastPaymentGuid);
				
				_logger.LogInformation("Сохраняем новую сессию оплаты");
				_fastPaymentModel.SaveNewTicketForOrder(orderRegistrationResponseDto, orderId, fastPaymentGuid);

				response.QRCode = orderRegistrationResponseDto.QRPngBase64;
				response.FastPaymentStatus = FastPaymentStatus.Processing;
				return response;
			}
			catch(Exception e)
			{
				response.ErrorMessage = e.Message;
			}
			
			return response;
		}

		[HttpGet]
		[Route("/api/RegisterOrder")]
		public async Task<FastPaymentResponseDTO> RegisterOrder(int orderId, string phoneNumber)
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
						var orderInfoResponseDto = await _fastPaymentOrderModel.GetOrderInfo(ticket);

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
							_logger.LogInformation($"Посылаем запрос в банк на отмену сессии оплаты: {ticket}");
							var cancelPaymentResponse = await _fastPaymentOrderModel.CancelPayment(ticket);

							if(cancelPaymentResponse.ResponseCode == 0)
							{
								_logger.LogInformation("Отменяем платеж");
								_fastPaymentModel.UpdateFastPaymentStatus(fastPayment, FastPaymentDTOStatus.Rejected, DateTime.Now);
							}
							else
							{
								_logger.LogError($"Не удалось отменить сессию: {ticket} код: {cancelPaymentResponse.ResponseCode}");
								response.ErrorMessage = "Не удалось отменить сессию оплаты";
								return response;
							}
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
				_logger.LogInformation("Регистрируем заказ в системе эквайринга");
				var orderRegistrationResponseDto = await _fastPaymentOrderModel.RegisterOrder(order, fastPaymentGuid, phoneNumber);
								
				_logger.LogInformation("Сохраняем новую сессию оплаты");
				_fastPaymentModel.SaveNewTicketForOrder(orderRegistrationResponseDto, orderId, fastPaymentGuid, phoneNumber);

				response.Ticket = orderRegistrationResponseDto.Ticket;
				response.FastPaymentGuid = fastPaymentGuid;
				response.FastPaymentStatus = FastPaymentStatus.Processing;
			}
			catch(Exception e)
			{
				response.ErrorMessage = e.Message;
			}

			return response;
		}

		/// <summary>
		/// Эндпойнт для регистрации заказа в системе эквайринга и получения сессии оплаты
		/// </summary>
		/// <param name="fastPaymentRequestDto">Dto с номерами заказа и телефона</param>
		/// <returns></returns>
		[HttpPost]
		[Route("/api/RegisterOrder")]
		public async Task<FastPaymentResponseDTO> RegisterOrder([FromBody] FastPaymentRequestDTO fastPaymentRequestDto) =>
			await RegisterOrder(fastPaymentRequestDto.OrderId, fastPaymentRequestDto.PhoneNumber);
		
		/// <summary>
		/// Эндпойнт для регистрации онлайн-заказа и получения ссылки на платежную страницу
		/// </summary>
		/// <param name="requestRegisterOnlineOrderDto">Dto для регистрации онлайн-заказа</param>
		/// <returns></returns>
		[HttpPost]
		[Route("/api/RegisterOnlineOrder")]
		public async Task<ResponseRegisterOnlineOrderDTO> RegisterOnlineOrder(
			[FromBody] RequestRegisterOnlineOrderDTO requestRegisterOnlineOrderDto)
		{
			var onlineOrderId = requestRegisterOnlineOrderDto.OrderId;
			_logger.LogInformation($"Поступил запрос регистрации онлайн-заказа №{onlineOrderId}");
			
			var response = new ResponseRegisterOnlineOrderDTO();
			var paramsValidationResult = _fastPaymentOrderModel.ValidateParameters(onlineOrderId);
			
			if(paramsValidationResult != null)
			{
				response.ErrorMessage = paramsValidationResult;
				return response;
			}

			try
			{
				var fastPayments =
					_fastPaymentModel.GetAllPerformedOrProcessingFastPaymentsByOnlineOrder(
						onlineOrderId, requestRegisterOnlineOrderDto.OrderSum);

				if(fastPayments.Any())
				{
					var fastPayment = fastPayments[0];

					if(fastPayment.FastPaymentStatus == FastPaymentStatus.Performed)
					{
						response.ErrorMessage = "Онлайн-заказ уже оплачен";
						return response;
					}

					if(fastPayment.FastPaymentStatus == FastPaymentStatus.Processing)
					{
						_logger.LogInformation($"Делаем запрос в банк, чтобы узнать статус оплаты сессии {fastPayment.Ticket}");
						var orderInfoResponseDto = await _fastPaymentOrderModel.GetOrderInfo(fastPayment.Ticket);
						
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
							response.PayUrl = _fastPaymentOrderModel.GetPayUrlForOnlineOrder(fastPayment.FastPaymentGuid);
							return response;
						}
					}
				}
				
				var orderValidationResult = _fastPaymentOrderModel.ValidateOnlineOrder(requestRegisterOnlineOrderDto.OrderSum);
				
				if(orderValidationResult != null)
				{
					response.ErrorMessage = orderValidationResult;
					return response;
				}

				var fastPaymentGuid = Guid.NewGuid();
				_logger.LogInformation("Регистрируем онлайн-заказ в системе эквайринга");
				var orderRegistrationResponseDto = await _fastPaymentOrderModel.RegisterOnlineOrder(requestRegisterOnlineOrderDto);
				
				_logger.LogInformation("Сохраняем новую сессию оплаты для онлайн-заказа");
				_fastPaymentModel.SaveNewTicketForOnlineOrder(orderRegistrationResponseDto, fastPaymentGuid, onlineOrderId);

				response.PayUrl = _fastPaymentOrderModel.GetPayUrlForOnlineOrder(fastPaymentGuid);
				return response;
			}
			catch(Exception e)
			{
				response.ErrorMessage = e.Message;
			}
			
			return response;
		}
		
		/// <summary>
		/// Эндпойнт получения инфы об оплаченном заказе
		/// </summary>
		/// <param name="paidOrderDto">Dto с информацией об оплате</param>
		/// <returns>При успешном выполнении отправляем 202</returns>
		[HttpPost]
		[Route("/api/ReceivePayment")]
		[Consumes("application/x-www-form-urlencoded")]
		public IActionResult ReceivePayment([FromForm] PaidOrderDTO paidOrderDto)
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
			
			try
			{
				_logger.LogInformation("Проверяем подпись");
				if(!_fastPaymentModel.ValidateSignature(paidOrderInfoDto))
				{
					var signature = paidOrderInfoDto.Signature;
					var orderNumber = paidOrderInfoDto.OrderNumber;
					_logger.LogError($"Ответ по оплате заказа №{orderNumber} пришел с неверной подписью {signature}");
					
					try
					{
						_fastPaymentOrderModel.NotifyEmployee(orderNumber, signature);
					}
					catch(Exception e)
					{
						_logger.LogError(e, "Не удалось уведомить пользователя о неверной подписи оплаты");
					}

					return new BadRequestResult();
				}

				_logger.LogInformation($"Обновляем статус оплаты платежа с ticket: {paidOrderInfoDto.Ticket}");
				_fastPaymentModel.UpdateFastPaymentStatus(paidOrderInfoDto);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					$"Ошибка при обработке поступившего платежа (ticket: {paidOrderInfoDto.Ticket}, status: {paidOrderInfoDto.Status})");
				return StatusCode(500);
			}
			
			try
			{
				_logger.LogInformation($"Уведомляем водителя о изменении статуса оплаты заказа: {paidOrderInfoDto.OrderNumber}");
				_driverApiService.NotifyOfFastPaymentStatusChangedAsync(int.Parse(paidOrderInfoDto.OrderNumber));
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Не удалось уведомить службу DriverApi об изменении статуса оплаты заказа");
			}
			return new AcceptedResult();
		}

		/// <summary>
		/// Эндпойнт отмены сессии оплаты/платежа
		/// </summary>
		/// <param name="cancelTicketRequestDto">Dto с сессией, которую надо отменить</param>
		/// <returns></returns>
		[HttpPost]
		[Route("/api/CancelPayment")]
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
				var cancelPaymentResponse = await _fastPaymentOrderModel.CancelPayment(ticket);

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
		
		[HttpGet]
		[Route("/api/GetOrderInfo")]
		public async Task<OrderInfoResponseDTO> GetOrderInfo(string ticket)
		{
			try
			{
				return await _fastPaymentOrderModel.GetOrderInfo(ticket);
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
		[HttpGet]
		[Route("/api/GetOrderId")]
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
