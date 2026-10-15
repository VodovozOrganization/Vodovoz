using System;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace EdoService.Library.Services
{
	/// <summary>
	/// Сохраняет подготовленный снимок счета и запускает создание неформализованной задачи ЭДО
	/// </summary>
	public class BillEdoRequestSender
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IPublishEndpoint _publishEndpoint;

		/// <summary>
		/// Создает отправитель заявок счетов
		/// </summary>
		/// <param name="uowFactory">Фабрика единиц работы</param>
		/// <param name="publishEndpoint">Публикация событий ЭДО</param>
		public BillEdoRequestSender(IUnitOfWorkFactory uowFactory, IPublishEndpoint publishEndpoint)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		/// <summary>
		/// Сохраняет новую заявку счета и публикует событие после commit
		/// </summary>
		/// <param name="request">Новая заявка, подготовленная фабрикой снимков счетов</param>
		/// <param name="source">Источник запуска отправки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Идентификатор сохраненной заявки</returns>
		public async Task<int> SendAsync(
			InformalEdoRequest request, EdoRequestSource source, CancellationToken cancellationToken = default)
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
			using(var uow = _uowFactory.CreateWithoutRoot("Создание заявки счета по ЭДО"))
			{
				if(request.Order != null)
				{
					request.Order = uow.Session.Load<OrderEntity>(request.Order.Id);
				}
				await uow.SaveAsync(request, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);
			}

			await _publishEndpoint.Publish(new InformalEdoRequestCreatedEvent { InformalRequestId = request.Id }, cancellationToken);
			return request.Id;
		}
	}
}
