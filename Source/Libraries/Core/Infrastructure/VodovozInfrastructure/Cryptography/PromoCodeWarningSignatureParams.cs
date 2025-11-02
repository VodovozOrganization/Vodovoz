namespace VodovozInfrastructure.Cryptography
{
	/// <summary>
	/// Данные для генерации подписи для предупреждающего сообщения при применении промокода
	/// </summary>
	public class PromoCodeWarningSignatureParams : SignatureParams
	{
		/// <summary>
		/// Id заказа
		/// </summary>
		[PositionForGenerateSignature(2)]
		public string OrderId { get; set; }
		/// <summary>
		/// Промокод
		/// </summary>
		[PositionForGenerateSignature(3)]
		public string PromoCode { get; set; }
	}
}
