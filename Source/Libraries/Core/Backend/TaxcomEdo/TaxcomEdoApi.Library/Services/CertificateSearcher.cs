using System;
using System.Security.Cryptography.X509Certificates;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services
{
	/// <inheritdoc/>
	public class CertificateSearcher : ICertificateSearcher
	{
		/// <inheritdoc/>
		public X509Certificate2 SearchBy(
			StoreName storeName,
			StoreLocation storeLocation,
			string thumbprint)
		{
			if(string.IsNullOrWhiteSpace(thumbprint))
			{
				throw new ArgumentNullException(nameof(thumbprint));
			}

			thumbprint = thumbprint.Replace(" ", string.Empty);
			
			var x509Store = new X509Store(storeName, storeLocation);
			x509Store.Open(OpenFlags.ReadOnly);
			
			var x509Certificates = x509Store.Certificates
				.Find(X509FindType.FindByThumbprint, thumbprint, true);
			x509Store.Close();

			return x509Certificates.Capacity switch
			{
				1 => x509Certificates[0],
				> 1 => throw new InvalidOperationException($"Найдено больше оного сертификата с отпечатком {thumbprint}"),
				_ => null
			};
		}
	}
}
