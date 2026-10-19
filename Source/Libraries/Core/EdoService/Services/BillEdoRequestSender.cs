using System;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using TransactionalOutbox.Domain;
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
		private readonly IUnitOfWorkFactory _uowFactory;

		/// <summary>
		/// Создает отправитель заявок счетов
		/// </summary>
		/// <param name="uowFactory">Фабрика единиц работы</param>
		public BillEdoRequestSender(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		/// <summary>
		/// Сохраняет новую заявку счета и событие её создания в одной транзакции
		/// </summary>
		/// <param name="request">Новая заявка, подготовленная фабрикой снимков счетов</param>
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
		/// <param name="request">Новая заявка со снимком счета</param>
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
			if(!request.IsBill || request.Id != 0 || string.IsNullOrWhiteSpace(request.DocumentSnapshot))
			{
				throw new ArgumentException("Требуется новая заявка со снимком счета", nameof(request));
			}

			request.Source = source;
			if(request.Order != null)
			{
				request.Order = uow.Session.Load<OrderEntity>(request.Order.Id);
			}
			await uow.SaveAsync(request, cancellationToken: cancellationToken);
			await uow.SaveAsync(
				new OutboxMessage(new InformalEdoRequestCreatedEvent { InformalRequestId = request.Id }),
				cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);
			return request.Id;
		}
	}
}
