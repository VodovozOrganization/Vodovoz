namespace VodovozBusiness.Nodes
{
	public class DriverScheduleTotals
	{
		public DriverScheduleTotals(int totalBottles, int totalAddresses)
		{
			TotalBottles = totalBottles;
			TotalAddresses = totalAddresses;
		}

		public int TotalBottles { get; }

		public int TotalAddresses { get; }
	}
}
