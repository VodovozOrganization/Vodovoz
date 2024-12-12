namespace VodovozInfrastructure.Cryptography
{
	public class OrderRatingSignatureParams : SignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OrderNumber { get; set; }
		[PositionForGenerateSignature(3)]
		public int Rating { get; set; }
	}
}
