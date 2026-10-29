namespace VodovozInfrastructure.Cryptography
{
	public class OrderRatingSignatureParams : ShopIdSignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OrderNumber { get; set; }
		[PositionForGenerateSignature(3)]
		public int Rating { get; set; }
	}
}
