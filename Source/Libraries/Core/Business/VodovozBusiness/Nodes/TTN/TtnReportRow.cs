namespace VodovozBusiness.Nodes.TTN
{
	public class TtnReportRow
	{
		public int Id { get; set; }
		public string NomenclatureName { get; set; }
		public string UnitName { get; set; }
		public decimal Count { get; set; }
		public decimal Price { get; set; }
		public decimal Sum { get; set; }
		public decimal Mass { get; set; }
		public int Places { get; set; }
	}
}
