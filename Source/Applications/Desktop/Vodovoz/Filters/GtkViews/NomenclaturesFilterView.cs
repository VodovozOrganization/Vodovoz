using Gamma.Utilities;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class NomenclaturesFilterView : FilterViewBase<NomenclatureFilterViewModel>
	{
		public NomenclaturesFilterView(NomenclatureFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Configure();
			InitializeRestrictions();
		}

		void Configure()
		{
			lstCategory.ItemsList = ViewModel.AvailableCategories;
			lstCategory.SetRenderTextFunc<NomenclatureCategory>(x => x.GetEnumTitle());
			lstCategory.Binding
				.AddBinding(ViewModel, s => s.RestrictCategory, w => w.SelectedItem)
				.InitializeFromSource();

			lstSaleCategory.ItemsList = ViewModel.AvailableSalesCategories;
			lstSaleCategory.SetRenderTextFunc<SaleCategory>(x => x.GetEnumTitle());
			lstSaleCategory.Binding
				.AddBinding(ViewModel, s => s.RestrictSaleCategory, w => w.SelectedItem)
				.AddBinding(ViewModel, s => s.IsSaleCategoryApplicable, w => w.Visible)
				.InitializeFromSource();

			chkShowDilers.Binding
				.AddBinding(ViewModel, s => s.RestrictDilers, w => w.Active)
				.AddBinding(ViewModel, s => s.AreDilersApplicable, w => w.Visible)
				.InitializeFromSource();

			chkOnlyDisposableTare.Binding
				.AddBinding(ViewModel, s => s.RestrictDisposbleTare, w => w.Active)
				.AddBinding(ViewModel, s => s.IsDispossableTareApplicable, w => w.Visible)
				.InitializeFromSource();

			ViewModel.RestrictArchive = false;
			chkShowArchive.Binding.AddBinding(ViewModel, vm => vm.RestrictArchive, w => w.Active).InitializeFromSource();

			yenumcomboboxAdditionalInfo.ItemsEnum = typeof(GlassHolderType);
			yenumcomboboxAdditionalInfo.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.GlassHolderType, w => w.SelectedItemOrNull)
				.AddFuncBinding(vm => vm.RestrictCategory == null || vm.RestrictCategory == NomenclatureCategory.equipment, w => w.Sensitive)
				.InitializeFromSource();

			chkOnlyOnlineNomenclatures.Binding
				.AddBinding(ViewModel, vm => vm.OnlyOnlineNomenclatures, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanChangeOnlyOnlineNomenclatures, w => w.Sensitive)
				.InitializeFromSource();
		}

		void InitializeRestrictions()
		{
			lstCategory.Sensitive = ViewModel.CanChangeCategory;
			lstSaleCategory.Sensitive = ViewModel.CanChangeSaleCategory;
			chkShowDilers.Sensitive = ViewModel.CanChangeShowDilers;
			chkOnlyDisposableTare.Sensitive = ViewModel.CanChangeShowDisposableTare;
			chkShowArchive.Sensitive = ViewModel.CanChangeShowArchive;
		}
	}
}
