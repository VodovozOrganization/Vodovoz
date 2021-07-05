namespace DriverAPI.DTOs
{
	public class PayBySmsRequestDto
	{
		public int OrderId { get; set; }
		public string PhoneNumber { get; set; }
	}
}
