using System.Diagnostics.CodeAnalysis;
using NHibernate.Criterion;
using Vodovoz.Domain.Goods;

namespace WhereIsTheBottle.Models.MainContent.CommonRestrictions
{
	public static class MainContentCommonRestrictions
	{
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		private static readonly Nomenclature nomenclatureAlias = null;
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		private static readonly Nomenclature nomenclatureAlias2 = null;

		/// <summary>
		/// Бутыли, попадающие в актив. Используется алиас: Nomenclature nomenclatureAlias = null
		/// </summary>
		public static Junction NomenclatureRestriction => Restrictions.Conjunction()
			.Add(Restrictions.Where(() => !nomenclatureAlias.IsShabbyBottle))
			.Add(NomenclatureRestrictionWithShabbyBottles);

		/// <summary>
		/// Бутыли, попадающие в актив. Используется алиас: Nomenclature nomenclatureAlias = null
		/// </summary>
		public static Junction NomenclatureRestrictionWithShabbyBottles => Restrictions.Conjunction()
			.Add(Restrictions.Where(() => !nomenclatureAlias.IsDefectiveBottle))
			.Add(Restrictions.Where(() => !nomenclatureAlias.IsArchive))
			.Add(Restrictions.Where(() => !nomenclatureAlias.IsDiler))
			.Add(Restrictions.Where(() => !nomenclatureAlias.IsDisposableTare))
			// Category == Bottle OR (Category == Water && TareVolume == 19L)
			.Add(Restrictions.Disjunction()
				.Add(Restrictions.Where(() => nomenclatureAlias.Category == NomenclatureCategory.bottle))
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water))
					.Add(Restrictions.Where(() => nomenclatureAlias.TareVolume == TareVolume.Vol19L))));

		/// <summary>
		/// Бутыли, попадающие в актив. Используется алиас: Nomenclature nomenclatureAlias2 = null
		/// </summary>
		public static Junction NomenclatureRestriction2 => Restrictions.Conjunction()
			.Add(Restrictions.Where(() => !nomenclatureAlias2.IsShabbyBottle))
			.Add(Restrictions.Where(() => !nomenclatureAlias2.IsDefectiveBottle))
			.Add(Restrictions.Where(() => !nomenclatureAlias2.IsArchive))
			.Add(Restrictions.Where(() => !nomenclatureAlias2.IsDiler))
			.Add(Restrictions.Where(() => !nomenclatureAlias2.IsDisposableTare))
			// Category == Bottle OR (Category == Water && TareVolume == 19L)
			.Add(Restrictions.Disjunction()
				.Add(Restrictions.Where(() => nomenclatureAlias2.Category == NomenclatureCategory.bottle))
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Where(() => nomenclatureAlias2.Category == NomenclatureCategory.water))
					.Add(Restrictions.Where(() => nomenclatureAlias2.TareVolume == TareVolume.Vol19L))));
	}
}
