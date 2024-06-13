namespace Vodovoz.Infrastructure.Persistance.Fuel
{
	public class FuelQueryResult
	{
		public decimal Gived { get; set; }
		public decimal Outlayed { get; set; }
		public decimal FuelBalance => Gived - Outlayed;
	}
}