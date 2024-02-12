using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Suppliers;
using Vodovoz.FilterViewModels.Suppliers;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class RequestsToSuppliersFilterView : FilterViewBase<RequestsToSuppliersFilterViewModel>
	{
		public RequestsToSuppliersFilterView(RequestsToSuppliersFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Configure();
			InitializeRestrictions();
		}

		private void Configure()
		{
			dPerCreatedDate.Binding.AddBinding(ViewModel, x => x.RestrictStartDate, x => x.StartDateOrNull).InitializeFromSource();
			dPerCreatedDate.Binding.AddBinding(ViewModel, x => x.RestrictEndDate, x => x.EndDateOrNull).InitializeFromSource();

			entryNomenclature.ViewModel = ViewModel.NomenclatureViewModel;

			enumStatus.ItemsEnum = typeof(RequestStatus);
			enumStatus.Binding.AddBinding(ViewModel, vm => vm.RestrictStatus, w => w.SelectedItemOrNull).InitializeFromSource();
		}

		private void InitializeRestrictions()
		{
			dPerCreatedDate.Sensitive = ViewModel.CanChangeEndDate && ViewModel.CanChangeStartDate;
			entryNomenclature.Sensitive = ViewModel.CanChangeNomenclature;
			enumStatus.Sensitive = ViewModel.CanChangeStatus;
		}
	}
}
