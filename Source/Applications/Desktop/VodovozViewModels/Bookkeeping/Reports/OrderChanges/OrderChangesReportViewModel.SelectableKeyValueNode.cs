using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges
{
	public partial class OrderChangesReportViewModel
	{
		public class SelectableKeyValueNode : PropertyChangedBase
		{
			private bool _isSelected;

			public SelectableKeyValueNode(string key, string value, bool isSelected = true)
			{
				Key = key;
				Value = value;
				IsSelected = isSelected;
			}

			public virtual bool IsSelected
			{
				get => _isSelected;
				set => SetField(ref _isSelected, value);
			}

			public string Key { get; set; }

			public string Value { get; set; }
		}
	}
}
