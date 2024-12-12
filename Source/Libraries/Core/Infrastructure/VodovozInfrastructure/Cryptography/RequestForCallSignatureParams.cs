namespace VodovozInfrastructure.Cryptography
{
	public class RequestForCallSignatureParams : SignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string PhoneNumber { get; set; }
	}
}
