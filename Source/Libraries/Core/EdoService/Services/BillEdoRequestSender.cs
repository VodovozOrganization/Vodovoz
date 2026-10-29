using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using NHibernate;
using TransactionalOutbox.MassTransit;
using NLog;
using TaxcomEdo.Contracts.Documents;

using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace EdoService.Library.Services
{
	/// <summary>
	/// Сохраняет подготовленный снимок счета и запускает создание неформализованной задачи ЭДО
	/// </summary>
	public class BillEdoRequestSender : IBillEdoRequestSender
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IBillEdoDocumentFileStorageService _fileStorageService;
		private readonly IUnitOfWorkOutbox<IBus> _outbox;

		/// <summary>
		/// Создает отправитель заявок счетов
		/// </summary>
		/// <param name="uowFactory">Фабрика единиц работы</param>
		/// <param name="fileStorageService">Хранилище PDF счетов</param>
		public BillEdoRequestSender(IUnitOfWorkFactory uowFactory, IBillEdoDocumentFileStorageService fileStorageService, IUnitOfWorkOutbox<IBus> outbox)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
			_outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
		}

		/// <summary>
		/// Сохраняет подготовленную заявку счета и событие её создания в одной транзакции
		/// </summary>
		/// <param name="request">Заявка со снимком счета без созданной задачи</param>
		/// <param name="source">Источник запуска отправки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Идентификатор сохраненной заявки</returns>
		public async Task<int> SendAsync(
			InformalEdoRequest request, EdoRequestSource source, CancellationToken cancellationToken = default)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Создание заявки счета по ЭДО"))
			{
				return await SendAsync(uow, request, source, cancellationToken);
			}
		}

		/// <summary>
		/// Сохраняет заявку, событие её создания и изменения инициатора в одной транзакции
		/// </summary>
		/// <param name="uow">Единица работы инициатора отправки</param>
		/// <param name="request">Заявка со снимком счета без созданной задачи</param>
		/// <param name="source">Источник запуска отправки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Идентификатор сохраненной заявки</returns>
		public async Task<int> SendAsync(
			IUnitOfWork uow, InformalEdoRequest request, EdoRequestSource source, CancellationToken cancellationToken = default)
		{
			if(request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}
			if(!request.IsBill || request.Task != null || string.IsNullOrWhiteSpace(request.DocumentSnapshot))
			{
				throw new ArgumentException("Требуется подготовленная заявка счета без созданной задачи", nameof(request));
			}

			try
			{
				request.Source = source;
				if(request.Order != null)
				{
					request.Order = uow.Session.Load<OrderEntity>(request.Order.Id);
				}
				await uow.SaveAsync(request, cancellationToken: cancellationToken);
				await _outbox.GetPublishEndpoint(uow).Publish(
					new InformalEdoRequestCreatedEvent { InformalRequestId = request.Id },
					cancellationToken);
				await uow.CommitAsync(cancellationToken);
				return request.Id;
			}
			catch
			{
				await DeleteUncommittedPdf(uow, request);
				throw;
			}
		}

		private async Task DeleteUncommittedPdf(IUnitOfWork uow, InformalEdoRequest request)
		{
			try
			{
				var transaction = uow.Session.GetCurrentTransaction();
				if(transaction?.IsActive == true)
				{
					await transaction.RollbackAsync(CancellationToken.None);
				}

				if(request.Id != 0)
				{
					using(var verificationUow = _uowFactory.CreateWithoutRoot("Проверка сохранения снимка счета ЭДО"))
					{
						if(verificationUow.GetAll<InformalEdoRequest>()
							.Any(x => x.Id == request.Id && x.DocumentSnapshot == request.DocumentSnapshot))
						{
							return;
						}
					}
				}

				var snapshot = JsonSerializer.Deserialize<InfoForCreatingEdoInformalOrderDocument>(request.DocumentSnapshot);
				await _fileStorageService.DeleteAsync(snapshot.MainDocumentId, CancellationToken.None);
			}
			catch(Exception exception)
			{
				_logger.Error(exception, "Не удалось проверить сохранение заявки счета №{RequestId} или удалить её PDF", request.Id);
			}
		}
	}
}
