using CustomerAppsApi.Library.V2.Dto.Goods.Attributes;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Услуги
	/// </summary>
	public class ServiceSaleItemDto : SaleItemDto
	{
		/// <summary>
		/// Атрибуты
		/// </summary>
		public ServiceSaleItemAttributes Attributes { get; set; }
		public override SaleItemType Type => SaleItemType.Service;
	}
}
