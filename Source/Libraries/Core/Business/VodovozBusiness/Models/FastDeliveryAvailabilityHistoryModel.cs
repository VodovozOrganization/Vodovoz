using QS.DomainModel.UoW;
using System;
using System.Linq;
using NLog;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Services;

namespace Vodovoz.Models
{
	public class FastDeliveryAvailabilityHistoryModel
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

		public void ClearFastDeliveryAvailabilityHistory(IFastDeliveryAvailabilityHistoryParameterProvider fastDeliveryAvailabilityHistoryParameterProvider)
		{
			if(fastDeliveryAvailabilityHistoryParameterProvider.FastDeliveryHistoryClearDate >= DateTime.Now.Date)
			{
				return;
			}

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("ClearFastDeliveryAvailabilityHistory"))
			{
				// Такое удаление по ID быстрее. Если грузить полностью сущность, то будут подтягиваться все ТД (в мапинге сущности Not.LazyLoad()) и связанные с ней данные 
				var availabilityHistories = uow.Session.Query<FastDeliveryAvailabilityHistory>()
					.Where(x => x.VerificationDate < DateTime.Now.Date.AddDays(-fastDeliveryAvailabilityHistoryParameterProvider.FastDeliveryHistoryStorageDays))
					.Select(x=> x.Id)
					.ToList();

				var availabilityOrderItemsHistory = uow.Session.Query<FastDeliveryOrderItemHistory>()
					.Where(x => availabilityHistories.Contains(x.FastDeliveryAvailabilityHistory.Id))
					.Select(x => x.Id)
					.ToList();

				var availabilityHistoriesItems = uow.Session.Query<FastDeliveryAvailabilityHistoryItem>()
					.Where(x => availabilityHistories.Contains(x.FastDeliveryAvailabilityHistory.Id))
					.Select(x => x.Id)
					.ToList();

				var availabilityDistributionHistory = uow.Session.Query<FastDeliveryNomenclatureDistributionHistory>()
					.Where(x => availabilityHistories.Contains(x.FastDeliveryAvailabilityHistory.Id))
					.Select(x => x.Id)
					.ToList();

				try
				{
					availabilityOrderItemsHistory.ForEach(id=> uow.Delete(new FastDeliveryOrderItemHistory { Id = id }));
					availabilityDistributionHistory.ForEach(id => uow.Delete(new FastDeliveryNomenclatureDistributionHistory { Id = id }));
					availabilityHistoriesItems.ForEach(id => uow.Delete(new FastDeliveryAvailabilityHistoryItem { Id = id }));
					availabilityHistories.ForEach(id => uow.Delete(new FastDeliveryAvailabilityHistory { Id = id }));

					uow.Commit();
				}
				catch(Exception e)
				{
					_logger.Error(e, "Не удалось очистить журнал истории проверки экспресс-доставки.");
				}
			}

			fastDeliveryAvailabilityHistoryParameterProvider.UpdateFastDeliveryHistoryClearDate(DateTime.Now.Date.ToString());
		}
	}
}
