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
				var availabilityHistories = uow.Session.Query<FastDeliveryAvailabilityHistory>()
					.Where(x => x.VerificationDate < DateTime.Now.Date.AddDays(-fastDeliveryAvailabilityHistoryParameterProvider.FastDeliveryHistoryStorageDays))
					.ToList();

				try
				{
					foreach(var history in availabilityHistories)
					{
						uow.Delete(history);
					}

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
