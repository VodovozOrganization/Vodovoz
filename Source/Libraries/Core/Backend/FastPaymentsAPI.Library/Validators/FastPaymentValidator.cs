using System;
using System.Linq;
using System.Text;
using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Requests;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;

namespace FastPaymentsAPI.Library.Validators
{
	public class FastPaymentValidator : IFastPaymentValidator
	{
		private readonly ILogger<FastPaymentValidator> _logger;

		public FastPaymentValidator(ILogger<FastPaymentValidator> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public string Validate(int orderId, FastPaymentRequestFromType? requestFromType = null)
		{
			if(!requestFromType.HasValue)
			{
				if(orderId <= 0)
				{
					_logger.LogError("Запрос на отправку платежа пришёл с неверным значением номера заказа {OrderId}", orderId);
					return "Неверное значение номера заказа";
				}
			}
			else
			{
				switch(requestFromType)
				{
					case FastPaymentRequestFromType.FromSiteByQr:
						if(orderId < 100_000_000)
						{
							_logger.LogError("Запрос на отправку платежа пришёл с неверным значением номера заказа {OrderId}", orderId);
							return "Неверное значение номера заказа";
						}
						break;
					case FastPaymentRequestFromType.FromMobileAppByQr:
						if(orderId < 200_000_000)
						{
							_logger.LogError("Запрос на отправку платежа пришёл с неверным значением номера заказа {OrderId}", orderId);
							return "Неверное значение номера заказа";
						}
						break;
				}
			}

			return null;
		}
		
		public string Validate(RequestRegisterOnlineOrderDTO registerOnlineOrderDto, FastPaymentRequestFromType fastPaymentRequestFromType)
		{
			var result = new StringBuilder();
			
			var orderIdValidationResult = Validate(registerOnlineOrderDto.OrderId, fastPaymentRequestFromType);
			if(orderIdValidationResult != null)
			{
				result.AppendLine(orderIdValidationResult);
			}

			if(fastPaymentRequestFromType == FastPaymentRequestFromType.FromMobileAppByQr)
			{
				return result.ToString();
			}
			
			if(string.IsNullOrEmpty(registerOnlineOrderDto.BackUrl))
			{
				_logger.LogError(GetLogMessageForOnlineOrderNullOrEmptyParameter(nameof(registerOnlineOrderDto.BackUrl)));
				result.AppendLine(GetReturnMessageForOnlineOrderNullOrEmptyParameter(nameof(registerOnlineOrderDto.BackUrl)));
			}
			if(string.IsNullOrEmpty(registerOnlineOrderDto.BackUrlOk))
			{
				_logger.LogError(GetLogMessageForOnlineOrderNullOrEmptyParameter(nameof(registerOnlineOrderDto.BackUrlOk)));
				result.AppendLine(GetReturnMessageForOnlineOrderNullOrEmptyParameter(nameof(registerOnlineOrderDto.BackUrlOk)));
			}
			if(string.IsNullOrEmpty(registerOnlineOrderDto.BackUrlFail))
			{
				_logger.LogError(GetLogMessageForOnlineOrderNullOrEmptyParameter(nameof(registerOnlineOrderDto.BackUrlFail)));
				result.AppendLine(GetReturnMessageForOnlineOrderNullOrEmptyParameter(nameof(registerOnlineOrderDto.BackUrlFail)));
			}

			return result.ToString();
		}
		
		public string ValidateOnlineOrder(decimal onlineOrderSum)
		{
			if(onlineOrderSum < 10)
			{
				_logger.LogError("Запрос на отправку платежа пришёл с суммой заказа меньше 10 рублей");
				return "Нельзя отправить платеж на заказ, сумма которого меньше 10 рублей";
			}

			return null;
		}

		public string Validate(int orderId, ref string phoneNumber)
		{
			var orderIdValidationResult = Validate(orderId);
			if(orderIdValidationResult != null)
			{
				return orderIdValidationResult;
			}

			if(string.IsNullOrWhiteSpace(phoneNumber))
			{
				_logger.LogError("Запрос на отправку платежа пришёл с неверным значением номера телефона");
				return "Неверное значение номера телефона";
			}

			phoneNumber = phoneNumber.TrimStart('+').TrimStart('7').TrimStart('8');
			if(string.IsNullOrWhiteSpace(phoneNumber)
				|| phoneNumber.Length == 0
				|| phoneNumber.First() != '9'
				|| phoneNumber.Length != 10)
			{
				_logger.LogError("Запрос на отправку платежа пришёл с неверным форматом номера телефона");
				return "Неверный формат номера телефона";
			}

			return null;
		}

		public string Validate(Order order, int orderId)
		{
			if(order == null)
			{
				_logger.LogError($"Запрос на отправку платежа пришёл со значением номера заказа, не существующем в базе (Id: {orderId})");
				return $"Заказ с номером {orderId} не существует в базе";
			}

			if(!order.DeliveryDate.HasValue)
			{
				_logger.LogError("Запрос на отправку платежа пришёл без даты доставки");
				return "Нельзя отправить платеж на заказ, в котором не указана дата доставки";
			}

			if(order.OrderDepositItems.Any())
			{
				_logger.LogError("Запрос на отправку платежа пришёл с возвратами залогов");
				return "Нельзя отправить платеж на заказ, в котором есть возврат залогов";
			}

			if(!order.OrderItems.Any())
			{
				_logger.LogError("Запрос на отправку платежа пришёл без товаров на продажу");
				return "Нельзя отправить платеж на заказ, в котором нет товаров на продажу";
			}

			if(order.OrderItems.Any(x => x.Price < 0))
			{
				_logger.LogError("Запрос на отправку платежа пришёл с товаром с ценой меньше 0");
				return "Нельзя отправить платеж на заказ, в котором есть товары с ценой меньше 0";
			}

			if(order.OrderSum < 10)
			{
				_logger.LogError("Запрос на отправку платежа пришёл с суммой заказа меньше 10 рублей");
				return "Нельзя отправить платеж на заказ, сумма которого меньше 10 рублей";
			}

			return null;
		}

		private static string GetLogMessageForOnlineOrderNullOrEmptyParameter(string parameterName)
		{
			return $"Запрос на отправку ссылки на оплату онлайн-заказа пришел с пустым значением {parameterName}";
		}
		
		private static string GetReturnMessageForOnlineOrderNullOrEmptyParameter(string parameterName)
		{
			return $"Null или пустое значение {parameterName}";
		}
	}
}
