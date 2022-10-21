namespace DriverAPI.Library.DTOs
{
	public class AddressDto
	{
		public string City { get; set; }
		public string Street { get; set; }
		public string Building { get; set; }
		public string Apartment { get; set; }
		public string Floor { get; set; }
		public string EntranceType { get; set; }
		public string RoomType { get; set; }
		public string DeliveryPointCategory { get; set; }
		public string Entrance { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
	}
}
