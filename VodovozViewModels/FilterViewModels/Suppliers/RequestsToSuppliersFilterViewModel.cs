using System;
using QS.Project.Filter;
using QS.Services;
using Vodovoz.Domain.Goods;

namespace Vodovoz.FilterViewModels.Suppliers
{
	public class RequestsToSuppliersFilterViewModel : FilterViewModelBase<RequestsToSuppliersFilterViewModel>
	{
		public RequestsToSuppliersFilterViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
		}

		private DateTime? restrictStartDate;
		public virtual DateTime? RestrictStartDate {
			get => restrictStartDate;
			set {
				if(UpdateFilterField(ref restrictStartDate, value))
					CanChangeStartDate = false;
			}
		}
		public bool CanChangeStartDate { get; private set; } = true;

		private DateTime? restrictEndDate;
		public virtual DateTime? RestrictEndDate {
			get => restrictEndDate;
			set {
				if(UpdateFilterField(ref restrictEndDate, value))
					CanChangeEndDate = false;
			}
		}
		public bool CanChangeEndDate { get; private set; } = true;

		Nomenclature restrictNomenclature;
		public virtual Nomenclature RestrictNomenclature {
			get => restrictNomenclature;
			set {
				if(UpdateFilterField(ref restrictNomenclature, value))
					CanChangeNomenclature = false;
			}
		}
		public bool CanChangeNomenclature { get; private set; } = true;
	}
}