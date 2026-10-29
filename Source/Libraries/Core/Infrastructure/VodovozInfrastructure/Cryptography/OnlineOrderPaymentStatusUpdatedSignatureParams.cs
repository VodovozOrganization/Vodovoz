namespace VodovozInfrastructure.Cryptography
{
	public class OnlineOrderPaymentStatusUpdatedSignatureParams : ShopIdSignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OnlineOrderId { get; set; }
		[PositionForGenerateSignature(3)]
		public int OnlinePayment { get; set; }
	}
}
