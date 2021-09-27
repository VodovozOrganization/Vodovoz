using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	public partial class NomenclaturePurchasePriceView : TabViewBase<NomenclaturePurchasePriceViewModel>
	{
		public NomenclaturePurchasePriceView(NomenclaturePurchasePriceViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ylblNomenclature.LabelProp = ViewModel.Entity.Nomenclature.Name;
			yspinbtnPurchasePrice.Binding
				.AddBinding(ViewModel.Entity,e => e.PurchasePrice, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Clicked += (sender, e) => ViewModel.Save();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}
	}
}
