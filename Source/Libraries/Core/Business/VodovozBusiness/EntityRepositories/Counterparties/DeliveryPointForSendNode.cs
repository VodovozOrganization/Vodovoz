namespace Vodovoz.EntityRepositories.Counterparties
{
	public class DeliveryPointForSendNode
	{
		public int Id { get; set; }
		public int CounterpartyId { get; set; }
		public string City { get; set; }
		public string LocalityType { get; set; }
		public string LocalityTypeShort { get; set; }
		public string Street { get; set; }
		public string StreetType { get; set; }
		public string StreetTypeShort { get; set; }
		public string Building { get; set; }
		public string Floor { get; set; }
		public string Entrance { get; set; }
		public string Room { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		public int CategoryId { get; set; }
		public string OnlineComment { get; set; }
		public string Intercom { get; set; }
	}
}
