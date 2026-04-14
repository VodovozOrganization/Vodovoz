using System;
using System.Collections.Generic;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using CryptoPro.Security.Cryptography.Pkcs;
using CryptoPro.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using TaxcomEdoApi.Library.Models;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Interfaces;
using TaxcomEdoApi.Library.Providers;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.SignProcessors
{
	/// <inheritdoc/>
	public class SpecifiedCertificateSignProcessor : ISignProcessor
	{
		private readonly ILogger<SpecifiedCertificateSignProcessor> _logger;
		private readonly ISignFilenameProvider _signFilenameProvider;
		private readonly ICertificateSearcher _certificateSearcher;

		public SpecifiedCertificateSignProcessor(
			ILogger<SpecifiedCertificateSignProcessor> logger,
			ISignFilenameProvider signFilenameProvider,
			ICertificateSearcher certificateSearcher)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_signFilenameProvider = signFilenameProvider ?? throw new ArgumentNullException(nameof(signFilenameProvider));
			_certificateSearcher = certificateSearcher ?? throw new ArgumentNullException(nameof(certificateSearcher));
		}
		
		/// <inheritdoc/>
		public IList<IFileData> Sign(IContainerDocument containerDocument, IDocument document)
		{
			var signatures = new List<IFileData>();
			
			foreach(var thumbprint in document.CertificatesForSign)
			{
				var fileData = FileData.Create(
					_signFilenameProvider.GetSignFilename(),
					_signFilenameProvider.GetFilePath(containerDocument),
					Sign(containerDocument.MainFile.Image, thumbprint));
				
				signatures.Add(fileData);
			}
			
			return signatures;
		}
		
		private byte[] Sign(byte[] content, string thumbprint)
		{
			if(content == null || content.Length == 0)
			{
				throw new ArgumentException("Не указано содержимое");
			}
			
			if(string.IsNullOrWhiteSpace(thumbprint))
			{
				throw new ArgumentException("Не указан отпечаток");
			}

			return SignContent(content, thumbprint);
		}
		
		private byte[] SignContent(byte[] content, string thumbprint)
		{
			/*var certificate = _certificateSearcher.SearchBy(StoreName.My, StoreLocation.CurrentUser, thumbprint)
				?? _certificateSearcher.SearchBy(StoreName.My, StoreLocation.LocalMachine, thumbprint);
			
			if(certificate is null)
			{
				throw new InvalidOperationException($"Не найден сертификат для подписи thumbprint: {thumbprint} ");
			}*/

			try
			{
				//TODO написать алгоритм подписи с помощью библиотек Крипто Про
				return CreateDetachedSignedCmsWithStore2012_256(content, thumbprint);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при подписи документа сертификатом {Thumbprint}", thumbprint);
				throw new InvalidOperationException("При расшифровке сообщения произошла ошибка. Проверьте, установлен ли CryptoAX", ex);
			}
		}
		
		public byte[] CreateDetachedSignedCmsWithStore2012_256(byte[] content, string thumbprint)
		{
			byte[] signature;

			var certificate = GetGost2012_256Certificate(thumbprint, StoreName.My, StoreLocation.CurrentUser)
				?? GetGost2012_256Certificate(thumbprint, StoreName.My, StoreLocation.LocalMachine);
			
			if(certificate is null)
			{
				throw new InvalidOperationException($"Не найден сертификат для подписи thumbprint: {thumbprint} ");
			}
			
			using(certificate)
			{
				var contentInfo = new ContentInfo(content);
				var signedCms = new CpSignedCms(contentInfo, true);
				var cmsSigner = new CpCmsSigner(certificate);

				// Опционально добавляем подписанные атрибуты.
				//cmsSigner.SignedAttributes.Add(new Pkcs9SigningTime(DateTime.Now));
				//cmsSigner.SignedAttributes.Add(new PkcsSigningCertificateV2(gostCert));

				// Вычисляем и кодируем подпись в массив байт.
				signedCms.ComputeSignature(cmsSigner);
				signature = signedCms.Encode();

				Console.WriteLine($"CMS Sign: {Convert.ToBase64String(signature)}");
				return signature;
			}

			// Создаем объект ContentInfo по сообщению.
			// Это необходимо для создания объекта SignedCms.
			ContentInfo contentInfoVerify = new ContentInfo(content);

			// Создаем SignedCms для декодирования и проверки.
			CpSignedCms signedCmsVerify = new CpSignedCms(contentInfoVerify, true);

			// Декодируем подпись
			signedCmsVerify.Decode(signature);

			// Проверяем подпись
			signedCmsVerify.CheckSignature(true);
		}
		
		private static CpX509Certificate2 GetGost2012_256Certificate(
			string thumbprint,
			StoreName storeName,
			StoreLocation storeLocation)
		{
			if(string.IsNullOrWhiteSpace(thumbprint))
			{
				throw new ArgumentNullException(nameof(thumbprint));
			}
			
			using var store = new CpX509Store(storeName, storeLocation);
			store.Open(OpenFlags.ReadOnly);
			
			var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);
				
			return certificates.Count switch
			{
				1 => certificates[0],
				> 1 => throw new InvalidOperationException($"Найдено больше одного сертификата с отпечатком {thumbprint}"),
				_ => null
			};
		}
	}
}
