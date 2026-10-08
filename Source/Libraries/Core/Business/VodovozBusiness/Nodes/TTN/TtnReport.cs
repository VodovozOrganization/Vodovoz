using System.Collections.Generic;

namespace VodovozBusiness.Nodes.TTN
{
	public class TtnReport
	{
		// Шапка
		public string DocNumber { get; set; }
		public string DocDay { get; set; }
		public string DocMonth { get; set; }
		public string DocYear { get; set; }

		public string CargoSenderText { get; set; }
		public string CargoReceiverText { get; set; }
		public string PayerText { get; set; }

		// Товарный раздел
		public IList<TtnReportRow> Rows { get; set; } = new List<TtnReportRow>();

		public int GrandTotalCount { get; set; }
		public string GrandTotalCountText { get; set; }

		public decimal MassBruttoValue { get; set; }
		public string MassBruttoText { get; set; }

		public string ReleaseAllowedPosition { get; set; }
		public string ReleaseAllowedName { get; set; }

		public string ReleaseProducedPosition { get; set; }
		public string ReleaseProducedName { get; set; }

		public string CargoAcceptedPosition { get; set; }
		public string CargoAcceptedName { get; set; }

		// Транспортный раздел
		public string DeliveryDay { get; set; }
		public string DeliveryMonthText { get; set; }
		public string DeliveryYear { get; set; }

		public string OrganizationText { get; set; }

		public string CarModel { get; set; }
		public string CarRegistrationNumber { get; set; }

		public string DriverFullName { get; set; }
		public string DriverLicenseNumber { get; set; }

		public string LoadingPointAddress { get; set; }
		public string UnloadingPointAddress { get; set; }

		public string TrailerModel { get; set; }
		public string TrailerRegistrationNumber { get; set; }
	}
}
