using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Nodes;

namespace Vodovoz.Tools.Orders
{
	public interface IFastDeliveryHistoryConverter
	{
		/// <summary>
		/// Конвертирование списка <see cref="FastDeliveryAvailabilityHistoryItem"/> в список <see cref="FastDeliveryVerificationDetailsNode"/>
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="items">Список данных</param>
		/// <returns></returns>
		IList<FastDeliveryVerificationDetailsNode> ConvertAvailabilityHistoryItemsToVerificationDetailsNodes(
			IUnitOfWork uow, IEnumerable<FastDeliveryAvailabilityHistoryItem> items);
		/// <summary>
		/// Конвертирование списка <see cref="NomenclatureAmountNode"/> в список <see cref="FastDeliveryOrderItemHistory"/>
		/// </summary>
		/// <param name="nomenclatureNodes">Данные по номенклатурам <see cref="NomenclatureAmountNode"/></param>
		/// <param name="fastDeliveryAvailabilityHistory">История доступности ДЗЧ</param>
		/// <returns></returns>
		IList<FastDeliveryOrderItemHistory> ConvertNomenclatureAmountNodesToOrderItemsHistory(
			IEnumerable<NomenclatureAmountNode> nomenclatureNodes,
			FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory);
		/// <summary>
		/// Конвертирование списка <see cref="AdditionalLoadingNomenclatureDistribution"/> в список <see cref="FastDeliveryNomenclatureDistributionHistory"/>
		/// </summary>
		/// <param name="distributions">Список данных <see cref="AdditionalLoadingNomenclatureDistribution"/></param>
		/// <param name="fastDeliveryAvailabilityHistory">История доступности ДЗЧ</param>
		/// <returns></returns>
		IList<FastDeliveryNomenclatureDistributionHistory> ConvertNomenclatureDistributionToDistributionHistory(
			IEnumerable<AdditionalLoadingNomenclatureDistribution> distributions,
			FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory);
		/// <summary>
		/// Конвертирование списка <see cref="FastDeliveryVerificationDetailsNode"/> в список <see cref="FastDeliveryAvailabilityHistoryItem"/>
		/// </summary>
		/// <param name="nodes">Список данных <see cref="FastDeliveryVerificationDetailsNode"/></param>
		/// <param name="fastDeliveryAvailabilityHistory">История доступности ДЗЧ</param>
		/// <returns></returns>
		IList<FastDeliveryAvailabilityHistoryItem> ConvertVerificationDetailsNodesToAvailabilityHistoryItems(
			IEnumerable<FastDeliveryVerificationDetailsNode> nodes,
			FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory);
	}
}
