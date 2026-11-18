using System.Reflection;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	internal static class OrderItemNomenclatureAccess
	{
		private static readonly FieldInfo BaseNomenclatureField = typeof(OrderItemEntity)
			.GetField("_nomenclature", BindingFlags.Instance | BindingFlags.NonPublic);

		public static NomenclatureEntity Of(OrderItemEntity item)
		{
			if(item == null)
			{
				return null;
			}

			if(item is OrderItem domainItem && domainItem.Nomenclature != null)
			{
				if(item.Nomenclature == null)
				{
					BaseNomenclatureField?.SetValue(item, domainItem.Nomenclature);
				}

				return domainItem.Nomenclature;
			}

			return item.Nomenclature;
		}
	}
}
