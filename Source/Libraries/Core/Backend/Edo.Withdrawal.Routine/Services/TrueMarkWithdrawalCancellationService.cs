using Edo.Transport;
using Microsoft.Extensions.Logging;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Edo.Withdrawal.Routine.Services
{
	/// <summary>
	/// Управляет отменой вывода кодов из оборота перед переотправкой документа ЭДО.
	/// </summary>
	public class TrueMarkWithdrawalCancellationService : ITrueMarkWithdrawalCancellationService
	{
		private const int MaxErrorMessageLength = 500;
		private readonly ILogger<TrueMarkWithdrawalCancellationService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ITrueMarkApiClient _trueMarkApiClient;
		private readonly IEdoRequestCreatedEventPublisher _edoRequestCreatedEventPublisher;

		public TrueMarkWithdrawalCancellationService(
			ILogger<TrueMarkWithdrawalCancellationService> logger,
			IUnitOfWorkFactory uowFactory,
			ITrueMarkApiClient trueMarkApiClient,
			IEdoRequestCreatedEventPublisher edoRequestCreatedEventPublisher)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));
			_edoRequestCreatedEventPublisher = edoRequestCreatedEventPublisher
				?? throw new ArgumentNullException(nameof(edoRequestCreatedEventPublisher));
		}

		/// <summary>
		/// Отправляет в ЧЗ ожидающие документы отмены вывода из оборота.
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		public async Task SendCancellationDocuments(CancellationToken cancellationToken)
		{
			int[] requestIds;

			using(var uow = _uowFactory.CreateWithoutRoot(nameof(SendCancellationDocuments)))
			{
				requestIds = uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
					.Where(x => x.Status == EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation)
					.Select(x => x.Id)
					.ToArray();
			}

			_logger.LogInformation("Найдено {RequestsCount} запросов для отмены вывода из оборота в ЧЗ", requestIds.Length);

			foreach(var requestId in requestIds)
			{
				await SendCancellationDocument(requestId, cancellationToken);
			}
		}

		/// <summary>
		/// Публикует заявки ЭДО, для которых ЧЗ успешно отменил вывод кодов из оборота.
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		public async Task PublishReadyResendRequests(CancellationToken cancellationToken)
		{
			int[] requestIds;

			using(var uow = _uowFactory.CreateWithoutRoot(nameof(PublishReadyResendRequests)))
			{
				requestIds = uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
					.Where(x => x.Status == EdoResendAfterTrueMarkCancellationStatus.ReadyToResend)
					.Select(x => x.Id)
					.ToArray();
			}

			foreach(var requestId in requestIds)
			{
				await PublishReadyResendRequest(requestId, cancellationToken);
			}
		}

		private async Task SendCancellationDocument(int requestId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot(nameof(SendCancellationDocument)))
			{
				var request = await GetRequestWithPessimisticLock(uow, requestId, cancellationToken);

				if(request?.Status != EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation)
				{
					return;
				}

				request.RegisterCancellationAttempt();

				try
				{
					var withdrawalDocument = request.WithdrawalDocument;

					if(withdrawalDocument?.Guid is null)
					{
						throw new InvalidOperationException("У документа вывода из оборота отсутствует Guid");
					}

					var cancellationDocumentId = await _trueMarkApiClient.SendIndividualAccountingWithdrawalCancellationDocument(
						withdrawalDocument.Guid.Value,
						withdrawalDocument.Organization.INN,
						cancellationToken);

					var cancellationDocument = new TrueMarkDocument
					{
						Order = request.Order,
						Guid = new Guid(cancellationDocumentId),
						Organization = withdrawalDocument.Organization,
						Type = TrueMarkDocument.TrueMarkDocumentType.WithdrawalCancellation,
						WithdrawalEdoTask = withdrawalDocument.WithdrawalEdoTask
					};

					request.MarkCancellationSent(cancellationDocument);

					await uow.SaveAsync(cancellationDocument, cancellationToken: cancellationToken);
					await uow.SaveAsync(request, cancellationToken: cancellationToken);
					await uow.CommitAsync(cancellationToken);
				}
				catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
				{
					throw;
				}
				catch(Exception ex)
				{
					request.MarkCancellationFailed(TruncateError(ex.Message));
					await uow.SaveAsync(request, cancellationToken: cancellationToken);
					await uow.CommitAsync(cancellationToken);

					_logger.LogError(ex, "Не удалось отправить документ отмены вывода из оборота по запросу {RequestId}", requestId);
				}
			}
		}

		private async Task PublishReadyResendRequest(int requestId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot(nameof(PublishReadyResendRequest)))
			{
				var request = await GetRequestWithPessimisticLock(uow, requestId, cancellationToken);

				if(request?.Status != EdoResendAfterTrueMarkCancellationStatus.ReadyToResend)
				{
					return;
				}

				await _edoRequestCreatedEventPublisher.Publish(
					request.ResendEdoRequest.Id,
					"Переотправка документов ЭДО после отмены вывода кодов из оборота в ЧЗ",
					cancellationToken);

				request.MarkCompleted();
				await uow.SaveAsync(request, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);
			}
		}

		private static async Task<EdoResendAfterTrueMarkCancellationRequest> GetRequestWithPessimisticLock(
			IUnitOfWork uow,
			int requestId,
			CancellationToken cancellationToken)
		{
			uow.OpenTransaction();

			return await uow.Session.GetAsync<EdoResendAfterTrueMarkCancellationRequest>(
				requestId,
				LockMode.Upgrade,
				cancellationToken);
		}

		private static string TruncateError(string error)
		{
			if(string.IsNullOrWhiteSpace(error))
			{
				return "Неизвестная ошибка при отмене вывода из оборота в ЧЗ";
			}

			return error.Substring(0, Math.Min(error.Length, MaxErrorMessageLength));
		}
	}
}
