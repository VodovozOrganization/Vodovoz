namespace VodovozInfrastructure.Cryptography
{
	public class OnlineOrderPaymentStatusUpdatedSignatureParams : SignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OnlineOrderId { get; set; }
		[PositionForGenerateSignature(3)]
		public int OnlinePayment { get; set; }
	}
}
