using QS.Project.Filter;
using QS.Services;

namespace Vodovoz.FilterViewModels.Organization
{
	public class SubdivisionFilterViewModel : FilterViewModelBase<SubdivisionFilterViewModel>
	{
		public SubdivisionFilterViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
		}

		private int[] excludedSubdivisions;
		public virtual int[] ExcludedSubdivisions {
			get => excludedSubdivisions;
			set => UpdateFilterField(ref excludedSubdivisions, value, () => ExcludedSubdivisions);
		}
	}
}
