using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using Polly;
using EarchiveApi.Extensions;
using System.Linq;

namespace EarchiveApi.Services
{
	public class EarchiveUpdService : EarchiveUpd.EarchiveUpdBase
	{
		private readonly ILogger<EarchiveUpdService> _logger;
		private readonly string _connectionString;
		public EarchiveUpdService(ILogger<EarchiveUpdService> logger, IConfiguration configuration)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));

			var connectionStringBuilder = new MySqlConnectionStringBuilder();
			var domainDbConfig = configuration.GetSection("DomainDB");
			connectionStringBuilder.Server = domainDbConfig.GetValue<string>("Server");
			connectionStringBuilder.Port = domainDbConfig.GetValue<uint>("Port");
			connectionStringBuilder.Database = domainDbConfig.GetValue<string>("Database");
			connectionStringBuilder.UserID = domainDbConfig.GetValue<string>("UserID");
			connectionStringBuilder.Password = domainDbConfig.GetValue<string>("Password");
			connectionStringBuilder.SslMode = MySqlSslMode.Disabled;
			connectionStringBuilder.DefaultCommandTimeout = 5;

			_connectionString = connectionStringBuilder.GetConnectionString(true);
		}

		public override async Task GetCounterparites(NameSubstring request, IServerStreamWriter<CounterpartyInfo> responseStream, ServerCallContext context)
		{
			var minNameSubstringLength = 2;
			if(request is null || request.NamePart?.Length < minNameSubstringLength)
			{
				_logger.LogInformation($"Запрос поиска контрагента не выполнен. Получен либо пустой запрос, либо кол-во симоволов меньше {minNameSubstringLength}");
				return;
			}

			try
			{
				var nameSubstring = request.NamePart;

				var retryPolicy = Policy
					.Handle<MySqlException>()
					.Or<TimeoutException>()
					.WaitAndRetryAsync(1, (_) => TimeSpan.FromSeconds(1));

				using var connection = new MySqlConnection(_connectionString);

				var counterparties = (await connection.QueryAsyncWithRetry<CounterpartyInfo>(SelectCounterpartiesSqlQuery, retryPolicy, new { nameSubstring })).ToList();

				foreach(var counterpartyInfo in counterparties)
				{
					await responseStream.WriteAsync(counterpartyInfo);
				}

				_logger.LogInformation($"Запрос поиска контрагента выполнен успешно. По запросу \"{nameSubstring}\" найдено {counterparties.Count} результатов");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при выполнении запроса поиска контрагента. Запрос: {request.NamePart}.", ex.Message);
			}			
		}

		public override async Task GetAddresses(CounterpartyInfo request, IServerStreamWriter<DeliveryPointInfo> responseStream, ServerCallContext context)
		{
			if(request is null || request.Id < 1)
			{
				_logger.LogInformation($"Запрос поиска точки доставки не выполнен. Получен либо пустой запрос, значение переданного Id точки доставки меньше 1");
				return;
			}

			try
			{
				var counterpartyId = request.Id;

				var retryPolicy = Policy
					.Handle<MySqlException>()
					.Or<TimeoutException>()
					.WaitAndRetryAsync(1, (_) => TimeSpan.FromSeconds(1));

				using var connection = new MySqlConnection(_connectionString);

				var deliveryPoints = (await connection.QueryAsyncWithRetry<DeliveryPointInfo>(SelectAddressesSqlQuery, retryPolicy, new { counterpartyId })).ToList();

				foreach(var deliveryPointInfo in deliveryPoints)
				{
					await responseStream.WriteAsync(deliveryPointInfo);
				}
				_logger.LogInformation($"Запрос поиска точки доставки выполнен успешно. У контрагента id={counterpartyId} найдено {deliveryPoints.Count} адресов точек доставки");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при выполнении запроса поиска точек доставки контрагента. Поиск выполнялся для контрагента id={request.Id}.", ex.Message);
				return;
			}
		}

		public override async Task GetUpdCode(UpdRequestInfo request, IServerStreamWriter<UpdResponseInfo> responseStream, ServerCallContext context)
		{
			if(request is null || request.CounterpartyId < 1)
			{
				_logger.LogInformation($"Запрос поиска кода УПД не выполнен. Получен либо пустой запрос, либо значение переданного Id контрагента меньше 1");
				return;
			}

			try
			{
				var clientId = request.CounterpartyId;
				var deliveryPointId = request.DeliveryPointId > 0 ? request.DeliveryPointId : -1;
				var startDate = $"{request.StartDate.ToDateTime(): yyyy-MM-dd}";
				var endDate =	request.EndDate.ToDateTime().Year < 1971
								? $"{DateTime.Now:yyyy-MM-dd}"
								: $"{request.EndDate.ToDateTime():yyyy-MM-dd}";

				var retryPolicy = Policy
					.Handle<MySqlException>()
					.Or<TimeoutException>()
					.WaitAndRetryAsync(1, (_) => TimeSpan.FromSeconds(1));

				using var connection = new MySqlConnection(_connectionString);

				var updIds = (await connection.QueryAsyncWithRetry<UpdResponseInfo>(SelectUpdCodesSqlQuery, retryPolicy, new { clientId, deliveryPointId, startDate, endDate })).ToList();

				foreach(var updId in updIds)
				{
					await responseStream.WriteAsync(updId);
				}
				_logger.LogInformation($"Запрос поиска кода УПД выполнен успешно. По запросу: Id контрагента = {clientId}, Id точки доставки = {deliveryPointId}, дата начала = {startDate}, дата окончания = {endDate}, найдено {updIds.Count} кодов");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при выполнении поиска кода УПД. Параметры запроса: Id контрагента = {request.CounterpartyId}, Id точки доставки = {request.DeliveryPointId}, дата начала = {request.StartDate}, дата окончания = {request.EndDate}.", ex.Message);
				return;
			}
		}

		#region SQL queries
		private static string SelectCounterpartiesSqlQuery =>
			@"SELECT c.id as id, c.full_name as name
			FROM counterparty c
			WHERE c.full_name LIKE CONCAT('%', @nameSubstring ,'%')
			LIMIT 10";

		private static string SelectAddressesSqlQuery =>
			@"SELECT dp.id as id, dp.compiled_address_short  as address
			FROM delivery_points dp
			WHERE dp.counterparty_id = @counterpartyId";

		private static string SelectUpdCodesSqlQuery =>
			@"SELECT DISTINCT docs.doc_id as id
			FROM
				(SELECT
					orders.id AS doc_id,
					orders.delivery_date AS doc_date
				FROM
					orders
				WHERE
					NOT (orders.order_status = 'Canceled'
					OR orders.order_status = 'DeliveryCanceled'
					OR orders.order_status = 'NotDelivered')
					AND !orders.is_contract_closer
					AND orders.client_id = @clientId
					AND (@deliveryPointId = -1 OR orders.delivery_point_id = @deliveryPointId)
					AND (@startDate = '' OR orders.delivery_date >= @startDate)
					AND (@endDate = '' OR orders.delivery_date <= @endDate)

				UNION SELECT
					doc_residue.id AS doc_id,
					doc_residue.date AS doc_date
				FROM
					doc_residue
				WHERE
					doc_residue.client_id = @clientId
					AND (@deliveryPointId = -1 OR doc_residue.delivery_point_id = @deliveryPointId)
					AND (@startDate = '' OR doc_residue.date >= @startDate)
					AND (@endDate = '' OR doc_residue.date <= @endDate)
				GROUP BY(doc_residue.id)

				UNION SELECT
					transfer_operations.id AS doc_id,
					transfer_operations.operation_time AS doc_date
				FROM
					transfer_operations
				WHERE
					transfer_operations.from_client_id = @clientId
					AND (@deliveryPointId = -1 OR transfer_operations.from_delivery_point_id = @deliveryPointId)
					AND (@startDate = '' OR transfer_operations.operation_time >= @startDate)
					AND (@endDate = '' OR transfer_operations.operation_time <= @endDate)

				UNION SELECT
					transfer_operations.id AS transfer_operation_id,
					transfer_operations.operation_time AS doc_date
				FROM
					transfer_operations
				WHERE
					transfer_operations.to_client_id = @clientId
					AND (@deliveryPointId = -1 OR transfer_operations.to_delivery_point_id = @deliveryPointId)
					AND (@startDate = '' OR transfer_operations.operation_time >= @startDate)
					AND (@endDate = '' OR transfer_operations.operation_time <= @endDate)
				) AS docs
			ORDER BY docs.doc_date";
		#endregion
	}
}
