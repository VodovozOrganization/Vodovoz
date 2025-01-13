using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors;

namespace DriverAPI.Library.V6.Services
{
	public interface IOrderService
	{
		Result<OrderDto> GetOrder(int orderId);
		IEnumerable<OrderDto> Get(int[] orderIds);
		Result ChangeOrderPaymentType(int orderId, PaymentType paymentType, Employee driver, PaymentByTerminalSource? paymentByTerminalSource);
		IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(Order order);
		Result<IEnumerable<PaymentDtoType>> GetAvailableToChangePaymentTypes(int orderId);
		Task<Result<PayByQrResponse>> SendQrPaymentRequestAsync(int orderId, int driverId);
		Result UpdateOrderShipmentInfo(DateTime actionTime, Employee driver, IDriverOrderShipmentInfo completeOrderInfo);
		Result<OrderAdditionalInfoDto> GetAdditionalInfo(int orderId);
		Result<OrderAdditionalInfoDto> GetAdditionalInfo(Order order);
		Result UpdateBottlesByStockActualCount(int orderId, int bottlesByStockActualCount);
		Result CompleteOrderDelivery(DateTime actionTime, Employee driver, IDriverOrderShipmentInfo completeOrderInfo, IDriverComplaintInfo driverComplaintInfo);
	}
}
