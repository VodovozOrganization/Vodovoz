namespace VodovozInfrastructure.Cryptography
{
	/// <summary>
	/// Параметры подписи запроса профиля клиента
	/// </summary>
	public sealed class ProfileInfoSignatureParams : ShopIdSignatureParams
	{
		/// <summary>
		/// Идентификатор контрагента ERP
		/// </summary>
		[PositionForGenerateSignature(2)]
		public string CounterpartyId { get; set; }
	}
}
