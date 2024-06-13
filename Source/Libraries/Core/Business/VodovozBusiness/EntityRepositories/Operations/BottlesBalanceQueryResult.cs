namespace Vodovoz.EntityRepositories.Operations
{
	public class BottlesBalanceQueryResult
	{
		public int Delivered { get; set; }
		public int Returned { get; set; }
		public int BottlesDebt => Delivered - Returned;
	}
}
