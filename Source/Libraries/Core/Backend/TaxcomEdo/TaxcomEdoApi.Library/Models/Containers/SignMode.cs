namespace TaxcomEdoApi.Library.Models.Containers
{
	/// <summary>
	/// Тип подписи документов
	/// </summary>
	public enum SignMode
	{
		/// <summary>
		/// Без подписи
		/// </summary>
		NotSign,
		/// <summary>
		/// Подпись, используя сертификат
		/// </summary>
		UseSpecifiedCertificate,
		/// <summary>
		/// Подпись, используя ЭЦП
		/// </summary>
		UseSpecifiedSignature
	}
}
