using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using DateTimeHelpers;
using Microsoft.Extensions.Options;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdoApi.Library.Config;

namespace TaxcomEdoApi.Library.Services
{
	public class EdoDocflowService : EdoLoginService, IEdoDocflowService
	{
		private readonly HttpClient _httpClient;
		private readonly IEdoAuthorizationService _edoAuthorizationService;
		private readonly TaxcomEdoApiOptions _options;

		public EdoDocflowService(
			HttpClient httpClient,
			IEdoAuthorizationService edoAuthorizationService,
			IOptions<TaxcomEdoApiOptions> options) : base(edoAuthorizationService)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_edoAuthorizationService = edoAuthorizationService ?? throw new ArgumentNullException(nameof(edoAuthorizationService));
			_options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
		}
		
		public async Task<EdoDocFlowUpdates> GetMessageListAsync(GetMessageListParameters parameters, byte[] certificateData, CancellationToken cancellationToken)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(parameters.Date, "DATE")
				.AddParameter(parameters.DocflowDirection, "direction")
				.AddParameter(parameters.WithTracing, "withTracing")
				.AddParameter(parameters.WithGroupInfo, "withGroupInfo")
				.ToString();
			
			var key = await CertificateLogin(certificateData);
			var response = await _httpClient.GetAsync(_options.DocflowUri.GetMessageListUri + query, cancellationToken);
			
			return response.IsSuccessStatusCode;
		}
		
		public async Task<EdoDocFlowUpdates> GetMessageAsync(string docFlowId, byte[] certificateData, CancellationToken cancellationToken)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(docFlowId, "ID")
				.ToString();
			
			var key = await CertificateLogin(certificateData);
			var response = await _httpClient.GetAsync(_options.DocflowUri.GetMessageUri + query, cancellationToken);
			
			return response.IsSuccessStatusCode;
		}
		
		public async Task<bool> SendMessageAsync(byte[] container, byte[] certificateData, CancellationToken cancellationToken)
		{
			var key = await CertificateLogin(certificateData);
			var response = await _httpClient.PostAsync(_options.DocflowUri.SendMessageUri, null, cancellationToken);
			
			return response.IsSuccessStatusCode;
		}
	}

	public interface IEdoDocflowService
	{
		/// <summary>
		/// Отправка контейнера Такском с электронным документом или служебным сообщением.
		/// Каждый отправляемый контейнер Такском должен содержать файлы meta.xml и card.xml, а также электронный документ или служебное сообщение
		/// </summary>
		/// <param name="container">Контйенер с документами</param>
		/// <param name="certificateData">Подпись</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns></returns>
		Task<bool> SendMessageAsync(byte[] container, byte[] certificateData, CancellationToken cancellationToken);
		/// <summary>
		/// Получение с сервера системы Такском-Доклайнз списка входящих или исходящих транзакций для всех получаемых и отправляемых электронных документов.
		/// </summary>
		/// <param name="parameters">Параметры фильтрации нужных данных</param>
		/// <param name="certificateData">Подпись</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns></returns>
		Task<EdoDocFlowUpdates> GetMessageListAsync(GetMessageListParameters parameters, byte[] certificateData, CancellationToken cancellationToken);
		/// <summary>
		/// Получение контейнера Такском с документом или служебным сообщением с сервера системы Такском-Доклайнз
		/// </summary>
		/// <param name="docFlowId">Id документооборота</param>
		/// <param name="certificateData">Подпись</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns></returns>
		Task<EdoDocFlowUpdates> GetMessageAsync(string docFlowId, byte[] certificateData, CancellationToken cancellationToken);
	}

	/// <summary>
	/// Параметры для запроса GetMessageList <see cref="IEdoDocflowService"/>
	/// </summary>
	public class GetMessageListParameters
	{
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
