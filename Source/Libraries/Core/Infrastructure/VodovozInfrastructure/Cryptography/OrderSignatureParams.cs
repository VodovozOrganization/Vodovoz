namespace VodovozInfrastructure.Cryptography
{
	public class OrderSignatureParams : SignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OrderId { get; set; }
		[PositionForGenerateSignature(3)]
		public int OrderSumInKopecks { get; set; }
	}
}
