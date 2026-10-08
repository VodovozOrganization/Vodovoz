using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Specifications.TrueMark
{
	/// <summary>
	/// Спецификации кодов продукта строки отпуска самовывоза
	/// </summary>
	public class SelfDeliveryDocumentItemTrueMarkProductCodeSpecification : ExpressionSpecification<SelfDeliveryDocumentItemTrueMarkProductCode>
	{
		private SelfDeliveryDocumentItemTrueMarkProductCodeSpecification(Expression<Func<SelfDeliveryDocumentItemTrueMarkProductCode, bool>> expression)
			: base(expression)
		{
		}

		/// <summary>
		/// Создает спецификацию для кодов продукта строки отпуска самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocumentItemId">Идентификатор строки отпуска самовывоза</param>
		public static SelfDeliveryDocumentItemTrueMarkProductCodeSpecification CreateForSelfDeliveryDocumentItemId(int selfDeliveryDocumentItemId)
			=> new SelfDeliveryDocumentItemTrueMarkProductCodeSpecification(x => x.SelfDeliveryDocumentItem.Id == selfDeliveryDocumentItemId);

		/// <summary>
		/// Создает спецификацию для кодов продукта нескольких строк отпуска самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocumentItemIds">Идентификаторы строк отпуска самовывоза</param>
		public static SelfDeliveryDocumentItemTrueMarkProductCodeSpecification CreateForSelfDeliveryDocumentItemIds(IEnumerable<int> selfDeliveryDocumentItemIds)
		{
			var ids = selfDeliveryDocumentItemIds.ToList();
			return new SelfDeliveryDocumentItemTrueMarkProductCodeSpecification(x => ids.Contains(x.SelfDeliveryDocumentItem.Id));
		}
	}
}
