namespace Vodovoz.Core.Data.Clients
{
	public class DeliveryPoint
	{
		public int Id { get; set; }
		public int CounterpartyId { get; set; }
		public string ShortAddress { get; set; }
		public string KPP { get; set; }
	}
}
