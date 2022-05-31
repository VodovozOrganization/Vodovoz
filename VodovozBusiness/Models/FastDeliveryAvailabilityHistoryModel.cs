using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Models
{
	public class FastDeliveryAvailabilityHistoryModel
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public FastDeliveryAvailabilityHistoryModel(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public void SaveFastDeliveryAvailabilityHistory(FastDeliveryVerificationDTO fastDeliveryVerificationDTO)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("FastDeliveryAvailabilityHistoryModel"))
			{
				var fastDeliveryAvailabilityHistory = fastDeliveryVerificationDTO.FastDeliveryAvailabilityHistory;

				var order = fastDeliveryVerificationDTO.FastDeliveryAvailabilityHistory.Order;
				if(order != null)
				{
					fastDeliveryAvailabilityHistory.Order = order.Id == 0 ? null : order;
					fastDeliveryAvailabilityHistory.Author = order.Author;
					fastDeliveryAvailabilityHistory.DeliveryPoint = order.DeliveryPoint;
					fastDeliveryAvailabilityHistory.District = order.DeliveryPoint.District;
					fastDeliveryAvailabilityHistory.Counterparty = order.Client;
				}

				var fastDeliveryHistoryItemConverter = new FastDeliveryHistoryConverter();

				fastDeliveryAvailabilityHistory.Items =
					fastDeliveryHistoryItemConverter.ConvertVerificationDetailsNodesToAvailabilityHistoryItems(
						fastDeliveryVerificationDTO.FastDeliveryVerificationDetailsNodes, fastDeliveryAvailabilityHistory);

				var distributions = uow.GetAll<AdditionalLoadingNomenclatureDistribution>();
				fastDeliveryAvailabilityHistory.NomenclatureDistributionHistoryItems =
					fastDeliveryHistoryItemConverter.ConvertNomenclatureDistributionToDistributionHistory(distributions, fastDeliveryAvailabilityHistory);


				uow.Save(fastDeliveryAvailabilityHistory);
				uow.Commit();
			}
		}
	}
}
