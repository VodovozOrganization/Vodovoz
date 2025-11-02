using System;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public class OrderOnDayFilters
	{
		public DateTime DateForRouting { get; set; }
		public bool ShowCompleted { get; set; }
		public bool FastDeliveryEnabled { get; set; }
		public bool IsCodesScanInWarehouseRequired { get; set; }
		public IEnumerable<OrderAddressType> OrderAddressTypes { get; set; }
		public int ClosingDocumentDeliveryScheduleId { get; set; }
		public int[] GeographicGroupIds { get; set; }
		public DeliveryScheduleFilterType DeliveryScheduleType { get; set; }
		public TimeSpan DeliveryFromTime { get; set; }
		public TimeSpan DeliveryToTime { get; set; }
		public int MinBottles19L { get; set; }
		public int MaxBottles19L { get; set; }
	}
}
