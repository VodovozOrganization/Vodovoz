using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Specifications;

namespace VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters.Specifications
{
	/// <summary>
	/// Спецификации для фильтрации параметров для робота Мия
	/// </summary>
	public static class RobotMiaParametersSpecifications
	{
		/// <summary>
		/// Спецификация для фильтрации параметров для робота Мия по идентификатору номенклатуры
		/// </summary>
		/// <param name="nomenclatureId">Идентификатор номенклатуры</param>
		/// <returns></returns>
		public static ExpressionSpecification<RobotMiaParameters> CreateForHasNomenclatureId(int nomenclatureId)
		{
			return new ExpressionSpecification<RobotMiaParameters>(x => x.NomenclatureId == nomenclatureId);
		}

		/// <summary>
		/// Спецификация для фильтрации параметров для робота Мия по идентификаторам номенклатур
		/// </summary>
		/// <param name="nomenclatureIds">Идентификаторы номенклатур</param>
		/// <returns></returns>
		public static ExpressionSpecification<RobotMiaParameters> CreateForHasNomenclatureIds(IEnumerable<int> nomenclatureIds)
		{
			var nomenclatureIdsArray = nomenclatureIds
				.Cast<int?>()
				.ToArray();

			return new ExpressionSpecification<RobotMiaParameters>(x => nomenclatureIdsArray.Contains(x.NomenclatureId));
		}
	}
}
