namespace VodovozInfrastructure.Cryptography
{
	public class RecomendationsSignatureParams : SignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OrderId { get; set; }
	}
}
