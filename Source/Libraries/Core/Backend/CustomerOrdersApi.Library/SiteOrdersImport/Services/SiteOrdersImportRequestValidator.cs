using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.SiteOrdersImport.Dto;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using Vodovoz.Core.Domain.Clients;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.SiteOrdersImport.Services
{
	/// <summary>
	/// Проверяет структуру пакета и токен по формуле сайта:
	/// <c>strtoupper(md5(strtoupper(md5(SOURCE_SIGN)) . md5(date)))</c>, где date — "yyyy.MM.dd".
	/// </summary>
	public class SiteOrdersImportRequestValidator : SignatureService, ISiteOrdersImportRequestValidator
	{
		private const string _dateFormat = "yyyy.MM.dd";
		private const string _invalidToken = "Некорректный токен";

		private readonly IMD5HexHashFromString _md5HexHashFromString;
		private readonly SignatureOptions _signatureOptions;

		/// <summary>
		/// Создаёт валидатор пакета выгрузки с сайта.
		/// </summary>
		public SiteOrdersImportRequestValidator(
			IMD5HexHashFromString md5HexHashFromString,
			IOptions<SignatureOptions> signatureOptions)
		{
			_md5HexHashFromString = md5HexHashFromString ?? throw new ArgumentNullException(nameof(md5HexHashFromString));
			_signatureOptions = (signatureOptions ?? throw new ArgumentNullException(nameof(signatureOptions))).Value;
		}

		/// <summary>
		/// Проверяет обязательные поля пакета и токен авторизации на указанную дату.
		/// </summary>
		public SiteOrdersImportRequestValidationResult Validate(OrdersImportRequest request, DateTime date)
		{
			if(string.IsNullOrWhiteSpace(request.Token))
			{
				return SiteOrdersImportRequestValidationResult.ValidationError("Не заполнен token");
			}

			if(string.IsNullOrWhiteSpace(request.BatchId))
			{
				return SiteOrdersImportRequestValidationResult.ValidationError("Не заполнен batch_id");
			}

			if(string.IsNullOrWhiteSpace(request.ContractVersion))
			{
				return SiteOrdersImportRequestValidationResult.ValidationError("Не заполнен contract_version");
			}

			if(request.Items is null || request.Items.Count == 0)
			{
				return SiteOrdersImportRequestValidationResult.ValidationError("Не заполнен items");
			}

			return IsValidToken(request.Token, date)
				? SiteOrdersImportRequestValidationResult.Success()
				: SiteOrdersImportRequestValidationResult.Unauthorized(_invalidToken);
		}

		private bool IsValidToken(string token, DateTime date)
		{
			var expectedToken = GenerateToken(date);

			return string.Equals(token, expectedToken, StringComparison.OrdinalIgnoreCase);
		}

		private string GenerateToken(DateTime date)
		{
			var sourceSign = GetSourceSign(Source.VodovozWebSite, _signatureOptions);

			if(string.IsNullOrEmpty(sourceSign))
			{
				throw new InvalidOperationException(
					$"Не задана подпись {nameof(SignatureOptions.VodovozWebSite)} в секции \"{SignatureOptions.Path}\".");
			}

			var signHash = _md5HexHashFromString.GetMD5HexHashFromString(sourceSign);
			var dateHash = _md5HexHashFromString.GetMD5HexHashFromString(date.ToString(_dateFormat, CultureInfo.InvariantCulture));
			var tokenBase = signHash.ToUpperInvariant() + dateHash;

			return _md5HexHashFromString.GetMD5HexHashFromString(tokenBase).ToUpperInvariant();
		}
	}
}
