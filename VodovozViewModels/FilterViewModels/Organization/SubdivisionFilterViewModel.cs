using QS.Project.Filter;
using QS.Services;

namespace Vodovoz.FilterViewModels.Organization
{
	public class SubdivisionFilterViewModel : FilterViewModelBase<SubdivisionFilterViewModel>
	{
		private int[] excludedSubdivisions;
		public virtual int[] ExcludedSubdivisions {
			get => excludedSubdivisions;
			set => UpdateFilterField(ref excludedSubdivisions, value, () => ExcludedSubdivisions);
		}

		private SubdivisionType? subdivisionType;
		public virtual SubdivisionType? SubdivisionType {
			get => subdivisionType;
			set => UpdateFilterField(ref subdivisionType, value, () => SubdivisionType);
		}
	}
}
