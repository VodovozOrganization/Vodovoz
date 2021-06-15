using DriverAPI.Library.DTOs;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace DriverAPI.Library.DataAccess
{
	public interface IAPIOrderData
	{
		OrderDto Get(int orderId);
		IEnumerable<OrderDto> Get(int[] orderIds);
		OrderAdditionalInfoDto GetAdditionalInfo(int orderId);
		void ChangeOrderPaymentType(int orderId, PaymentType paymentType);
		IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(Order order);
		IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(int orderId);
		void CompleteOrderDelivery(Employee driver, int orderId, int bottlesReturnCount, int rating, int driverComplaintReasonId, string otherDriverComplaintReasonComment, DateTime actionTime);
	}
}