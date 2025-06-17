using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Goods;

namespace VodovozBusiness.Domain.Goods.Specifications
{
	/// <summary>
	/// Спецификации для фильтрации номенклатур
	/// </summary>
	public static class NomenclatureSpecifications
	{
		/// <summary>
		/// Спецификация для фильтрации номенклатур по идентификатору
		/// </summary>
		/// <param name="id">Идентификатор номенклатуры</param>
		/// <returns></returns>
		public static ExpressionSpecification<Nomenclature> CreateForId(int id)
			=> new ExpressionSpecification<Nomenclature>(n => n.Id == id);

		/// <summary>
		/// Спецификация для фильтрации номенклатур по идентификаторам
		/// </summary>
		/// <param name="ids">Идентификаторы номенклатур</param>
		/// <returns></returns>
		public static ExpressionSpecification<Nomenclature> CreateForIds(IEnumerable<int> ids)
		{
			var idsArray = ids.ToArray();
			return new ExpressionSpecification<Nomenclature>(n => idsArray.Contains(n.Id));
		}

		/// <summary>
		/// Спецификация для фильтрации номенклатур, которые не являются архивными
		/// </summary>
		/// <returns></returns>
		public static ExpressionSpecification<Nomenclature> CreateForIsNotArchive()
			=> new ExpressionSpecification<Nomenclature>(n => !n.IsArchive);

		/// <summary>
		/// Спецификация для фильтрации архивных номенклатур
		/// </summary>
		/// <returns></returns>
		public static ExpressionSpecification<Nomenclature> CreateForIsArchive()
			=> new ExpressionSpecification<Nomenclature>(n => n.IsArchive);

		/// <summary>
		/// Спецификация для фильтрации номенклатур, которые не являются архивными и имеют указанный идентификатор
		/// </summary>
		/// <param name="id">Идентификатор номенклатуры</param>
		/// <returns></returns>
		public static ExpressionSpecification<Nomenclature> CreateForIsNotArchiveAndId(int id)
			=> CreateForIsNotArchive()
				& CreateForId(id);
	}
}
