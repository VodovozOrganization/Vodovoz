using System;
using Vodovoz.Domain.Logistic;
using System.Runtime.Serialization;
using Gamma.Utilities;

namespace Android
{
	[DataContract]
	public class RouteListDTO
	{
		[DataMember]
		public int Id;

		[DataMember]
		public string Status;

		[DataMember]
		public string Forwarder;

		[DataMember]
		public DateTime Date;

		[DataMember]
		public string DeliveryShift;

		public RouteListDTO (RouteList routeList)
		{
			Id = routeList.Id;
			Status = routeList.Status.GetEnumTitle();
			Forwarder = routeList.Forwarder?.FullName ?? String.Empty;
			Date = routeList.Date;
			DeliveryShift = routeList.Shift?.Name ?? String.Empty;
		}
	}
}

