using QS.Project.Filter;
using QS.Project.Journal;

namespace Vodovoz.FilterViewModels.Organization
{
	public class SubdivisionFilterViewModel : FilterViewModelBase<SubdivisionFilterViewModel>
	{
		private int[] _excludedSubdivisions;
		private SubdivisionType? _subdivisionType;
		private bool _onlyCashSubdivisions;


		public virtual int[] ExcludedSubdivisions
		{
			get => _excludedSubdivisions;
			set => UpdateFilterField(ref _excludedSubdivisions, value);
		}

		public virtual SubdivisionType? SubdivisionType
		{
			get => _subdivisionType;
			set => UpdateFilterField(ref _subdivisionType, value);
		}

		public virtual bool OnlyCashSubdivisions
		{
			get => _onlyCashSubdivisions;
			set => UpdateFilterField(ref _onlyCashSubdivisions, value);
		}

		public override bool IsShow { get; set; } = true;
	}
}
