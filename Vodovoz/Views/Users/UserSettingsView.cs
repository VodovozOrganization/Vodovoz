using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Additions.Store;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Users;

namespace Vodovoz.Views.Users
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserSettingsView : TabViewBase<UserSettingsViewModel>
	{
		public UserSettingsView(UserSettingsViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			var storeDocument = new StoreDocumentHelper();
			yentryrefWarehouse.ItemsQuery = storeDocument.GetWarehouseQuery();
			yentryrefWarehouse.Binding.AddBinding(ViewModel.Entity, e => e.DefaultWarehouse, w => w.Subject).InitializeFromSource();

			yenumcomboDefaultCategory.ItemsEnum = typeof(NomenclatureCategory);
			var itemsToHide = Nomenclature.GetAllCategories().Except(Nomenclature.GetCategoriesForSaleToOrder()).Cast<object>().ToArray();
			yenumcomboDefaultCategory.AddEnumToHideList(itemsToHide);
			yenumcomboDefaultCategory.Binding.AddBinding(ViewModel.Entity, e => e.DefaultSaleCategory, w => w.SelectedItemOrNull).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };

			ycheckbuttonDelivery.Binding.AddBinding(ViewModel.Entity, e => e.LogisticDeliveryOrders, w => w.Active).InitializeFromSource();
			ycheckbuttonService.Binding.AddBinding(ViewModel.Entity, e => e.LogisticServiceOrders, w => w.Active).InitializeFromSource();
			ycheckbuttonChainStore.Binding.AddBinding(ViewModel.Entity, e => e.LogisticChainStoreOrders, w => w.Active).InitializeFromSource();

			yenumcomboStatus.ShowSpecialStateAll = true;
			yenumcomboStatus.ItemsEnum = typeof(ComplaintStatuses);
			yenumcomboStatus.Binding.AddBinding(ViewModel.Entity, e => e.DefaultComplaintStatus, w => w.SelectedItemOrNull).InitializeFromSource();

			frame2.Visible = ViewModel.IsUserFromRetail;
			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entryCounterparty.Binding.AddBinding(ViewModel.Entity, e => e.DefaultCounterparty, w => w.Subject).InitializeFromSource();

			ycheckbuttonUse.Binding.AddBinding(ViewModel.Entity, e => e.UseEmployeeSubdivision, w => w.Active).InitializeFromSource();

			frameSortingCashInfo.Visible = ViewModel.UserIsCashier;
			treeViewSubdivisionsToSort.ColumnsConfig = FluentColumnsConfig<CashSubdivisionSortingSettings>.Create()
				.AddColumn("№").AddNumericRenderer(x => x.SortingIndex)
				.AddColumn("Подразделение кассы").AddTextRenderer(x => x.CashSubdivision.Name)
				.Finish();
			treeViewSubdivisionsToSort.EnableGridLines = TreeViewGridLines.Vertical;
			treeViewSubdivisionsToSort.SetItemsSource(ViewModel.SubdivisionSortingSettings);
			treeViewSubdivisionsToSort.DragDataReceived += (o, args) => ViewModel.UpdateIndices();

			if (ViewModel.IsUserFromOkk)
			{
				complaintsFrame.Sensitive = false;
			}
			else
			{
				yentrySubdivision.Sensitive = !ViewModel.Entity.UseEmployeeSubdivision;

				ycheckbuttonUse.Toggled += (sender, e) =>
				{
					var useEmployeeSubdivision = ViewModel.Entity.UseEmployeeSubdivision;
					yentrySubdivision.Sensitive = !useEmployeeSubdivision;

					if (useEmployeeSubdivision)
					{
						yentrySubdivision.Subject = null;
					}

				};

				yentrySubdivision.SetEntityAutocompleteSelectorFactory(ViewModel.SubdivisionSelectorDefaultFactory);
				yentrySubdivision.Binding.AddBinding(ViewModel.Entity, s => s.DefaultSubdivision, w => w.Subject).InitializeFromSource();
			}
		}
	}
}
