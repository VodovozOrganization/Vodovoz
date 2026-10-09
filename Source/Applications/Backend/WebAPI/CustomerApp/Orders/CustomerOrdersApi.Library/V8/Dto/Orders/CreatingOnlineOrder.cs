using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CustomerOrdersApi.Library.V8.Dto.Orders.OrderItem;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V8.Dto.Orders
{
	public class CreatingOnlineOrder : ICreatingOnlineOrder
	{
		public const string ExchangeAndQueueName = "creating-online-orders-v8";
		
		/// <inheritdoc/>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public Source Source { get; set; }
		
		/// <inheritdoc/>
		public Guid ExternalOrderId { get; set; }
		
		/// <inheritdoc/>
		public int? ErpCounterpartyId { get; set; }
		
		/// <inheritdoc/>
		public string Signature { get; set; }
		
		/// <inheritdoc/>
		public Guid? ExternalCounterpartyId { get; set; }
		
		/// <inheritdoc/>
		public int? DeliveryPointId { get; set; }

		/// <inheritdoc/>
		public bool IsSelfDelivery { get; set; }
		
		/// <inheritdoc/>
		public int? SelfDeliveryGeoGroupId { get; set; }
		
		/// <inheritdoc/>
		public OnlineOrderPaymentType OnlineOrderPaymentType { get; set; }
		
		/// <inheritdoc/>
		public OnlineOrderPaymentStatus OnlineOrderPaymentStatus { get; set; }

		/// <inheritdoc/>
		public int? OnlinePayment { get; set; }

		/// <inheritdoc/>
		public OnlinePaymentSource? OnlinePaymentSource { get; set; }

		/// <inheritdoc/>
		public bool IsNeedConfirmationByCall { get; set; }

		/// <inheritdoc/>
		public DateTime DeliveryDate { get; set; }

		/// <inheritdoc/>
		public int? DeliveryScheduleId { get; set; }
		
		/// <inheritdoc/>
		public int? CallBeforeArrivalMinutes { get; set; }
		
		/// <inheritdoc/>
		public bool IsFastDelivery { get; set; }

		/// <inheritdoc/>
		public string ContactPhone { get; set; }

		/// <inheritdoc/>
		public string OnlineOrderComment { get; set; }

		/// <inheritdoc/>
		public int? Trifle { get; set; }

		/// <inheritdoc/>
		public int? BottlesReturn { get; set; }
		
		/// <inheritdoc/>
		public decimal OrderSum { get; set; }
		
		/// <inheritdoc/>
		public bool DontArriveBeforeInterval { get; set; }
				
		/// <inheritdoc/>
		public bool IsAutoOrderEnabled { get; set; }

		/// <inheritdoc/>
		public IList<OnlineOrderItemDto> OnlineOrderItems { get; set; }
	}
}
