namespace VodovozInfrastructure.Cryptography
{
	public class CounterpartyOrdersSignatureParams : ShopIdSignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string CounterpartyId { get; set; }
		[PositionForGenerateSignature(3)]
		public int Page { get; set; }
	}
}
