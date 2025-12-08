namespace VodovozInfrastructure.Cryptography
{
	/// <summary>
	/// Данные для генерации подписи для применения фиксы
	/// </summary>
	public class ApplyFixedPriceSignatureParams : SignatureParams
	{
		/// <summary>
		/// Id заказа
		/// </summary>
		[PositionForGenerateSignature(2)]
		public string OrderId { get; set; }
		/// <summary>
		/// Сумма заказа в копейках
		/// </summary>
		[PositionForGenerateSignature(3)]
		public int OrderSumInKopecks { get; set; }
	}
}
