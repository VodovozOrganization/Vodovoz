using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum OrderAddressType
	{
		[Display(Name = "Обычная доставка")]
		Delivery,
		[Display(Name = "Сервисное обслуживание")]
		Service,
		[Display(Name = "Сетевой магазин")]
		ChainStore,
		[Display(Name = "Складская логистика")]
		StorageLogistics
	}
}
