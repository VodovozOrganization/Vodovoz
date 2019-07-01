using System;
using QS.Project.Filter;
using QS.Services;

namespace Vodovoz.Filters.ViewModels
{
	public class ResidueFilterViewModel : FilterViewModelBase<ResidueFilterViewModel>
	{
		public ResidueFilterViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
		}

		private DateTime? startDate;
		public virtual DateTime? StartDate {
			get => startDate;
			set {
				if(SetField(ref startDate, value, () => StartDate)) {
					Update();
				}
			}
		}


		private DateTime? endDate;
		public virtual DateTime? EndDate {
			get => endDate;
			set {
				if(SetField(ref endDate, value, () => EndDate)) {
					Update();
				}
			}
		}
	}
}
