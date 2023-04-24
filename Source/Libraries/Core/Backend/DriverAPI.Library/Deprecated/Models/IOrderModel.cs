using DriverAPI.Library.DTOs;
using DriverAPI.Library.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace DriverAPI.Library.Deprecated.Models
{
	[Obsolete("Будет удален с прекращением поддержки API v1")]
	public interface IOrderModel
	{
		DTOs.OrderDto Get(int orderId);
		IEnumerable<DTOs.OrderDto> Get(int[] orderIds);
		OrderAdditionalInfoDto GetAdditionalInfo(int orderId);
		void ChangeOrderPaymentType(int orderId, PaymentType paymentType, Employee driver);
		IEnumerable<Deprecated.DTOs.PaymentDtoType> GetAvailableToChangePaymentTypes(Order order);
		IEnumerable<Deprecated.DTOs.PaymentDtoType> GetAvailableToChangePaymentTypes(int orderId);
		void CompleteOrderDelivery(DateTime actionTime, Employee driver, IDriverCompleteOrderInfo completeOrderInfo);
		void SendSmsPaymentRequest(int orderId, string phoneNumber, int driverId);
		Task<PayByQRResponseDTO> SendQRPaymentRequestAsync(int orderId, int driverId);
		void UpdateBottlesByStockActualCount(int orderId, int bottlesByStockActualCount);
	}
}
