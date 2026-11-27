using QS.Project.Filter;
using System;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.FilterViewModels.Organization
{
	public class SubdivisionFilterViewModel : FilterViewModelBase<SubdivisionFilterViewModel>
	{
		private int[] _excludedSubdivisionsIds = Array.Empty<int>();
		private int[] _includedSubdivisionsIds = Array.Empty<int>();
		private SubdivisionType? _subdivisionType;
		private bool _onlyCashSubdivisions;
		private bool _showArchieved = false;
		private int? _restrictParentId;
		private SubdivisionLevel[] _restrictLevels = Array.Empty<SubdivisionLevel>();
		private SubdivisionLevel[] _restrictSelectLevels = Array.Empty<SubdivisionLevel>();

		public int[] ExcludedSubdivisionsIds
		{
			get => _excludedSubdivisionsIds;
			set => UpdateFilterField(ref _excludedSubdivisionsIds, value);
		}

		public int[] IncludedSubdivisionsIds
		{
			get => _includedSubdivisionsIds;
			set => UpdateFilterField(ref _includedSubdivisionsIds, value);
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

		public int? RestrictParentId
		{
			get => _restrictParentId;
			set => UpdateFilterField(ref _restrictParentId, value);
		}

		/// <summary>
		/// Уровни, которые показываются в журнале, пусто - все
		/// </summary>
		public SubdivisionLevel[] RestrictLevels
		{
			get => _restrictLevels;
			set => UpdateFilterField(ref _restrictLevels, value);
		}

		/// <summary>
		/// Уровни, которые можно выбрать, пусто - любые
		/// </summary>
		public SubdivisionLevel[] RestrictSelectLevels
		{
			get => _restrictSelectLevels;
			set => UpdateFilterField(ref _restrictSelectLevels, value);
		}

		public override bool IsShow { get; set; } = true;

		public string SearchString { get; internal set; }
	}
}
