using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class GoodsInRouteListResultToDivide
	{
		public int NomenclatureId { get; set; }
		public decimal Amount { get; set; }
		public decimal? ExpireDatePercent { get; set; } = null;
		public OwnTypes OwnType { get; set; }
		public int? OrderId { get; set; }
	}
}
