using DriverAPI.Library.Deprecated2.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using IDriverCompleteOrderInfo = DriverAPI.Library.Models.IDriverCompleteOrderInfo;

namespace DriverAPI.Library.Deprecated2.Models
{
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	public interface IOrderModel
	{
		OrderDto Get(int orderId);
		IEnumerable<OrderDto> Get(int[] orderIds);
		OrderAdditionalInfoDto GetAdditionalInfo(int orderId);
		void ChangeOrderPaymentType(int orderId, PaymentType paymentType, Employee driver);
		IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(Order order);
		IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(int orderId);
		void CompleteOrderDelivery(DateTime actionTime, Employee driver, IDriverCompleteOrderInfo completeOrderInfo);
		void SendSmsPaymentRequest(int orderId, string phoneNumber, int driverId);
		Task<PayByQRResponseDTO> SendQRPaymentRequestAsync(int orderId, int driverId);
		void UpdateBottlesByStockActualCount(int orderId, int bottlesByStockActualCount);
	}
}
