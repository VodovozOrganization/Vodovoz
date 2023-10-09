using QS.Project.Filter;
using System;

namespace Vodovoz.FilterViewModels.Organization
{
	public class SubdivisionFilterViewModel : FilterViewModelBase<SubdivisionFilterViewModel>
	{
		private int[] _excludedSubdivisionsIds = Array.Empty<int>();
		private SubdivisionType? _subdivisionType;
		private bool _onlyCashSubdivisions;
		private bool _showArchieved = false;

		public int[] ExcludedSubdivisionsIds
		{
			get => _excludedSubdivisionsIds;
			set => UpdateFilterField(ref _excludedSubdivisionsIds, value);
		}

		public SubdivisionType? SubdivisionType
		{
			get => _subdivisionType;
			set => UpdateFilterField(ref _subdivisionType, value);
		}

		public bool OnlyCashSubdivisions
		{
			get => _onlyCashSubdivisions;
			set => UpdateFilterField(ref _onlyCashSubdivisions, value);
		}

		public bool ShowArchieved
		{
			get => _showArchieved;
			set => UpdateFilterField(ref _showArchieved, value);
		}

		public override bool IsShow { get; set; } = true;

		public string SearchString { get; internal set; }
	}
}
