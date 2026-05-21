using Gamma.Utilities;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.ViewModels.Reports.DeliveryAnalytics
{
	public class WeekDayNodes: PropertyChangedBase
	{
		private bool selected;
		public virtual bool Selected {
			get => selected;
			set => SetField(ref selected, value);
		}

		private WeekDayName _weekNameNode;
		public virtual WeekDayName WeekNameNode {
			get => _weekNameNode;
			set => SetField(ref _weekNameNode, value);
		}

		public override string ToString()
		{
			return _weekNameNode.GetEnumTitle();
		}
	}
}