namespace VodovozInfrastructure.Cryptography
{
	public abstract class ShopIdSignatureParams : SignatureParams
	{
		[PositionForGenerateSignature(1)]
		public long ShopId { get; set; }
	}
}
