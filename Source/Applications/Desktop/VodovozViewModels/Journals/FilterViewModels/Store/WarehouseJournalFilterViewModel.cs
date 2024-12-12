using System.Collections.Generic;
using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarehouseJournalFilterViewModel : FilterViewModelBase<WarehouseJournalFilterViewModel> 
	{
		private IEnumerable<int> _includeWarehouseIds;
		private int[] _excludeWarehousesIds;
		private bool _ignorePermissions;

		public IEnumerable<int> IncludeWarehouseIds
		{
			get => _includeWarehouseIds;
			set => UpdateFilterField(ref _includeWarehouseIds, value);
		}

		public int[] ExcludeWarehousesIds
		{
			get => _excludeWarehousesIds;
			set => UpdateFilterField(ref _excludeWarehousesIds, value);
		}

		public override bool IsShow { get; set; } = false;

		public bool IgnorePermissions
		{
			get => _ignorePermissions;
			set => UpdateFilterField(ref _ignorePermissions, value);
		}
	}
}
