using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class AddMasterServiceHandler : AddProductHandler
	{
		/// <summary>
		/// Добавление в заказ номенклатуры типа "Сервисное обслуживание"
		/// </summary>
		/// <param name="uow">unit of work"</param>
		/// <param name="contractUpdater">Сервис обновления договора заказа</param>
		/// <param name="nomenclature">Номенклатура типа "Сервисное обслуживание"</param>
		/// <param name="count">Количество</param>
		/// <param name="quantityOfFollowingNomenclatures">Колличество номенклатуры, указанной в параметрах БД,
		/// которые будут добавлены в заказ вместе с мастером</param>
		public virtual void AddMasterNomenclature(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			Nomenclature nomenclature,
			int count,
			int quantityOfFollowingNomenclatures = 0)
		{
			if(nomenclature.Category != NomenclatureCategory.master) {
				return;
			}

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			AddOrderItem(
				uow,
				contractUpdater,
				OrderItem.CreateForSale(this, nomenclature, count, nomenclature.GetPrice(1, canApplyAlternativePrice)));

			if(quantityOfFollowingNomenclatures > 0)
			{
				Nomenclature followingNomenclature = _nomenclatureRepository.GetNomenclatureToAddWithMaster(UoW);
				if(!ObservableOrderItems.Any(i => i.Nomenclature.Id == followingNomenclature.Id))
				{
					AddAnyGoodsNomenclatureForSale(
						uow,
						contractUpdater,
						followingNomenclature,
						false,
						1);
				}
			}
		}
	}
}
