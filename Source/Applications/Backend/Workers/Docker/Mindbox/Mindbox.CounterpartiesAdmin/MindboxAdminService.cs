using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.Mindbox;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.CounterpartiesAdmin
{

	/// <inheritdoc />
	public sealed class MindboxAdminService : IMindboxAdminService
	{
		private static readonly UTF8Encoding _utf8WithBom = new(true);

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxAdminService> _logger;

		public MindboxAdminService(
			IUnitOfWorkFactory unitOfWorkFactory,
			IMindboxSettings settings,
			ILogger<MindboxAdminService> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
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
				var messages = unitOfWork.Session.QueryOver<MindboxCounterpartyOutboxMessage>()
					.WhereRestrictionOn(message => message.Status)
					.IsIn(options.Statuses.Cast<object>().ToArray())
					.List()
					.OrderBy(message => message.Counterparty.Id)
					.ToList();

				if(!string.IsNullOrWhiteSpace(options.OutputFile))
				{
					WriteCsv(messages, options.OutputFile);
				}
				else
				{
					WriteConsole(messages);
				}

				_logger.LogInformation("Отчет Mindbox сформирован, записей: {Count}", messages.Count);

				return Task.FromResult(messages.Count);
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
				var counterpartyIds = GetCounterpartyIds(unitOfWork, options);
				var resendCount = 0;

				foreach(var counterpartyId in counterpartyIds)
				{
					Counterparty counterpartyAlias = null;

					var message = unitOfWork.Session.QueryOver<MindboxCounterpartyOutboxMessage>()
						.JoinAlias(item => item.Counterparty, () => counterpartyAlias)
						.Where(() => counterpartyAlias.Id == counterpartyId)
						.SingleOrDefault();

					if(message == null)
					{
						var counterparty = unitOfWork.GetById<Counterparty>(counterpartyId);

						if(counterparty == null)
						{
							_logger.LogWarning("Контрагент {CounterpartyId} не найден", counterpartyId);
							continue;
						}

						message = new MindboxCounterpartyOutboxMessage
						{
							Counterparty = counterparty,
							CreatedAt = DateTime.UtcNow
						};

						unitOfWork.Save(message);
					}

					ResetForResend(message);

					resendCount++;
				}

				unitOfWork.Commit();

				_logger.LogInformation("Поставлено на повторную отправку в Mindbox: {Count}", resendCount);

				return Task.FromResult(resendCount);
			}
		}

		private static IList<int> GetCounterpartyIds(IUnitOfWork unitOfWork, MindboxAdminOptions options)
		{
			if(options.CounterpartyIds.Count > 0)
			{
				return options.CounterpartyIds.Distinct().ToList();
			}

			var messages = unitOfWork.Session.QueryOver<MindboxCounterpartyOutboxMessage>()
				.WhereRestrictionOn(message => message.Status)
				.IsIn(options.Statuses.Cast<object>().ToArray())
				.List();

			return messages
				.Select(message => message.Counterparty.Id)
				.Distinct()
				.ToList();
		}

		private void ResetForResend(MindboxCounterpartyOutboxMessage message)
		{
			message.Status = MindboxCounterpartyOutboxStatus.Queued;
			message.TransactionId = Guid.NewGuid();
			message.Attempts = 0;
			message.LastAttemptAt = null;
			message.LastError = null;
			message.LastHttpStatusCode = null;
			message.LastMindboxStatus = null;
			message.LastErrorId = null;
			message.LastAttemptDurationMs = null;
			message.NextAttemptAt = null;
			message.SentAt = null;
			message.RetryUntil = DateTime.UtcNow.AddHours(Math.Max(1, _settings.RetryMaxHours));
		}

		private static void WriteConsole(IList<MindboxCounterpartyOutboxMessage> messages)
		{
			Console.WriteLine("erpId\tКонтрагент\tСтатус\tПопыток\tПоследняя попытка\t Ошибка");

			foreach(var message in messages)
			{
				Console.WriteLine(
					$"{message.Counterparty.Id}\t{message.Counterparty.Name}\t{GetStatusName(message.Status)}\t{message.Attempts}\t{FormatDate(message.LastAttemptAt)}\t{message.LastError}");
			}
		}

		private static void WriteCsv(IList<MindboxCounterpartyOutboxMessage> messages, string outputFile)
		{
			var directory = Path.GetDirectoryName(outputFile);

			if(!string.IsNullOrWhiteSpace(directory))
			{
				Directory.CreateDirectory(directory);
			}

			using(var writer = new StreamWriter(outputFile, false, _utf8WithBom))
			{
				writer.Write("erpId;counterpartyName;status;attempts;lastAttemptAt;lastError;lastHttpStatusCode;lastMindboxStatus;lastErrorId;transactionId\n");

				foreach(var message in messages)
				{
					var values = new[]
					{
						message.Counterparty.Id.ToString(),
						message.Counterparty.Name,
						GetStatusName(message.Status),
						message.Attempts.ToString(),
						FormatDate(message.LastAttemptAt),
						message.LastError,
						message.LastHttpStatusCode?.ToString(),
						message.LastMindboxStatus,
						message.LastErrorId,
						message.TransactionId.ToString()
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
