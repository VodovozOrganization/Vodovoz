namespace VodovozInfrastructure.Cryptography
{
	public class OrderIdSignatureParams : SignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OrderId { get; set; }
	}
}
