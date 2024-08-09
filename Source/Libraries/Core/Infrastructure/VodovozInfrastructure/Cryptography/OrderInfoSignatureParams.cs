namespace VodovozInfrastructure.Cryptography
{
	public class OrderInfoSignatureParams : SignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OrderId { get; set; }
	}
}
