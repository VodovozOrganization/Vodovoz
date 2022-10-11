using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Suppliers;

namespace Vodovoz.EntityRepositories.Suppliers
{
	public interface ISupplierPriceItemsRepository
	{
		/// <summary>
		/// Возврат списка сущностей типа <see cref="SupplierPriceItem"/>,
		/// которые содержат информацию о контрагенте, номенклатуре, стоимости
		/// и различных условий поставки, упорядоченых по возрастанию цены и в
		/// колличестве "Все", "Топ-3" или "Самый дешёвый" из <paramref name="orderingType"/>
		/// для ТМЦ <paramref name="nomenclature"/>.
		/// </summary>
		/// <param name="availabilityForSale">я дурак. мне не сформулировать.</param>
		IEnumerable<SupplierPriceItem> GetSupplierPriceItemsForNomenclature(
			IUnitOfWork uow,
			Nomenclature nomenclature,
			SupplierOrderingType orderingType,
			AvailabilityForSale[] availabilityForSale,
			bool withDelayOnly
		);
	}
}