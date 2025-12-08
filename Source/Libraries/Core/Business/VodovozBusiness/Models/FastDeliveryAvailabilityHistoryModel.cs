using NLog;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.Models
{
	public class FastDeliveryAvailabilityHistoryModel : IFastDeliveryAvailabilityHistoryModel
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public FastDeliveryAvailabilityHistoryModel(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public void SaveFastDeliveryAvailabilityHistory(FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("SaveFastDeliveryAvailabilityHistory"))
			{
				try
				{
					uow.Save(fastDeliveryAvailabilityHistory);
					uow.Commit();
				}
				catch(Exception e)
				{
					_logger.Error(e, "Не удалось сохранить историю проверки экспресс-доставки.");
				}
			}
		}

		public async Task SaveFastDeliveryAvailabilityHistoryAsync(
			FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory,
			CancellationToken cancellationToken
			)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("SaveFastDeliveryAvailabilityHistory"))
			{
				try
				{
					await uow.SaveAsync(fastDeliveryAvailabilityHistory, cancellationToken: cancellationToken);
					await uow.CommitAsync(cancellationToken);
				}
				catch(Exception e)
				{
					_logger.Error(e, "Не удалось сохранить историю проверки экспресс-доставки.");
				}
			}
		}

		public void ClearFastDeliveryAvailabilityHistory(
			IFastDeliveryAvailabilityHistorySettings fastDeliveryAvailabilityHistorySettings,
			TimeSpan? queryTimeoutTimeSpan = null)
		{
			var queryTimeout =
				queryTimeoutTimeSpan != null && queryTimeoutTimeSpan > TimeSpan.Zero
				? (int)queryTimeoutTimeSpan.Value.TotalSeconds
				: 600;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("ClearFastDeliveryAvailabilityHistory"))
			{
				try
				{
					var dateBefore = DateTime.Now.Date.AddDays(-fastDeliveryAvailabilityHistorySettings.FastDeliveryHistoryStorageDays);

					int deletedReferencedItems = default;
					int deletedFastDeliveryAvailabilityHistoryRows = default;

					using(var transaction = uow.Session.BeginTransaction())
					{
						deletedReferencedItems = uow.Session
							.CreateSQLQuery(GetFastDeliveryAvailabilityHistoryItemsDeletionQuery(dateBefore))
							.SetTimeout(queryTimeout)
							.ExecuteUpdate();

						deletedFastDeliveryAvailabilityHistoryRows = uow.Session
							.CreateSQLQuery(GetFastDeliveryAvailabilityHistoryDeletionQuery(dateBefore))
							.SetTimeout(queryTimeout)
							.ExecuteUpdate();

						transaction.Commit();
					}

					_logger.Debug("Удалено {DeletedFastDeliveryAvailabilityHistoryRows} записей проверки доступности экспресс-доставки и {DeletedReferencedItems} связанных записей",
						deletedFastDeliveryAvailabilityHistoryRows,
						deletedReferencedItems);
				}
				catch(Exception e)
				{
					_logger.Error(e, "Не удалось очистить журнал истории проверки экспресс-доставки.");
				}
			}
		}

		private string GetFastDeliveryAvailabilityHistoryItemsDeletionQuery(DateTime dateBefore)
		{
			var query =
				@"DELETE
					fast_delivery_order_items_history,
					fast_delivery_availability_history_items,
					fast_delivery_nomenclature_distribution_history
				FROM fast_delivery_availability_history
				LEFT JOIN fast_delivery_order_items_history
					ON fast_delivery_order_items_history.fast_delivery_availability_history_id = fast_delivery_availability_history.id
				LEFT JOIN fast_delivery_availability_history_items
					ON fast_delivery_availability_history_items.fast_delivery_availability_history_id = fast_delivery_availability_history.id
				LEFT JOIN fast_delivery_nomenclature_distribution_history
					ON fast_delivery_nomenclature_distribution_history.fast_delivery_availability_history_id  = fast_delivery_availability_history.id
				";

			query += $"WHERE fast_delivery_availability_history.verification_date < '{dateBefore:yyyy-MM-dd}'";

			return query;
		}

		private string GetFastDeliveryAvailabilityHistoryDeletionQuery(DateTime dateBefore)
		{
			var query =
				@"DELETE
				FROM fast_delivery_availability_history
				";

			query += $"WHERE fast_delivery_availability_history.verification_date < '{dateBefore:yyyy-MM-dd}'";

			return query;
		}
	}
}
