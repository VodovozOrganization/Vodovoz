using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.ViewModels.ViewModels.Sale;

namespace Vodovoz.Views.Sale
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPriceRuleView : TabViewBase<DeliveryPriceRuleViewModel>
	{
		public DeliveryPriceRuleView(DeliveryPriceRuleViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			spinOrderMinSumEShopGoods.Visible = false;
			lblOrderMinSumEShopGoods.Visible = false;
			spin600mlQty.Visible = false;
			lbl600mlQty.Visible = false;

			spin19LQty.Binding.AddBinding(ViewModel.Entity, e => e.Water19LCount, w => w.ValueAsInt).InitializeFromSource();
			spin6LQty.Binding.AddBinding(ViewModel.Entity, e => e.Water6LCount, w => w.ValueAsInt).InitializeFromSource();
			spin1500mlQty.Binding.AddBinding(ViewModel.Entity, e => e.Water1500mlCount, w => w.ValueAsInt).InitializeFromSource();
			spin600mlQty.Binding.AddBinding(ViewModel.Entity, e => e.Water600mlCount, w => w.ValueAsInt).InitializeFromSource();
			spin500mlQty.Binding.AddBinding(ViewModel.Entity, e => e.Water500mlCount, w => w.ValueAsInt).InitializeFromSource();
			spinOrderMinSumEShopGoods.Binding.AddBinding(ViewModel.Entity, e => e.OrderMinSumEShopGoods, w => w.ValueAsDecimal).InitializeFromSource();
			ytextviewRuleName.Binding.AddBinding(ViewModel.Entity, e => e.RuleName, w => w.Buffer.Text).InitializeFromSource();
			vboxDistricts.Visible = ViewModel.Entity.Id > 0;

			treeDistricts.ColumnsConfig = ColumnsConfigFactory.Create<DistrictAndDistrictSet>()
				.AddColumn("Правило используется в районах:").AddTextRenderer(d => d.DistrictName)
				.AddColumn("Версия района:").AddTextRenderer(d => d.DistrictSetName)
				.AddColumn("Дата создания версии района:").AddTextRenderer(d => d.DistrictSetCreationDate.ToShortDateString())
				.Finish();
			treeDistricts.ItemsDataSource = ViewModel.DistrictsHavingCurrentRule;

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonSave.Sensitive = ViewModel.CanCreateOrUpdate;
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };

			vboxRuleName.Sensitive = ViewModel.CanCreateOrUpdate;
			vboxRuleSettings.Sensitive = ViewModel.CanCreateOrUpdate;
		}
	}
}
