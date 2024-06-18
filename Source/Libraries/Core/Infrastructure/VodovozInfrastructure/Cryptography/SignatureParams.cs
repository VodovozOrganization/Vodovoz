namespace VodovozInfrastructure.Cryptography
{
	public class SignatureParams
	{
		public string OrderId { get; set; }
		public int OrderSumInKopecks { get; set; }
		public string Sign { get; set; }
		public long ShopId { get; set; }
	}
}
