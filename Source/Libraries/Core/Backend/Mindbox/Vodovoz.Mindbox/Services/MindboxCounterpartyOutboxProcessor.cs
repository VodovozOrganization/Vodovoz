using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client.Mindbox;
using Vodovoz.Mindbox.Mapping;
using Vodovoz.Settings.Mindbox;

namespace Vodovoz.Mindbox.Services
{
	/// <summary>
	/// Отправляет сообщения очереди контрагентов в Mindbox
	/// </summary>
	public interface IMindboxCounterpartyOutboxProcessor
	{
		/// <summary>
		/// Обрабатывает пачку сообщений очереди
		/// </summary>
		/// <returns>Количество обработанных сообщений</returns>
		Task<int> ProcessAsync(CancellationToken cancellationToken);
	}

	/// <inheritdoc />
	public sealed class MindboxCounterpartyOutboxProcessor : IMindboxCounterpartyOutboxProcessor
	{
		private const int MaxErrorLength = 2000;
		private const int MaxRetryDelaySeconds = 3600;
		private const int DefaultRetryDelaySeconds = 60;

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IMindboxClient _mindboxClient;
		private readonly IMindboxCounterpartyRequestFactory _requestFactory;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxCounterpartyOutboxProcessor> _logger;

		public MindboxCounterpartyOutboxProcessor(
			IUnitOfWorkFactory unitOfWorkFactory,
			IMindboxClient mindboxClient,
			IMindboxCounterpartyRequestFactory requestFactory,
			IMindboxSettings settings,
			ILogger<MindboxCounterpartyOutboxProcessor> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_mindboxClient = mindboxClient ?? throw new ArgumentNullException(nameof(mindboxClient));
			_requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task<int> ProcessAsync(CancellationToken cancellationToken)
		{
			if(!_settings.IsEnabled)
			{
				return 0;
			}

			var maxAttempts = Math.Max(1, _settings.MaxSendAttempts);
			var batchSize = Math.Max(1, _settings.SendBatchSize);

			List<int> messageIds;

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Выборка очереди отправки в Mindbox"))
			{
				var now = DateTime.UtcNow;

				messageIds = unitOfWork.Session.Query<MindboxCounterpartyOutboxMessage>()
					.Where(message => message.Status == MindboxCounterpartyOutboxStatus.Pending
						&& message.Attempts < maxAttempts
						&& (message.NextAttemptAt == null || message.NextAttemptAt <= now))
					.OrderBy(message => message.CreatedAt)
					.Take(batchSize)
					.Select(message => message.Id)
					.ToList();
			}

			foreach(var messageId in messageIds)
			{
				if(cancellationToken.IsCancellationRequested)
				{
					break;
				}

				try
				{
					using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Отправка контрагента в Mindbox"))
					{
						var message = unitOfWork.GetById<MindboxCounterpartyOutboxMessage>(messageId);

						if(message == null)
						{
							continue;
						}

						await ProcessMessageAsync(unitOfWork, message, maxAttempts, cancellationToken);
					}
				}
				catch(Exception ex)
				{
					_logger.LogError(
						ex,
						"Не удалось обработать сообщение очереди Mindbox {MessageId}",
						messageId);
				}
			}

			return messageIds.Count;
		}

		private async Task ProcessMessageAsync(
			IUnitOfWork unitOfWork,
			MindboxCounterpartyOutboxMessage message,
			int maxAttempts,
			CancellationToken cancellationToken)
		{
			try
			{
				var counterparty = message.Counterparty;

				if(counterparty == null)
				{
					RegisterFailure(message, "Контрагент не найден", maxAttempts);
					_logger.LogError(
						"Не найден контрагент для сообщения очереди Mindbox {MessageId}",
						message.Id);
				}
				else
				{
					var request = _requestFactory.Create(counterparty);

					var result = await _mindboxClient.CreateCounterpartyAsync(
						request,
						message.TransactionId,
						cancellationToken);

					if(result.IsSuccess)
					{
						message.Status = MindboxCounterpartyOutboxStatus.Sent;
						message.SentAt = DateTime.UtcNow;
						message.LastError = null;
						message.NextAttemptAt = null;

						_logger.LogInformation(
							"Контрагент {CounterpartyId} успешно отправлен в Mindbox",
							counterparty.Id);
					}
					else
					{
						RegisterFailure(message, result.ErrorMessage, maxAttempts);

						_logger.LogWarning(
							"Не удалось отправить контрагента {CounterpartyId} в Mindbox. Попытка {Attempt}. Ошибка: {Error}",
							counterparty.Id,
							message.Attempts,
							result.ErrorMessage);
					}
				}
			}
			catch(Exception ex)
			{
				RegisterFailure(message, ex.Message, maxAttempts);
				_logger.LogError(ex, "Ошибка отправки сообщения очереди Mindbox {MessageId}", message.Id);
			}

			unitOfWork.Save(message);
			unitOfWork.Commit();
		}

		private void RegisterFailure(MindboxCounterpartyOutboxMessage message, string error, int maxAttempts)
		{
			message.Attempts++;
			message.LastError = Truncate(error, MaxErrorLength);

			if(message.Attempts >= maxAttempts)
			{
				message.Status = MindboxCounterpartyOutboxStatus.Failed;
				message.NextAttemptAt = null;

				_logger.LogError(
					"Исчерпаны попытки отправки сообщения очереди Mindbox {MessageId}. Ошибка: {Error}",
					message.Id,
					error);

				return;
			}

			var baseDelay = _settings.RetryDelaySeconds > 0
				? _settings.RetryDelaySeconds
				: DefaultRetryDelaySeconds;

			var delaySeconds = baseDelay * Math.Pow(2, message.Attempts - 1);

			message.NextAttemptAt = DateTime.UtcNow.AddSeconds(Math.Min(delaySeconds, MaxRetryDelaySeconds));
		}

		private static string Truncate(string value, int maxLength)
		{
			if(string.IsNullOrEmpty(value) || value.Length <= maxLength)
			{
				return value;
			}

			return value.Substring(0, maxLength);
		}
	}
}
