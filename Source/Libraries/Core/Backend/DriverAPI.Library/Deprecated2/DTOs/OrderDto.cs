using System;
using System.Collections.Generic;
using SmsPaymentDtoStatus = DriverAPI.Library.DTOs.SmsPaymentDtoStatus;
using QRPaymentDTOStatus = DriverAPI.Library.DTOs.QRPaymentDTOStatus;
using PhoneDto = DriverAPI.Library.DTOs.PhoneDto;
using AddressDto = DriverAPI.Library.DTOs.AddressDto;
using OrderSaleItemDto = DriverAPI.Library.DTOs.OrderSaleItemDto;
using OrderDeliveryItemDto = DriverAPI.Library.DTOs.OrderDeliveryItemDto;
using OrderReceptionItemDto = DriverAPI.Library.DTOs.OrderReceptionItemDto;
using SignatureDtoType = DriverAPI.Library.DTOs.SignatureDtoType;

namespace DriverAPI.Library.Deprecated2.DTOs
{
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	public class OrderDto
	{
		public int OrderId { get; set; }
		public SmsPaymentDtoStatus? SmsPaymentStatus { get; set; }
		public QRPaymentDTOStatus? QRPaymentStatus { get; set; }
		public string DeliveryTime { get; set; }
		public int FullBottleCount { get; set; }
		public int EmptyBottlesToReturn { get; set; }
		public string Counterparty { get; set; }
		public IEnumerable<PhoneDto> PhoneNumbers { get; set; }
		public PaymentDtoType PaymentType { get; set; }
		public AddressDto Address { get; set; }
		public string OrderComment { get; set; }
		public decimal OrderSum { get; set; }
		public bool IsFastDelivery { get; set; }
		public string AddedToRouteListTime { get; set; }
		public IEnumerable<OrderSaleItemDto> OrderSaleItems { get; set; }
		public IEnumerable<OrderDeliveryItemDto> OrderDeliveryItems { get; set; }
		public IEnumerable<OrderReceptionItemDto> OrderReceptionItems { get; set; }
		public OrderAdditionalInfoDto OrderAdditionalInfo { get; set; }
		public int Trifle { get; set; }
		public SignatureDtoType? SignatureType { get; set; }
	}
}
