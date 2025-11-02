namespace Vodovoz.EntityRepositories.Store
{
	public class SelfDeliveryAddressDto
	{
		public int GeoGroupId { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		public string Address { get; set; }
	}
}
