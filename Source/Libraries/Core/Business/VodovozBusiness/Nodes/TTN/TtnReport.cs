using System.Collections.Generic;

namespace VodovozBusiness.Nodes.TTN
{
	public class TtnReport
	{
		public string SelectedFilters { get; set; }
		public string DocNumber { get; set; }
		public string DocDate { get; set; }
		public string OrganizationName { get; set; }
		public string OrganizationAddress { get; set; }
		public string OrganizationInn { get; set; }
		public string OrganizationKpp { get; set; }
		public string CargoSender { get; set; }
		public string CargoReceiver { get; set; }
		public string Payer { get; set; }
		public string DriverFullName { get; set; }
		public string VehicleNumber { get; set; }
		public int TotalPlaces { get; set; }
		public decimal MassNetto { get; set; }
		public decimal MassBrutto { get; set; }
		public IList<TtnReportRow> Rows { get; set; } = new List<TtnReportRow>();
	}
}
