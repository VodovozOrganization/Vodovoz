using Edo.Transport;
using Microsoft.Extensions.Logging;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark;

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
			int[] resendEdoRequestIds;

			using(var uow = _uowFactory.CreateWithoutRoot(nameof(PublishReadyResendRequests)))
			{
				resendEdoRequestIds = uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
					.Where(x => x.Status == EdoResendAfterTrueMarkCancellationStatus.ReadyToResend)
					.Select(x => x.ResendEdoRequest.Id)
					.Distinct()
					.ToArray();
			}

			foreach(var resendEdoRequestId in resendEdoRequestIds)
			{
				await PublishReadyResendRequest(resendEdoRequestId, cancellationToken);
			}
		}

		private async Task SendCancellationDocument(int requestId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot(nameof(SendCancellationDocument)))
			{
				uow.OpenTransaction();

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
						withdrawalDocument.ProductGroup,
						cancellationToken);

					var cancellationDocument = new TrueMarkDocument
					{
						Order = request.Order,
						Guid = new Guid(cancellationDocumentId),
						Organization = withdrawalDocument.Organization,
						Type = TrueMarkDocument.TrueMarkDocumentType.WithdrawalCancellation,
						WithdrawalEdoTask = withdrawalDocument.WithdrawalEdoTask,
						ProductGroup = withdrawalDocument.ProductGroup
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

		private async Task PublishReadyResendRequest(int resendEdoRequestId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot(nameof(PublishReadyResendRequest)))
			{
				var requestIds = uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
					.Where(x => x.ResendEdoRequest.Id == resendEdoRequestId)
					.OrderBy(x => x.Id)
					.Select(x => x.Id)
					.ToList();

				if(!requestIds.Any())
				{
					return;
				}

				uow.OpenTransaction();

				var requests = new List<EdoResendAfterTrueMarkCancellationRequest>();
				foreach(var requestId in requestIds)
				{
					var lockedRequest = await GetRequestWithPessimisticLock(uow, requestId, cancellationToken);
					if(lockedRequest != null)
					{
						requests.Add(lockedRequest);
					}
				}

				if(requests.Count == 0 || requests.Any(x => x.Status != EdoResendAfterTrueMarkCancellationStatus.ReadyToResend))
				{
					return;
				}

				await _edoRequestCreatedEventPublisher.Publish(
					uow,
					resendEdoRequestId,
					"Переотправка документов ЭДО после отмены вывода кодов из оборота в ЧЗ",
					cancellationToken);

				foreach(var request in requests)
				{
					request.MarkCompleted();
					await uow.SaveAsync(request, cancellationToken: cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private static async Task<EdoResendAfterTrueMarkCancellationRequest> GetRequestWithPessimisticLock(
			IUnitOfWork uow,
			int requestId,
			CancellationToken cancellationToken)
		{
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
