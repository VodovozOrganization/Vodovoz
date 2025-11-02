using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using TrueMark.Contracts.Responses;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Domain.TrueMark;

//using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Models.CashReceipts;
using Vodovoz.Models.TrueMark;

namespace VodovozBusiness.Models.TrueMark
{
	//public class Tag1260Checker : ITag1260Checker
	//{
	//	private readonly HttpClient _httpClient;
	//	private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
	//	private readonly ITrueMarkOrganizationClientSettingProvider _trueMarkOrganizationClientSettingProvider;

	//	private readonly JsonSerializerOptions _serializeOptions = new JsonSerializerOptions
	//	{
	//		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	//		NumberHandling = JsonNumberHandling.AllowReadingFromString,
	//		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	//		PropertyNameCaseInsensitive = true
	//	};

	//	public Tag1260Checker(
	//		IHttpClientFactory httpClientFactory,
	//		TrueMarkWaterCodeParser trueMarkWaterCodeParser,
	//		ITrueMarkOrganizationClientSettingProvider trueMarkOrganizationClientSettingProvide
	//	)
	//	{
	//		if(httpClientFactory is null)
	//		{
	//			throw new ArgumentNullException(nameof(httpClientFactory));
	//		}

	//		_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
	//		_trueMarkOrganizationClientSettingProvider = trueMarkOrganizationClientSettingProvide ??
	//		                                             throw new ArgumentNullException(nameof(trueMarkOrganizationClientSettingProvide));

	//		_httpClient = httpClientFactory.CreateClient(nameof(Tag1260Checker));
	//	}

	//	private void SetHttpClientHeaderApiKey(Guid headerApiKey)
	//	{
	//		_httpClient.DefaultRequestHeaders.Clear();
	//		_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
	//		_httpClient.DefaultRequestHeaders.Add("X-API-KEY", headerApiKey.ToString());
	//	}

	//	private async Task<string> GetCdnAsync(CancellationToken cancellationToken)
	//	{
	//		var uri = "https://cdn.crpt.ru/api/v4/true-api/cdn/info";

	//		var response = await _httpClient.GetAsync(uri);
	//		var responseBody = await response.Content.ReadAsStreamAsync();
	//		var result = await JsonSerializer.DeserializeAsync<CdnInfo>(responseBody, _serializeOptions, cancellationToken);

	//		var cdnHealths = new List<CdnHealth>();

	//		foreach(var cdnHost in result.Hosts)
	//		{
	//			var cdnHealth = await GetAvgTimeMsAsync(cdnHost.Host, cancellationToken);

	//			if(cdnHealth.Code == 0 && cdnHealth.AvgTimeMs < 1000)
	//			{
	//				return cdnHost.Host;
	//			}

	//			cdnHealth.Host = cdnHost.Host;

	//			cdnHealths.Add(cdnHealth);
	//		}

	//		return cdnHealths.Where(x => x.Code == 0)?
	//			.OrderBy(x => x.AvgTimeMs)?
	//			.FirstOrDefault()?.Host;
	//	}

	//	private async Task<CdnHealth> GetAvgTimeMsAsync(string cdnHost, CancellationToken cancellationToken)
	//	{
	//		var uri = $"{cdnHost}/api/v4/true-api/cdn/health/check";

	//		var response = await _httpClient.GetAsync(uri);
	//		var responseBody = await response.Content.ReadAsStreamAsync();
	//		var result = await JsonSerializer.DeserializeAsync<CdnHealth>(responseBody, _serializeOptions, cancellationToken);

	//		return result;
	//	}

	//	private void UpdateTag1260CodeCheckResult(IUnitOfWork unitOfWork, IEnumerable<TrueMarkWaterIdentificationCode> sourceCodes,
	//		CodeCheckResponse codeCheckResponse, Tag1260CodeCheckResult tag1260CodeCheckResult)
	//	{
	//		foreach(var sourceCode in sourceCodes)
	//		{
	//			var code = _trueMarkWaterCodeParser.GetProductCodeForTag1260(sourceCode);

	//			var codeCheckInfo = codeCheckResponse.Codes.FirstOrDefault(x => x.Cis.Equals(code));

	//			sourceCode.Tag1260CodeCheckResult = tag1260CodeCheckResult;

	//			unitOfWork.Save(sourceCode.Tag1260CodeCheckResult);

	//			sourceCode.IsTag1260Valid =
	//				codeCheckInfo.ErrorCode == 0 && codeCheckInfo.Found && codeCheckInfo.Valid && codeCheckInfo.Verified
	//				&& codeCheckInfo.ExpireDate > DateTime.Now && codeCheckInfo.Realizable && codeCheckInfo.Utilised
	//				&& !codeCheckInfo.IsBlocked && !codeCheckInfo.Sold;

	//			unitOfWork.Save(sourceCode);
	//		}
	//	}

	//	private async Task CheckAndUpdateAsync(IUnitOfWork unitOfWork, IEnumerable<TrueMarkWaterIdentificationCode> sourceCodes,
	//		Guid headerApiKey, CancellationToken cancellationToken)
	//	{
	//		SetHttpClientHeaderApiKey(headerApiKey);

	//		var cdn = await GetCdnAsync(cancellationToken);

	//		var uri = $"{cdn}/api/v4/true-api/codes/check";

	//		var codesCount = sourceCodes.Count();
	//		var toSkip = 0;

	//		while(codesCount > toSkip)
	//		{
	//			var sourceCodesToCheck = sourceCodes.Skip(toSkip).Take(100);

	//			toSkip += 100;

	//			var codesToRequest = new List<string>();

	//			foreach(var sourceCodeToCheck in sourceCodesToCheck)
	//			{
	//				var requetCode = _trueMarkWaterCodeParser.GetProductCodeForTag1260(sourceCodeToCheck);
	//				codesToRequest.Add(requetCode);
	//			}

	//			var request = new
	//			{
	//				codes = codesToRequest
	//			};

	//			var serializedRequest = JsonSerializer.Serialize(request);
	//			var content = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
	//			var response = await _httpClient.PostAsync(uri, content, cancellationToken);

	//			if(!response.IsSuccessStatusCode)
	//			{
	//				throw new Exception(
	//					$"Не удалось проверить коды для разрешительного режима 1260. Code: {response.StatusCode}. {response.ReasonPhrase}");
	//			}

	//			var responseBody = await response.Content.ReadAsStreamAsync();

	//			var result = await JsonSerializer.DeserializeAsync<CodeCheckResponse>(responseBody, _serializeOptions, cancellationToken);

	//			JsonSerializerOptions options = new JsonSerializerOptions
	//			{
	//				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	//				WriteIndented = true,
	//				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	//			};

	//			var jsonResponse = JsonSerializer.Serialize(result, options);

	//			var tag1260CodeCheckResult = new Tag1260CodeCheckResult
	//			{
	//				ReqId = result.ReqId,
	//				ReqTimestamp = result.ReqTimestamp,
	//			};

	//			UpdateTag1260CodeCheckResult(unitOfWork, sourceCodes, result, tag1260CodeCheckResult);

	//			if(toSkip < codesCount)
	//			{
	//				await Task.Delay(60000);
	//			}
	//		}
	//	}

	//	public async Task UpdateInfoForTag1260Async(CashReceipt cashReceipt, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
	//	{
	//		var sourceCodes = cashReceipt.ScannedCodes
	//			.Where(x => !x.IsDefectiveSourceCode && x.IsValid)
	//			.Select(x => x.SourceCode)
	//			.ToList();

	//		var organizationId = cashReceipt.Order.Contract?.Organization?.Id;

	//		if(organizationId == null)
	//		{
	//			return;
	//		}

	//		await UpdateInfoForTag1260Async(sourceCodes, unitOfWork, organizationId.Value, cancellationToken);
	//	}

	//	public async Task UpdateInfoForTag1260Async(IEnumerable<TrueMarkWaterIdentificationCode> sourceCodes, IUnitOfWork unitOfWork,
	//		int organizationId, CancellationToken cancellationToken)
	//	{
	//		if(!sourceCodes.Any())
	//		{
	//			return;
	//		}

	//		var trueMarkOrganizationClientSetting = _trueMarkOrganizationClientSettingProvider.GetModulKassaOrganizationSettings();

	//		var headerKey = trueMarkOrganizationClientSetting?
	//			.FirstOrDefault(x => x.OrganizationId == organizationId)?
	//			.HeaderTokenApiKey;

	//		if(headerKey == null)
	//		{
	//			throw new Exception("В настройках модуль кассы отсутствует headerApiKey");
	//		}

	//		await CheckAndUpdateAsync(unitOfWork, sourceCodes, headerKey.Value, cancellationToken);
	//	}
	//}
}
