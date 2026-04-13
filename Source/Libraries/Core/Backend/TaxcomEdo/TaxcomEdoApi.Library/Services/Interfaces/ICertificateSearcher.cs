using System.Security.Cryptography.X509Certificates;

namespace TaxcomEdoApi.Library.Services.Interfaces
{
	/// <summary>
	/// Поисковик сертификата
	/// </summary>
	public interface ICertificateSearcher
	{
		/// <summary>
		/// Поиск сертификата по отпечатку
		/// </summary>
		/// <param name="storeName">Хранилище</param>
		/// <param name="storeLocation">Расположение хранилища</param>
		/// <param name="thumbprint">Отпечаток</param>
		/// <returns></returns>
		X509Certificate2 SearchBy(
			StoreName storeName,
			StoreLocation storeLocation,
			string thumbprint);
	}
}
