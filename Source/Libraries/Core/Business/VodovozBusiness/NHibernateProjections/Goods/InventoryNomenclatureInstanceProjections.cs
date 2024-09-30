using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Domain.Goods;

namespace Vodovoz.NHibernateProjections.Goods
{
	public static class InventoryNomenclatureInstanceProjections
	{
		public static IProjection InventoryNumberProjection(InventoryNomenclatureInstance instanceAlias)
		{
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
