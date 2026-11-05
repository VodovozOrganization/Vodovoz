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
			if(options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Отчет по статусам Mindbox"))
			{
				var counterparties = GetCounterparties(unitOfWork, options.Statuses);

				if(!string.IsNullOrWhiteSpace(options.OutputFile))
				{
					WriteCsv(counterparties, options.OutputFile);
				}
				else
				{
					WriteConsole(counterparties);
				}

				_logger.LogInformation("Отчет Mindbox сформирован, записей: {Count}", counterparties.Count);

				return Task.FromResult(counterparties.Count);
			}
		}

		/// <inheritdoc />
		public Task<int> ResendAsync(MindboxAdminOptions options, CancellationToken cancellationToken)
		{
			if(options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Повторная отправка в Mindbox"))
			{
				var counterparties = GetCounterpartiesForResend(unitOfWork, options);
				var resendCount = 0;

				foreach(var counterparty in counterparties)
				{
					ResetForResend(counterparty);
					unitOfWork.Save(counterparty);
					resendCount++;
				}

				unitOfWork.Commit();

				_logger.LogInformation(
					"Поставлено на повторную отправку в Mindbox: {Count}. Отправка начнется после следующего прохода воркера.",
					resendCount);

				return Task.FromResult(resendCount);
			}
		}

		private static IList<CounterpartyEntity> GetCounterparties(
			IUnitOfWork unitOfWork,
			IEnumerable<MindboxCounterpartyOutboxStatus> statuses)
		{
			var statusArray = statuses.Cast<object>().ToArray();

			return unitOfWork.Session.QueryOver<CounterpartyEntity>()
				.WhereRestrictionOn(counterparty => counterparty.MindboxStatus)
				.IsIn(statusArray)
				.OrderBy(counterparty => counterparty.Id)
				.Asc
				.List();
		}

		private static IList<CounterpartyEntity> GetCounterpartiesForResend(
			IUnitOfWork unitOfWork,
			MindboxAdminOptions options)
		{
			if(options.CounterpartyIds.Count > 0)
			{
				var ids = options.CounterpartyIds.Distinct().ToArray();

				return unitOfWork.Session.QueryOver<CounterpartyEntity>()
					.WhereRestrictionOn(counterparty => counterparty.Id)
					.IsIn(ids)
					.List();
			}

			return GetCounterparties(unitOfWork, options.Statuses);
		}

		private static void ResetForResend(CounterpartyEntity counterparty)
		{
			// Новый transactionId нужен только если прежний запрос гарантированно не был принят Mindbox:
			// ошибка данных или transactionId ещё не задан. В остальных случаях повторяем с тем же
			// transactionId, чтобы Mindbox не создал дубликат.
			var shouldGenerateNewTransactionId =
				counterparty.MindboxStatus == MindboxCounterpartyOutboxStatus.DataError
				|| counterparty.MindboxTransactionId == Guid.Empty;

			if(shouldGenerateNewTransactionId)
			{
				counterparty.MindboxTransactionId = Guid.NewGuid();
			}

			// Unknown — признак того, что контрагента нужно поставить в очередь.
			// Воркер увидит его в следующем проходе и опубликует сообщение в outbox.
			counterparty.MindboxStatus = MindboxCounterpartyOutboxStatus.Unknown;
		}

		private static void WriteConsole(IList<CounterpartyEntity> counterparties)
		{
			Console.WriteLine("erpId\tКонтрагент\tСтатус\ttransactionId");

			foreach(var counterparty in counterparties)
			{
				Console.WriteLine(
					$"{counterparty.Id}\t{counterparty.Name}\t{GetStatusName(counterparty.MindboxStatus)}\t{counterparty.MindboxTransactionId}");
			}
		}

		private static void WriteCsv(IList<CounterpartyEntity> counterparties, string outputFile)
		{
			var directory = Path.GetDirectoryName(outputFile);

			if(!string.IsNullOrWhiteSpace(directory))
			{
				Directory.CreateDirectory(directory);
			}

			using(var writer = new StreamWriter(outputFile, false, _utf8WithBom))
			{
				writer.Write("erpId;counterpartyName;status;transactionId\n");

				foreach(var counterparty in counterparties)
				{
					var values = new[]
					{
						counterparty.Id.ToString(),
						counterparty.Name,
						GetStatusName(counterparty.MindboxStatus),
						counterparty.MindboxTransactionId.ToString()
					};

					writer.Write(string.Join(";", values.Select(Escape)));
					writer.Write('\n');
				}
			}
		}

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
	}
}
