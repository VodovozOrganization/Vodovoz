namespace VodovozInfrastructure.Cryptography
{
	public class OrderInfoSignatureParams : ShopIdSignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OrderId { get; set; }
	}
}
