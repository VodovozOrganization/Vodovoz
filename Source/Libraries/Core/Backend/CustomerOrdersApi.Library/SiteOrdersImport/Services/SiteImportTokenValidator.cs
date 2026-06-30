using CustomerOrdersApi.Library.SiteOrdersImport.Config;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.SiteOrdersImport.Services
{
	/// <summary>
	/// Проверка токена приёма выгрузки с сайта по формуле контракта v1:
	/// <c>strtoupper(md5(strtoupper(md5(SOURCE_SIGN) . md5(date))))</c>, где date — "yyyy.MM.dd".
	/// </summary>
	public class SiteImportTokenValidator : ISiteImportTokenValidator
	{
		private const string _dateFormat = "yyyy.MM.dd";

		private readonly IMD5HexHashFromString _md5HexHashFromString;
		private readonly SiteOrdersImportOptions _options;

		public SiteImportTokenValidator(
			IMD5HexHashFromString md5HexHashFromString,
			IOptions<SiteOrdersImportOptions> options)
		{
			_md5HexHashFromString = md5HexHashFromString ?? throw new ArgumentNullException(nameof(md5HexHashFromString));
			_options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
		}

		public bool Validate(string token, DateTime date, out string expectedToken)
		{
			expectedToken = GenerateToken(date);

			return !string.IsNullOrEmpty(token)
				&& string.Equals(token, expectedToken, StringComparison.OrdinalIgnoreCase);
		}

		public string GenerateToken(DateTime date)
		{
			if(string.IsNullOrEmpty(_options.SourceSign))
			{
				throw new InvalidOperationException(
					$"Не задан секрет {nameof(SiteOrdersImportOptions.SourceSign)} в секции \"{SiteOrdersImportOptions.Path}\".");
			}

			var signHash = _md5HexHashFromString.GetMD5HexHashFromString(_options.SourceSign);
			var dateHash = _md5HexHashFromString.GetMD5HexHashFromString(date.ToString(_dateFormat, CultureInfo.InvariantCulture));

			var innerUpper = (signHash + dateHash).ToUpperInvariant();

			return _md5HexHashFromString.GetMD5HexHashFromString(innerUpper).ToUpperInvariant();
		}
	}
}
