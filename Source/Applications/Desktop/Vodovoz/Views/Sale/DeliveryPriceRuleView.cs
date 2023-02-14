using Vodovoz.Domain.Sale;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Sale;
using Gamma.GtkWidgets;

namespace Vodovoz.Views.Sale
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPriceRuleView : TabViewBase<DeliveryPriceRuleViewModel>
	{
		public DeliveryPriceRuleView(DeliveryPriceRuleViewModel viewModel) : base(viewModel)
		{
			this.Build();
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

			if(ViewModel.Entity.Id > 0)
			{
				treeDistricts.ColumnsConfig = ColumnsConfigFactory.Create<string[]>()
					.AddColumn("Правило используется в районах:").AddTextRenderer(d => d[0])
					.AddColumn("Версия района:").AddTextRenderer(d => d[1])
					.AddColumn("Дата создания версии района:").AddTextRenderer(d => d[2])
					.Finish();

				//var districtItemsWithDistrictSetValues = _districtRuleRepository.GetDistrictNameDistrictSetNameAndCreationDateByDeliveryPriceRule(UoW, Entity);

				//treeDistricts.ItemsDataSource = districtItemsWithDistrictSetValues;

				//if(districtItemsWithDistrictSetValues.Count() > 0)
				//{
				//	vboxRuleName.Sensitive = false;
				//	vboxRuleSettings.Sensitive = false;
				//	buttonSave.Sensitive = false;
				//}
			}

			//yenumcomboPurpose.ItemsEnum = typeof(PhonePurpose);
			//yenumcomboPurpose.Binding.AddBinding(ViewModel, vm => vm.PhonePurpose, w => w.SelectedItem).InitializeFromSource();
			//yenumcomboPurpose.Binding.AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive).InitializeFromSource();
			//yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			//yentryName.Binding.AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive).InitializeFromSource();

			//buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			//buttonSave.Binding.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive);
			//buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };

			//yenumcomboPurpose.ItemsEnum = typeof(PhonePurpose);
			//yenumcomboPurpose.Binding.AddBinding(ViewModel, vm => vm.PhonePurpose, w => w.SelectedItem).InitializeFromSource();
			//yenumcomboPurpose.Binding.AddBinding(ViewModel, vm => vm.CanUpdate, w => w.Sensitive).InitializeFromSource();
		}
	}
}
