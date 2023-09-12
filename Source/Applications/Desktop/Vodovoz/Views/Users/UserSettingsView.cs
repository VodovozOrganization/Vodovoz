using Gamma.ColumnConfig;
using Gtk;
using QS.Dialog;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Users;
using Vodovoz.ViewWidgets.Users;

namespace Vodovoz.Views.Users
{
	[ToolboxItem(true)]
	public partial class UserSettingsView : TabViewBase<UserSettingsViewModel>
	{
		public UserSettingsView(UserSettingsViewModel viewModel) : base(viewModel) {
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetNotArchiveWarehousesQuery();
			yentryrefWarehouse.Binding
				.AddBinding(ViewModel.Entity, e => e.DefaultWarehouse, w => w.Subject)
				.InitializeFromSource();

			yenumcomboDefaultCategory.ItemsEnum = typeof(NomenclatureCategory);
			var itemsToHide = Nomenclature.GetAllCategories().Except(Nomenclature.GetCategoriesForSaleToOrder()).Cast<object>().ToArray();
			yenumcomboDefaultCategory.AddEnumToHideList(itemsToHide);
			yenumcomboDefaultCategory.Binding
				.AddBinding(ViewModel.Entity, e => e.DefaultSaleCategory, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonSave.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsFixedPricesUpdating, w => w.Sensitive)
				.InitializeFromSource();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
			buttonCancel.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsFixedPricesUpdating, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonDelivery.Binding
				.AddBinding(ViewModel.Entity, e => e.LogisticDeliveryOrders, w => w.Active)
				.InitializeFromSource();
			ycheckbuttonService.Binding
				.AddBinding(ViewModel.Entity, e => e.LogisticServiceOrders, w => w.Active)
				.InitializeFromSource();
			ycheckbuttonChainStore.Binding
				.AddBinding(ViewModel.Entity, e => e.LogisticChainStoreOrders, w => w.Active)
				.InitializeFromSource();

			yenumcomboStatus.ShowSpecialStateAll = true;
			yenumcomboStatus.ItemsEnum = typeof(ComplaintStatuses);
			yenumcomboStatus.Binding
				.AddBinding(ViewModel.Entity, e => e.DefaultComplaintStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			frame2.Visible = ViewModel.IsUserFromRetail;
			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entryCounterparty.Binding
				.AddBinding(ViewModel.Entity, e => e.DefaultCounterparty, w => w.Subject)
				.InitializeFromSource();

			ycheckbuttonUse.Binding
				.AddBinding(ViewModel.Entity, e => e.UseEmployeeSubdivision, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonHideComplaintsNotifications.Binding
				.AddBinding(ViewModel.Entity, e => e.HideComplaintNotification, w => w.Active)
				.InitializeFromSource();

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
				yentrySubdivision.Binding
					.AddBinding(ViewModel.Entity, s => s.DefaultSubdivision, w => w.Subject)
					.InitializeFromSource();
			}

			#region Обновление фиксы

			btnUpdateFixedPrices.Clicked += (sender, args) => UpdateFixedPrices();
			btnUpdateFixedPrices.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanUpdateFixedPrices && !vm.IsFixedPricesUpdating, w => w.Sensitive)
				.InitializeFromSource();
			
			spinBtnIncrementFixedPrices.Binding
				.AddBinding(ViewModel, vm => vm.IncrementFixedPrices, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			#endregion

			var warehousesUserSelectionView = new WarehousesUserSelectionView(ViewModel.WarehousesUserSelectionViewModel);
			yhboxWarehousesForNotifications.Add(warehousesUserSelectionView);
			warehousesUserSelectionView.Show();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.ProgressMessage):
					Application.Invoke((s, args) =>
					{
						updateFixedPricesProgress.Text = ViewModel.ProgressMessage;
					});
					break;
				case nameof(ViewModel.ProgressFraction):
					Application.Invoke((s, args) =>
					{
						updateFixedPricesProgress.Fraction = ViewModel.ProgressFraction;
					});
					break;
			}
		}

		private async void UpdateFixedPrices()
		{
			if(ViewModel.IncrementFixedPrices == 0)
			{
				ViewModel.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Нельзя увеличить фиксу на 0 руб");
				return;
			}
			if(!ViewModel.InteractiveService.Question(
				"Вы уверены, что хотите обновить всю фиксу контрагентов и точек доставки для 19л воды?\n" +
					"Обновление займет больше 5мин"))
			{
				return;
			}
			await Task.Run(() =>
			{
				try
				{
					ViewModel.UpdateFixedPricesCommand.Execute();
				}
				catch(Exception ex)
				{
					Application.Invoke((s, eventArgs) =>
					{
						updateFixedPricesProgress.Text = "При обновлении фиксы произошла ошибка. Попробуйте повторить позже...";
						updateFixedPricesProgress.Fraction = 0;
						throw ex;
					});
				}
			});
		}
		
		public override void Destroy()
		{
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			base.Destroy();
		}
	}
}
