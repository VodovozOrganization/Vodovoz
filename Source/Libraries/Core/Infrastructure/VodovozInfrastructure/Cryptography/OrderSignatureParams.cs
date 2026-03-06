namespace VodovozInfrastructure.Cryptography
{
	public class OrderSignatureParams : SignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OrderId { get; set; }
		[PositionForGenerateSignature(3)]
		public int OrderSumInKopecks { get; set; }
	}

	/// <summary>
	/// Параметры подписи для переноса заказа
	/// </summary>
	public class TransferOrderSignatureParams : SignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OrderId { get; set; }

		[PositionForGenerateSignature(3)]
		public string DeliveryDate { get; set; }

		[PositionForGenerateSignature(4)]
		public int DeliveryScheduleId { get; set; }
	}

	/// <summary>
	/// Параметры подписи для отмены заказа
	/// </summary>
	public class CancelOrderSignatureParams : SignatureParams
	{
		[PositionForGenerateSignature(2)]
		public string OrderId { get; set; }
	}
}
