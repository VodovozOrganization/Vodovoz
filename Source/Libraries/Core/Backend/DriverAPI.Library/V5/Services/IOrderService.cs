using DriverApi.Contracts.V5;
using DriverApi.Contracts.V5.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors;

namespace DriverAPI.Library.V5.Services
{
	public interface IOrderService
	{
		Result<OrderDto> TryGetOrder(int orderId);
		IEnumerable<OrderDto> Get(int[] orderIds);
		void ChangeOrderPaymentType(int orderId, PaymentType paymentType, Employee driver, PaymentByTerminalSource? paymentByTerminalSource);
		IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(Order order);
		IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(int orderId);
		Task<Result<PayByQrResponse>> TrySendQrPaymentRequestAsync(int orderId, int driverId);
		void UpdateOrderShipmentInfo(DateTime actionTime, Employee driver, IDriverOrderShipmentInfo completeOrderInfo);
		Result<OrderAdditionalInfoDto> TryGetAdditionalInfo(int orderId);
		Result<OrderAdditionalInfoDto> TryGetAdditionalInfo(Order order);
		Result TryUpdateBottlesByStockActualCount(int orderId, int bottlesByStockActualCount);
		Result TryCompleteOrderDelivery(DateTime actionTime, Employee driver, IDriverOrderShipmentInfo completeOrderInfo, IDriverComplaintInfo driverComplaintInfo);
	}
}
