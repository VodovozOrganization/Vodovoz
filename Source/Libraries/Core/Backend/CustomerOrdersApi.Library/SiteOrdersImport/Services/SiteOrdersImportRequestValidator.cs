using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.SiteOrdersImport.Dto;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.SiteOrdersImport.Services
{
	/// <summary>
	/// Проверяет структуру пакета и подпись запроса.
	/// </summary>
	public class SiteOrdersImportRequestValidator : SignatureService, ISiteOrdersImportRequestValidator
	{
		private const string _dateFormat = "yyyy.MM.dd";
		private static readonly HashSet<string> _availableEntityTypes = new(StringComparer.Ordinal)
		{
			"order",
			"abandoned_cart"
		};

		private readonly ISignatureManager _signatureManager;
		private readonly SignatureOptions _signatureOptions;

		/// <summary>
		/// Создаёт валидатор пакета выгрузки с сайта.
		/// </summary>
		public SiteOrdersImportRequestValidator(
			ISignatureManager signatureManager,
			IOptions<SignatureOptions> signatureOptions)
		{
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_signatureOptions = (signatureOptions ?? throw new ArgumentNullException(nameof(signatureOptions))).Value;
		}

		/// <summary>
		/// Проверяет подпись пакета на указанную дату.
		/// </summary>
		public bool ValidateSignature(OrdersImportRequest request, DateTime date, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(Source.VodovozWebSite, _signatureOptions);

			if(string.IsNullOrEmpty(sourceSign))
			{
				throw new InvalidOperationException(
					$"Не задана подпись {nameof(SignatureOptions.VodovozWebSite)} в секции \"{SignatureOptions.Path}\".");
			}

			return _signatureManager.Validate(
				request?.Token,
				new SiteOrdersImportSignatureParams
				{
					Sign = sourceSign,
					Date = date.ToString(_dateFormat, CultureInfo.InvariantCulture)
				},
				out generatedSignature);
		}

		/// <summary>
		/// Проверяет обязательные поля пакета.
		/// </summary>
		public Result Validate(OrdersImportRequest request)
		{
			if(string.IsNullOrWhiteSpace(request.Token))
			{
				return ValidationError("Не заполнен token");
			}

			if(string.IsNullOrWhiteSpace(request.BatchId))
			{
				return ValidationError("Не заполнен batch_id");
			}

			if(string.IsNullOrWhiteSpace(request.ContractVersion))
			{
				return ValidationError("Не заполнен contract_version");
			}

			if(request.Items is null || request.Items.Count == 0)
			{
				return ValidationError("Не заполнен items");
			}

			return Result.Success();
		}

		/// <summary>
		/// Проверяет обязательные поля записи пакета.
		/// </summary>
		public Result ValidateItem(OrderImportItem item)
		{
			if(item is null)
			{
				return ValidationError("Запись пакета выгрузки не заполнена.");
			}

			if(item.OrderId <= 0)
			{
				return ValidationError("В записи пакета выгрузки не заполнен order_id.");
			}

			if(string.IsNullOrWhiteSpace(item.EntityType) || !_availableEntityTypes.Contains(item.EntityType))
			{
				return ValidationError("В записи пакета выгрузки entity_type должен быть order или abandoned_cart.");
			}

			if(IsPayloadEmpty(item.Payload))
			{
				return ValidationError("В записи пакета выгрузки не заполнен payload.");
			}

			return Result.Success();
		}

		private static bool IsPayloadEmpty(JsonElement payload)
		{
			return payload.ValueKind switch
			{
				JsonValueKind.Undefined => true,
				JsonValueKind.Null => true,
				JsonValueKind.Object => !payload.EnumerateObject().Any(),
				JsonValueKind.Array => !payload.EnumerateArray().Any(),
				JsonValueKind.String => string.IsNullOrWhiteSpace(payload.GetString()),
				_ => false
			};
		}

		private static Result ValidationError(string message)
		{
			return Result.Failure(new Error("400", message));
		}
	}
}
