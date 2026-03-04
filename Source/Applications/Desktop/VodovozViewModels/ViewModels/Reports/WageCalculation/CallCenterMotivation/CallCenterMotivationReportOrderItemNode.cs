using System;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Reports.WageCalculation.CallCenterMotivation
{
	/// <summary>
	/// Нода отчёта по мотивации
	/// </summary>
	public class CallCenterMotivationReportOrderItemNode
	{
		public int Id { get; set; }

		public int NomenclatureId { get; set; }

		public string NomenclatureOfficialName { get; set; }

		public DateTime? OrderDeliveryDate { get; set; }

		public int ProductGroupId { get; set; }

		public string ProductGroupName { get; set; }

		public decimal? ActualCount { get; set; }

		public decimal Count { get; set; }

		public decimal Price { get; set; }

		public decimal ActualSum { get; set; }

		public int OrderAuthorId { get; set; }

		public string OrderAuthorName { get; set; }

		public NomenclatureMotivationUnitType? MotivationUnitType { get; set; }

		public decimal? MotivationCoefficient { get; set; }
	}
}
