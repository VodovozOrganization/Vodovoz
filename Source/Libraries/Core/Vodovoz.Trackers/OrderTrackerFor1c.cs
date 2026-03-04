using MySqlConnector;
using NHibernate.Event;
using QS.DomainModel.Tracking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vodovoz.Core.Domain.Attributes;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Trackers
{
	public class OrderTrackerFor1c :
		ISingleUowEventListener,
		IUowPostInsertEventListener,
		IUowPostUpdateEventListener,
		IUowPostDeleteEventListener,
		IUowPostCommitEventListener
	{
		private readonly MySqlConnectionStringBuilder _connectionStringBuilder;
		private readonly HashSet<int> _changedOrderIds = new HashSet<int>();

		private static readonly ConcurrentDictionary<Type, HashSet<string>> _trackedPropertiesByType
			= new ConcurrentDictionary<Type, HashSet<string>>();

		public OrderTrackerFor1c(MySqlConnectionStringBuilder connectionStringBuilder)
		{
			if(!connectionStringBuilder.AllowUserVariables)
			{
				connectionStringBuilder.AllowUserVariables = true;
			}
			_connectionStringBuilder = connectionStringBuilder;
		}

		public void OnPostInsert(IUnitOfWorkTracked uow, PostInsertEvent insertEvent)
		{
			var orderId = GetOrderId(insertEvent.Entity);

			if(orderId == null)
			{
				return;
			}

			_changedOrderIds.Add(orderId.Value);
		}

		public void OnPostDelete(IUnitOfWorkTracked uow, PostDeleteEvent deleteEvent)
		{
			var orderId = GetOrderId(deleteEvent.Entity);

			if(orderId == null)
			{
				return;
			}
			_changedOrderIds.Add(orderId.Value);
		}

		public void OnPostUpdate(IUnitOfWorkTracked uow, PostUpdateEvent updateEvent)
		{
			var orderId = GetOrderId(updateEvent.Entity);

			if(orderId == null)
			{
				return;
			}

			var trackedProps = GetTrackedProperties(updateEvent.Entity.GetType());

			for(int i = 0; i < updateEvent.State.Length; i++)
			{
				var propName = updateEvent.Persister.PropertyNames[i];

				if(!trackedProps.Contains(propName))
				{
					continue;
				}

				if(!Equals(updateEvent.OldState[i], updateEvent.State[i]))
				{
					_changedOrderIds.Add(orderId.Value);

					return;
				}
			}
		}

		public void OnPostCommit(IUnitOfWorkTracked uow)
		{
			if(!_changedOrderIds.Any())
			{
				return;
			}

			var orderIds = _changedOrderIds.ToArray();

			_changedOrderIds.Clear();

			const string sql = @"
				INSERT INTO orders_to_1c_exports 
					(order_id, last_order_change_date)
				VALUES 
					(@OrderId, CURRENT_TIMESTAMP)
				ON DUPLICATE KEY UPDATE 
					last_order_change_date = CURRENT_TIMESTAMP";

			using(var connection = new MySqlConnection(_connectionStringBuilder.ConnectionString))
			{
				connection.Open();

				using(var transaction = connection.BeginTransaction())
				{
					try
					{
						using(var cmd = new MySqlCommand(sql, connection, transaction))
						{
							var param = cmd.Parameters.AddWithValue("@OrderId", MySqlDbType.Int32);

							foreach(int id in orderIds)
							{
								param.Value = id;
								cmd.ExecuteNonQuery();
							}
						}

						transaction.Commit();
					}
					catch
					{
						transaction.Rollback();

						throw;
					}
				}
			}
		}

		private int? GetOrderId(object entity)
		{
			var order =
				(entity as OrderEntity)
				?? (entity as OrderItemEntity)?.Order;

			if(order == null)
			{
				return null;
			}

			var date = order.DeliveryDate;

			if(date is null)
			{
				throw new InvalidOperationException("Заказ не может быть обработан: отсутствует дата доставки");
			}

			var canChangeToday = IsOrderInAllowedDepth(date.Value);

			return canChangeToday ? (int?)order.Id : null;
		}


		private static HashSet<string> GetTrackedProperties(Type entityType)
		{
			return _trackedPropertiesByType.GetOrAdd(entityType, type =>
			{
				var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where(p => p.GetCustomAttribute<OrderTracker1cAttribute>() != null)
					.Select(p => p.Name);

				return new HashSet<string>(props, StringComparer.Ordinal);
			});
		}

		private static bool IsOrderInAllowedDepth(DateTime orderDate)
		{
			var today = DateTime.Today;

			var todayQuarterStart = GetQuarterStart(today);

			var previousQuarterStart = todayQuarterStart.AddMonths(-3);

			var previousQuarterEnd = todayQuarterStart.AddDays(-1);

			var lastAllowedDate = previousQuarterEnd.AddDays(15);

			if(orderDate >= todayQuarterStart)
			{
				return true;
			}

			var canChangeOrder = orderDate >= previousQuarterStart && orderDate <= previousQuarterEnd
				? today <= lastAllowedDate
				: false;

			return canChangeOrder;
		}

		private static DateTime GetQuarterStart(DateTime date)
		{
			var quarter = (date.Month - 1) / 3 + 1;
			var month = (quarter - 1) * 3 + 1;
			return new DateTime(date.Year, month, 1);
		}
	}
}
