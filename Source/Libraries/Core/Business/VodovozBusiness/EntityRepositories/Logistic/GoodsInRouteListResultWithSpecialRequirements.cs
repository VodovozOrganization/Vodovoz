﻿using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class GoodsInRouteListResultWithSpecialRequirements
	{
		public int NomenclatureId { get; set; }
		public string NomenclatureName { get; set; }
		public OwnTypes OwnType { get; set; }
		public decimal? ExpireDatePercent { get; set; } = null;
		public decimal Amount { get; set; }
	}
}
