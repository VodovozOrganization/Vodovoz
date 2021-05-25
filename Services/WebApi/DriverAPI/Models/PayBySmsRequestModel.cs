namespace DriverAPI.Models
{
	public class PayBySmsRequestModel
	{
		public int OrderId { get; set; }
		public string PhoneNumber { get; set; }
	}
}
