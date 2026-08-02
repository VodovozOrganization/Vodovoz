namespace VodovozInfrastructure.Cryptography
{
	public class OrderSignatureParams : ShopIdSignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OrderId { get; set; }
		[PositionForGenerateSignature(3)]
		public int OrderSumInKopecks { get; set; }
	}
}
