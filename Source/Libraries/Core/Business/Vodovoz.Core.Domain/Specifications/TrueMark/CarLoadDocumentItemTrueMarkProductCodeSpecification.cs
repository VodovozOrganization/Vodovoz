using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Specifications.TrueMark
{
	/// <summary>
	/// Спецификации кодов продукта строки талона погрузки
	/// </summary>
	public class CarLoadDocumentItemTrueMarkProductCodeSpecification : ExpressionSpecification<CarLoadDocumentItemTrueMarkProductCode>
	{
		private CarLoadDocumentItemTrueMarkProductCodeSpecification(Expression<Func<CarLoadDocumentItemTrueMarkProductCode, bool>> expression)
			: base(expression)
		{
		}

		/// <summary>
		/// Создает спецификацию для кодов продукта строки талона погрузки
		/// </summary>
		/// <param name="carLoadDocumentItemId">Идентификатор строки талона погрузки</param>
		public static CarLoadDocumentItemTrueMarkProductCodeSpecification CreateForCarLoadDocumentItemId(int carLoadDocumentItemId)
			=> new CarLoadDocumentItemTrueMarkProductCodeSpecification(x => x.CarLoadDocumentItem.Id == carLoadDocumentItemId);

		/// <summary>
		/// Создает спецификацию для кодов продукта нескольких строк талона погрузки
		/// </summary>
		/// <param name="carLoadDocumentItemIds">Идентификаторы строк талона погрузки</param>
		public static CarLoadDocumentItemTrueMarkProductCodeSpecification CreateForCarLoadDocumentItemIds(IEnumerable<int> carLoadDocumentItemIds)
		{
			var ids = carLoadDocumentItemIds.ToList();
			return new CarLoadDocumentItemTrueMarkProductCodeSpecification(x => ids.Contains(x.CarLoadDocumentItem.Id));
		}
	}
}
