using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Mindbox;

namespace Mindbox.CounterpartiesAdmin
{
	/// <inheritdoc />
	public sealed class MindboxAdminService : IMindboxAdminService
	{
		private static readonly UTF8Encoding _utf8WithBom = new(true);

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ILogger<MindboxAdminService> _logger;

		public MindboxAdminService(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILogger<MindboxAdminService> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public Task<int> ReportAsync(MindboxAdminOptions options, CancellationToken cancellationToken)
		{
			if(options is null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Отчет по статусам Mindbox"))
			{
				var rows = GetRowsByStatuses(unitOfWork, options.Statuses);

				if(!string.IsNullOrWhiteSpace(options.OutputFile))
				{
					WriteCsv(rows, options.OutputFile);
				}
				else
				{
					WriteConsole(rows);
				}

				_logger.LogInformation("Отчет Mindbox сформирован, записей: {Count}", rows.Count);

				return Task.FromResult(rows.Count);
			}
		}

		/// <inheritdoc />
		public Task<int> ResendAsync(MindboxAdminOptions options, CancellationToken cancellationToken)
		{
			if(options is null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Повторная отправка в Mindbox"))
			{
				var items = GetItemsForResend(unitOfWork, options);
				var resendCount = 0;

				foreach(var item in items)
				{
					var isNewState = item.State is null;
					var state = item.State ?? new MindboxCounterpartyState
					{
						CounterpartyId = item.Counterparty.Id,
						CreatedAt = DateTime.UtcNow
					};

					ResetForResend(state);

					if(isNewState)
					{
						// Id задаётся вручную (counterparty_id), поэтому нужен явный Insert.
						unitOfWork.Session.Save(state);
					}
					else
					{
						unitOfWork.Save(state);
					}

					resendCount++;
				}

				unitOfWork.Commit();

				_logger.LogInformation(
					"Поставлено на повторную отправку в Mindbox: {Count}. Отправка начнется после следующего прохода воркера.",
					resendCount);

				return Task.FromResult(resendCount);
			}
		}

		private static IList<ReportRow> GetRowsByStatuses(
			IUnitOfWork unitOfWork,
			IEnumerable<MindboxCounterpartyOutboxStatus> statuses)
		{
			var statusArray = statuses.Cast<object>().ToArray();

			if(statusArray.Length == 0)
			{
				return new List<ReportRow>();
			}

			var states = unitOfWork.Session.QueryOver<MindboxCounterpartyState>()
				.WhereRestrictionOn(state => state.Status)
				.IsIn(statusArray)
				.OrderBy(state => state.CounterpartyId)
				.Asc
				.List();

			return ToRows(unitOfWork, states);
		}		private static IList<CounterpartyWithState> GetItemsForResend(
			IUnitOfWork unitOfWork,
			MindboxAdminOptions options)
		{
			if(options.CounterpartyIds.Count > 0)
			{
				var ids = options.CounterpartyIds.Distinct().ToArray();

				var counterparties = unitOfWork.Session.QueryOver<CounterpartyEntity>()
					.WhereRestrictionOn(counterparty => counterparty.Id)
					.IsIn(ids)
					.List();

				var states = unitOfWork.Session.QueryOver<MindboxCounterpartyState>()
					.WhereRestrictionOn(state => state.CounterpartyId)
					.IsIn(ids)
					.List()
					.ToDictionary(state => state.CounterpartyId);

				return counterparties
					.Select(counterparty => new CounterpartyWithState
					{
						Counterparty = counterparty,
						State = states.TryGetValue(counterparty.Id, out var state) ? state : null
					})
					.ToList();
			}

			var statesByStatuses = unitOfWork.Session.QueryOver<MindboxCounterpartyState>()
				.WhereRestrictionOn(state => state.Status)
				.IsIn(options.Statuses.Cast<object>().ToArray())
				.List();

			var counterpartyIds = statesByStatuses
				.Select(state => state.CounterpartyId)
				.ToArray();

			if(counterpartyIds.Length == 0)
			{
				return new List<CounterpartyWithState>();
			}

			var counterpartiesById = unitOfWork.Session.QueryOver<CounterpartyEntity>()
				.WhereRestrictionOn(counterparty => counterparty.Id)
				.IsIn(counterpartyIds)
				.List()
				.ToDictionary(counterparty => counterparty.Id);

			return statesByStatuses
				.Where(state => counterpartiesById.ContainsKey(state.CounterpartyId))
				.Select(state => new CounterpartyWithState
				{
					Counterparty = counterpartiesById[state.CounterpartyId],
					State = state
				})
				.ToList();
		}

		private static IList<ReportRow> ToRows(
			IUnitOfWork unitOfWork,
			IList<MindboxCounterpartyState> states)
		{
			var counterpartyIds = states
				.Select(state => state.CounterpartyId)
				.ToArray();

			if(counterpartyIds.Length == 0)
			{
				return new List<ReportRow>();
			}

			var counterpartiesById = unitOfWork.Session.QueryOver<CounterpartyEntity>()
				.WhereRestrictionOn(counterparty => counterparty.Id)
				.IsIn(counterpartyIds)
				.List()
				.ToDictionary(counterparty => counterparty.Id);

			return states
				.Where(state => counterpartiesById.ContainsKey(state.CounterpartyId))
				.Select(state => new ReportRow
				{
					Counterparty = counterpartiesById[state.CounterpartyId],
					State = state
				})
				.ToList();
		}

		private static void ResetForResend(MindboxCounterpartyState state)
		{
			var now = DateTime.UtcNow;

			// Новый transactionId нужен, если прежний запрос точно не был принят Mindbox:
			// ошибка данных или ключ ещё не задан. В остальных случаях повторяем с тем же
			// ключом, чтобы Mindbox не создал дубликат.
			if(state.Status == MindboxCounterpartyOutboxStatus.DataError
				|| !state.TransactionId.HasValue
				|| state.TransactionId.Value == Guid.Empty)
			{
				state.TransactionId = Guid.NewGuid();
			}

			state.Status = MindboxCounterpartyOutboxStatus.Unknown;
			state.Attempts = 0;
			state.LastAttemptAt = null;
			state.LastError = null;
			state.LastHttpStatusCode = null;
			state.LastMindboxStatus = null;
			state.LastErrorId = null;
			state.LastAttemptDurationMs = null;
			state.SentAt = null;
			state.UpdatedAt = now;

			if(!state.CreatedAt.HasValue)
			{
				state.CreatedAt = now;
			}
		}

		private static void WriteConsole(IList<ReportRow> rows)
		{
			Console.WriteLine("erpId\tКонтрагент\tСтатус\tПопыток\tПоследняя попытка\tОшибка\ttransactionId");

			foreach(var row in rows)
			{
				Console.WriteLine(
					$"{row.Counterparty.Id}\t{row.Counterparty.Name}\t{GetStatusName(row.State.Status)}\t{row.State.Attempts}\t{FormatDate(row.State.LastAttemptAt)}\t{row.State.LastError}\t{row.State.TransactionId?.ToString()}");
			}
		}

		private static void WriteCsv(IList<ReportRow> rows, string outputFile)
		{
			var directory = Path.GetDirectoryName(outputFile);

			if(!string.IsNullOrWhiteSpace(directory))
			{
				Directory.CreateDirectory(directory);
			}

			using(var writer = new StreamWriter(outputFile, false, _utf8WithBom))
			{
				writer.Write("erpId;counterpartyName;status;attempts;lastAttemptAt;lastError;lastHttpStatusCode;lastMindboxStatus;lastErrorId;transactionId\n");

				foreach(var row in rows)
				{
					var values = new[]
					{
						row.Counterparty.Id.ToString(),
						row.Counterparty.Name,
						GetStatusName(row.State.Status),
						row.State.Attempts.ToString(),
						FormatDate(row.State.LastAttemptAt),
						row.State.LastError,
						row.State.LastHttpStatusCode?.ToString(),
						row.State.LastMindboxStatus,
						row.State.LastErrorId,
						row.State.TransactionId?.ToString() ?? string.Empty
					};

					writer.Write(string.Join(";", values.Select(Escape)));
					writer.Write('\n');
				}
			}
		}

		private static string FormatDate(DateTime? date) =>
			date.HasValue ? date.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;

		private static string GetStatusName(MindboxCounterpartyOutboxStatus status)
		{
			switch(status)
			{
				case MindboxCounterpartyOutboxStatus.Sent:
					return "Отправлен";
				case MindboxCounterpartyOutboxStatus.DataError:
					return "Ошибка данных";
				case MindboxCounterpartyOutboxStatus.Error:
					return "Ошибка";
				case MindboxCounterpartyOutboxStatus.Unknown:
					return "Не отправлялся";
				default:
					return "В очереди";
			}
		}

		private static string Escape(string value)
		{
			if(string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			if(value.IndexOf(';') < 0
				&& value.IndexOf('"') < 0
				&& value.IndexOf('\r') < 0
				&& value.IndexOf('\n') < 0)
			{
				return value;
			}

			return $"\"{value.Replace("\"", "\"\"")}\"";
		}

		private sealed class ReportRow
		{
			public CounterpartyEntity Counterparty { get; set; }
			public MindboxCounterpartyState State { get; set; }
		}

		private sealed class CounterpartyWithState
		{
			public CounterpartyEntity Counterparty { get; set; }
			public MindboxCounterpartyState State { get; set; }
		}
	}
}
