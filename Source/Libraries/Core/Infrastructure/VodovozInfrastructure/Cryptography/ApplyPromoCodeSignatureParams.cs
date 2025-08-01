namespace VodovozInfrastructure.Cryptography
{
	/// <summary>
	/// Данные для генерации подписи для применения промокода
	/// </summary>
	public class ApplyPromoCodeSignatureParams : SignatureParams
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
		/// <summary>
		/// Промокод
		/// </summary>
		[PositionForGenerateSignature(4)]
		public string PromoCode { get; set; }
	}
}
