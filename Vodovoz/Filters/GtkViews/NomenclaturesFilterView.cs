using QS.Views.GtkUI;
using Vodovoz.Domain.Goods;
using Vodovoz.FilterViewModels.Goods;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclaturesFilterView : FilterViewBase<NomenclatureFilterViewModel>
	{
		public NomenclaturesFilterView(NomenclatureFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
			InitializeRestrictions();
		}

		void Configure()
		{
			enumcomboCategory.ItemsEnum = typeof(NomenclatureCategory);
			enumcomboCategory.Binding.AddBinding(ViewModel, s => s.RestrictCategory, w => w.SelectedItemOrNull).InitializeFromSource();

			cmbSaleCategory.ItemsEnum = typeof(SaleCategory);
			cmbSaleCategory.Binding.AddBinding(ViewModel, s => s.RestrictSaleCategory, w => w.SelectedItemOrNull).InitializeFromSource();
			//cmbSaleCategory.Binding.AddBinding(ViewModel, s => s.CanChangeSaleCategory, w => w.Sensitive).InitializeFromSource();
			cmbSaleCategory.Binding.AddBinding(ViewModel, s => s.IsSaleCategoryApplicable, w => w.Visible).InitializeFromSource();

			chkShowDilers.Binding.AddBinding(ViewModel, s => s.RestrictDilers, w => w.Active).InitializeFromSource();
			//chkShowDilers.Binding.AddBinding(ViewModel, s => s.CanChangeShowDilers, w => w.Sensitive).InitializeFromSource();
			chkShowDilers.Binding.AddBinding(ViewModel, s => s.AreDilersApplicable, w => w.Visible).InitializeFromSource();

			chkOnlyDisposableTare.Binding.AddBinding(ViewModel, s => s.RestrictDisposbleTare, w => w.Active).InitializeFromSource();
			//chkOnlyDisposableTare.Binding.AddBinding(ViewModel, s => s.CanChangeShowDisposableTare, w => w.Sensitive).InitializeFromSource();
			chkOnlyDisposableTare.Binding.AddBinding(ViewModel, s => s.IsDispossableTareApplicable, w => w.Visible).InitializeFromSource();
		}

		void InitializeRestrictions()
		{
			enumcomboCategory.Sensitive = ViewModel.CanChangeCategory;
			cmbSaleCategory.Sensitive = ViewModel.CanChangeSaleCategory;
			chkShowDilers.Sensitive = ViewModel.CanChangeShowDilers;
			chkOnlyDisposableTare.Sensitive = ViewModel.CanChangeShowDisposableTare;
		}
	}
}
