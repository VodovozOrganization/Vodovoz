namespace VodovozInfrastructure.Cryptography
{
	public class RequestForCallSignatureParams : ShopIdSignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string PhoneNumber { get; set; }
	}
}
