using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Extensions;
using TaxcomEdo.Contracts.Responses;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services
{
	public class EdoDocflowService : EdoLoginService, IEdoDocflowService
	{
		private readonly HttpClient _httpClient;

		public EdoDocflowService(
			HttpClient httpClient,
			IEdoAuthorizationService edoAuthorizationService
			) : base(edoAuthorizationService)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}
		
		/// <inheritdoc/>
		public async Task<TaxcomResponse<ContainerDescription>> GetMessageListAsync(
			GetMessageListParameters parameters, byte[] certificateData, CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(parameters.Date, "DATE")
				.AddParameter(parameters.DocflowDirection, "direction")
				.AddParameter(parameters.WithTracing, "withTracing")
				.AddParameter(parameters.WithGroupInfo, "withGroupInfo")
				.ToString();

			var assistantKey = await CertificateLoginAsync(certificateData, cancellationToken);
			var message = PrepareGetHttpRequestMessage(assistantKey, ExternalApiConstants.GetMessageListUri + query);
			var response = await _httpClient.SendAsync(message, cancellationToken);

			return await response.ToTaxcomResponseAsync<ContainerDescription>(cancellationToken);
		}
		
		/// <inheritdoc/>
		public async Task<TaxcomResponse<ContainerDescription>> GetListAsync(
			GetDocFlowsUpdatesParameters parameters, byte[] certificateData, CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(parameters.LastEventTimeStamp, "date")
				.AddParameter(parameters.DocFlowDirection, "direction")
				.AddParameter(parameters.DocFlowStatus, "status")
				.AddParameter(parameters.DepartmentId, "departmentId")
				.AddParameter(parameters.IncludeExtendedDocFlowStatuses, "includeExtendedDocflowStatuses")
				.AddParameter(parameters.IncludeTransportInfo, "includeTransportInfo")
				.ToString();

			var assistantKey = await CertificateLoginAsync(certificateData, cancellationToken);
			var message = PrepareGetHttpRequestMessage(assistantKey, ExternalApiConstants.GetListUri + query);
			var response = await _httpClient.SendAsync(message, cancellationToken);

			return await response.ToTaxcomResponseAsync<ContainerDescription>(cancellationToken);
		}
		
		/// <inheritdoc/>
		public async Task<TaxcomResponse<EdoDocFlowUpdates>> GetMessageAsync(
			string docFlowId, byte[] certificateData, CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(docFlowId, "ID")
				.ToString();

			var assistantKey = await CertificateLoginAsync(certificateData, cancellationToken);
			var message = PrepareGetHttpRequestMessage(assistantKey, ExternalApiConstants.GetMessageUri + query);
			var response = await _httpClient.SendAsync(message, cancellationToken);

			return await response.ToTaxcomResponseAsync<EdoDocFlowUpdates>(cancellationToken);
		}
		
		/// <inheritdoc/>
		public async Task<TaxcomResponse> SendMessageAsync(byte[] container, byte[] certificateData, CancellationToken cancellationToken = default)
		{
			var assistantKey = await CertificateLoginAsync(certificateData, cancellationToken);
			var content = PrepareByteArrayContent(container, assistantKey);
			var response = await _httpClient.PostAsync(ExternalApiConstants.SendMessageUri, content, cancellationToken);

			return response.ToTaxcomResponse();
		}
	}

	/// <summary>
	/// Параметры для запроса GetMessageList <see cref="IEdoDocflowService"/>
	/// </summary>
	[Serializable]
	public class GetMessageListParameters
	{
		public GetMessageListParameters() { }
		
		protected GetMessageListParameters(string date, string direction, bool withTracing, bool withGroupInfo)
		{
			Date = date;
			DocflowDirection = direction;
			WithTracing = withTracing;
			WithGroupInfo = withGroupInfo;
		}
		
		/// <summary>
		/// Обязательный параметр
		/// В качестве этого параметра указывается момент предыдущего вызова метода
		/// Формат параметра Date: yyyy-MM-ddTHH:mm:ss.ms
		/// </summary>
		public string Date { get; }
		/// <summary>
		/// Необязательный параметр
		/// Направление ДО
		/// </summary>
		public string DocflowDirection { get; }
		/// <summary>
		/// Необязательный параметр
		/// С прослеживаемостью
		/// </summary>
		public bool WithTracing { get; }
		/// <summary>
		/// Необязательный параметр
		/// При значении true в xml ответа в блок AdditionalData будет записан признак принадлежности к пакету следующего вида:
		/// AdditionalParameter с Name="GroupID" Value "[GuidGroup]", где [GuidGroup] - GUID группы, в которую входит транзакция
		/// </summary>
		public bool WithGroupInfo  { get; }

		public static GetMessageListParameters Create(
			DateTime date,
			DocflowDirection direction = Services.DocflowDirection.Ingoing,
			bool withTracing = false,
			bool withGroupInfo = false)
		{
			return new GetMessageListParameters($"{date:o}", direction.ToString(), withTracing, withGroupInfo);
		}
	}
	
	/// <summary>
	/// Направление документооборота
	/// </summary>
	public enum DocflowDirection
	{
		/// <summary>
		/// Входящий
		/// </summary>
		Ingoing,
		/// <summary>
		/// Исходящий
		/// </summary>
		Outgoing
	}
}
