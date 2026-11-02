using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V4.Services.Import
{
	/// <summary>
	/// Параметры подписи для проверки пакета выгрузки с сайта.
	/// </summary>
	public class SiteOrdersImportSignatureParams : SignatureParams
	{
		/// <summary>
		/// Дата формирования токена в формате "yyyy.MM.dd".
		/// </summary>
		[PositionForGenerateSignature(1)]
		public string Date { get; set; }
	}
}
