using System;
using System.Collections.Generic;
using Vodovoz.Models.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class RoboatsOrderArgs
	{
		public int CounterpartyId { get; set; }
		public int DeliveryPointId { get; set; }
		public IEnumerable<RoboatsWaterInfo> WatersInfo { get; set; }
		public int BottlesReturn { get; set; }
		public DateTime Date { get; set; }
		public int DeliveryScheduleId { get; set; }
		public RoboAtsOrderPayment PaymentType { get; set; }
		public int? BanknoteForReturn { get; set; }
	}
}
