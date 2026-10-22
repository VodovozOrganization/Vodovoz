using Comparer;
using QS.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.Edo
{
	public class UpdCompareViewModel : ViewModelBase
	{
		public UpdCompareViewModel(PropertyDifference propertyDifference, bool onlyDiff)
		{
			HasChanges = propertyDifference.HasDiff;
			Name = propertyDifference.DisplayName;
			CurrentValue = propertyDifference.FormattedSecondValue; 
			SentValue = propertyDifference.FormattedFirstValue;

			ChildNodes = propertyDifference.ChildDifferences
				.Where(x => onlyDiff ? x.HasDiff : true)
				.Select(x => new UpdCompareViewModel(x, onlyDiff))
				.ToList();

			ChildNodes.ForEach(x => x.ParentNode = this);
		}

		public bool HasChanges { get; set; }
		public string Name { get; set; }
		public string CurrentValue { get; set; }
		public string SentValue { get; set; }
		public UpdCompareViewModel ParentNode { get; set; }
		public List<UpdCompareViewModel> ChildNodes { get; set; } = new List<UpdCompareViewModel>();
	}
}
