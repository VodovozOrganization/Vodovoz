using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Domain.Goods;

namespace Vodovoz.NHibernateProjections.Goods
{
	/// <summary>
	/// Проекции для экземпляров номенклатур
	/// </summary>
	public static class InventoryNomenclatureInstanceProjections
	{
		/// <summary>
		/// Проекция получения инвентарного номера экземпляра
		/// для запросов, где экземпляр головная сущность
		/// </summary>
		/// <returns></returns>
		public static IProjection InventoryNumberForRootProjection()
		{
			return Projections.Conditional(
				Restrictions.Where<InventoryNomenclatureInstance>(ini => ini.IsUsed),
				CustomProjections.Concat(
					Projections.Constant("Б/У - "),
					Projections.Property<InventoryNomenclatureInstance>(ini => ini.InventoryNumber)),
				Projections.Property<InventoryNomenclatureInstance>(ini => ini.InventoryNumber)
			);
		}
		
		/// <summary>
		/// Проекция получения инвентарного номера экземпляра
		/// для запросов, где экземпляр не головная сущность и т.к. составление запроса идет через рефлексию,
		/// то имя алиаса должно совпадать с основным запросом
		/// </summary>
		/// <returns></returns>
		public static IProjection InventoryNumberProjection()
		{
			InventoryNomenclatureInstance instanceAlias = null;
			
			return Projections.Conditional(
				Restrictions.Where(() => instanceAlias.IsUsed),
				CustomProjections.Concat(
					Projections.Constant("Б/У - "),
					Projections.Property(() => instanceAlias.InventoryNumber)),
				Projections.Property(() => instanceAlias.InventoryNumber)
			);
		}
	}
}
