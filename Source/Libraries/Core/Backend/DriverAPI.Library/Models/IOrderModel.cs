using DriverAPI.Library.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Models.TrueMark;

namespace DriverAPI.Library.Models
{
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
	}

	public interface IDriverCompleteOrderInfo
	{
		int OrderId { get; }
		int BottlesReturnCount { get; }
		int Rating { get; }
		int DriverComplaintReasonId { get; }
		string OtherDriverComplaintReasonComment { get; }
		string DriverComment { get; }
		IEnumerable<IOrderItemScannedInfo> ScannedItems { get; }
		string UnscannedCodesReason { get; }
	}

	public interface IOrderItemScannedInfo
	{
		IEnumerable<string> BottleCodes { get; set; }
		IEnumerable<string> DefectiveBottleCodes { get; set; }
		int OrderSaleItemId { get; set; }
	}
}
