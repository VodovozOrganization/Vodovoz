using Microsoft.Extensions.DependencyInjection;
using NHibernate.Action;
using NHibernate.Event;
using QS.DomainModel.Entity;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	/// <summary>
	/// Пересчитывает частоту затронутых точек доставки после flush, в транзакции заказа.
	/// </summary>
	internal sealed class DeliveryPointOrderFrequencyTracker : ISingleUowEventListener,
		IUowPostUpdateEventListener, IUowPostInsertEventListener,
		IBeforeTransactionCompletionProcess, IAfterTransactionCompletionProcess
	{
		private readonly IUnitOfWork _uow;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly Dictionary<OrderEntity, (OrderStatus? Status, IDomainObject Point, bool Inserted)> _orders
			= new Dictionary<OrderEntity, (OrderStatus?, IDomainObject, bool)>();
		private bool _registered;

		public DeliveryPointOrderFrequencyTracker(IUnitOfWorkTracked uow, IServiceScopeFactory scopeFactory)
		{
			_uow = uow as IUnitOfWork ?? throw new ArgumentException("Трекер требует единицу работы с сессией.", nameof(uow));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}

		public void OnPostUpdate(IUnitOfWorkTracked uow, PostUpdateEvent updateEvent)
		{
			if(!(updateEvent.Entity is OrderEntity order) || _orders.ContainsKey(order))
			{
				return;
			}

			var statusIndex = Array.IndexOf(updateEvent.Persister.PropertyNames, nameof(OrderEntity.OrderStatus));
			if(statusIndex < 0)
			{
				return;
			}

			var oldStatus = updateEvent.OldState == null
				? (OrderStatus?)null
				: (OrderStatus)updateEvent.OldState[statusIndex];
			var pointIndex = Array.IndexOf(updateEvent.Persister.PropertyNames, nameof(OrderEntity.DeliveryPoint));
			var oldPoint = pointIndex >= 0 && updateEvent.OldState != null
				? updateEvent.OldState[pointIndex] as IDomainObject
				: GetDeliveryPoint(order);
			// Общий трекер подавляет повторные события одной сущности в транзакции.
			// Поэтому сохраняем исходное состояние и сравниваем с итоговым после всех flush.
			_orders.Add(order, (oldStatus, oldPoint, false));

			Register(updateEvent.Session);
		}

		public void OnPostInsert(IUnitOfWorkTracked uow, PostInsertEvent insertEvent)
		{
			if(insertEvent.Entity is OrderEntity order && !_orders.ContainsKey(order))
			{
				_orders.Add(order, (null, null, true));
				Register(insertEvent.Session);
			}
		}

		private static bool IsIncluded(OrderStatus status) => OrderEntity.GetOnClosingOrderStatuses.Contains(status);

		// Бизнес-модель скрывает свойство базового класса и хранит точку в отдельном поле.
		private static IDomainObject GetDeliveryPoint(OrderEntity order) => order is Order businessOrder
			? businessOrder.DeliveryPoint
			: order.DeliveryPoint;

		private static void AddDeliveryPoint(HashSet<int> ids, IDomainObject point)
		{
			if(point?.Id > 0)
			{
				ids.Add(point.Id);
			}
		}

		private void Register(IEventSource session)
		{
			if(_registered)
			{
				return;
			}

			session.ActionQueue.RegisterProcess((IBeforeTransactionCompletionProcess)this);
			session.ActionQueue.RegisterProcess((IAfterTransactionCompletionProcess)this);
			_registered = true;
		}

		public void ExecuteBeforeTransactionCompletion()
		{
			var deliveryPointIds = new HashSet<int>();
			foreach(var entry in _orders)
			{
				var oldStatus = entry.Value.Status;
				var newStatus = entry.Key.OrderStatus;
				var changed = entry.Value.Inserted
					? IsIncluded(newStatus)
					: !oldStatus.HasValue || (oldStatus.Value != newStatus
						&& (IsIncluded(oldStatus.Value) || IsIncluded(newStatus)));
				if(changed)
				{
					AddDeliveryPoint(deliveryPointIds, GetDeliveryPoint(entry.Key));
					AddDeliveryPoint(deliveryPointIds, entry.Value.Point);
				}
			}

			if(deliveryPointIds.Count == 0)
			{
				return;
			}

			using(var scope = _scopeFactory.CreateScope())
			{
				var repository = scope.ServiceProvider.GetRequiredService<IDeliveryPointRepository>();
				foreach(var id in deliveryPointIds.OrderBy(x => x))
				{
					repository.UpdateOrderFrequency(_uow, id);
				}
			}
		}

		public Task ExecuteBeforeTransactionCompletionAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ExecuteBeforeTransactionCompletion();
			return Task.CompletedTask;
		}

		public void ExecuteAfterTransactionCompletion(bool success)
		{
			// Очищаем накопленное и после rollback, если UoW будет использован повторно.
			_orders.Clear();
			_registered = false;
		}

		public Task ExecuteAfterTransactionCompletionAsync(bool success, CancellationToken cancellationToken)
		{
			ExecuteAfterTransactionCompletion(success);
			return Task.CompletedTask;
		}
	}
}
