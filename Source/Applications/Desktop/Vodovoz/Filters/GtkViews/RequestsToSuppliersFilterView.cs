using QS.Views.GtkUI;
using Vodovoz.Domain.Suppliers;
using Vodovoz.FilterViewModels.Suppliers;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RequestsToSuppliersFilterView : FilterViewBase<RequestsToSuppliersFilterViewModel>
	{
		public RequestsToSuppliersFilterView(RequestsToSuppliersFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
			InitializeRestrictions();
		}

		void Configure()
		{
			dPerCreatedDate.Binding.AddBinding(ViewModel, x => x.RestrictStartDate, x => x.StartDateOrNull).InitializeFromSource();
			dPerCreatedDate.Binding.AddBinding(ViewModel, x => x.RestrictEndDate, x => x.EndDateOrNull).InitializeFromSource();
			//entVMEntNomenclature.SetEntitySelectorFactory(ViewModel.NomenclatureSelectorFactory);
			//entVMEntNomenclature.Binding.AddBinding(ViewModel, x => x.RestrictNomenclature, x => x.Subject).InitializeFromSource();
			enumStatus.ItemsEnum = typeof(RequestStatus);
			enumStatus.Binding.AddBinding(ViewModel, vm => vm.RestrictStatus, w => w.SelectedItemOrNull).InitializeFromSource();
		}

		void InitializeRestrictions()
		{
			dPerCreatedDate.Sensitive = ViewModel.CanChangeEndDate && ViewModel.CanChangeStartDate;
			entVMEntNomenclature.Sensitive = ViewModel.CanChangeNomenclature;
			enumStatus.Sensitive = ViewModel.CanChangeStatus;
		}
	}
}
