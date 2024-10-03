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
		/// </summary>
		/// <returns></returns>
		public static IProjection InventoryNumberProjection()
		{
			return Projections.Conditional(
				Restrictions.Where<InventoryNomenclatureInstance>(ini => ini.IsUsed),
				CustomProjections.Concat(
					Projections.Constant("Б/У - "),
					Projections.Property<InventoryNomenclatureInstance>(ini => ini.InventoryNumber)),
				Projections.Property<InventoryNomenclatureInstance>(ini => ini.InventoryNumber)
			);
		}
	}
}
