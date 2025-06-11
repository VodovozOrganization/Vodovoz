using Gamma.ColumnConfig;
using Gtk;
using QS.Dialog;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Domain.Goods;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Users;
using Vodovoz.ViewWidgets.Users;

namespace Vodovoz.Views.Users
{
	[ToolboxItem(true)]
	public partial class UserSettingsView : TabViewBase<UserSettingsViewModel>
	{
		public UserSettingsView(UserSettingsViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			entityentryWarehouse.ViewModel = ViewModel.WarehouseViewModel;

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

			ViewModel.CounterpartyViewModel = new LegacyEEVMBuilderFactory<UserSettingsViewModel>(Tab, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.LifetimeScope)
				.ForProperty(vm => vm.DefaultCounterparty)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			entryCounterparty.ViewModel = ViewModel.CounterpartyViewModel;

			ycheckbuttonUse.Binding
				.AddBinding(ViewModel.Entity, e => e.UseEmployeeSubdivision, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonHideComplaintsNotifications.Binding
				.AddBinding(ViewModel.Entity, e => e.HideComplaintNotification, w => w.Active)
				.InitializeFromSource();

			frameSortingCashInfo.Visible = ViewModel.UserIsCashier;
			treeViewSubdivisionsToSort.ColumnsConfig = FluentColumnsConfig<CashSubdivisionSortingSettings>.Create()
				.AddColumn("№").AddNumericRenderer(x => x.SortingIndex)
				.AddColumn("Подразделение кассы").AddTextRenderer(x => x.CashSubdivisionId != null ? ViewModel.SubdivisionInMemoryCacheRepository.GetTitleById(x.CashSubdivisionId.Value) : "")
				.Finish();
			treeViewSubdivisionsToSort.EnableGridLines = TreeViewGridLines.Vertical;
			treeViewSubdivisionsToSort.SetItemsSource(ViewModel.SubdivisionSortingSettings);
			treeViewSubdivisionsToSort.DragDataReceived += (o, args) => ViewModel.UpdateIndices();

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			if(ViewModel.IsUserFromOkk)
			{
				complaintsFrame.Sensitive = false;
			}
			else
			{
				entrySubdivision.Sensitive = !ViewModel.Entity.UseEmployeeSubdivision;

				ycheckbuttonUse.Toggled += (sender, e) =>
				{
					var useEmployeeSubdivision = ViewModel.Entity.UseEmployeeSubdivision;
					entrySubdivision.Sensitive = !useEmployeeSubdivision;

					if(useEmployeeSubdivision)
					{
						entrySubdivision.ViewModel.Entity = null;
					}
				};
			}

			#region FuelControlApi

			frameFuelControl.Visible = true;

			yentryFuelApiLogin.Binding
				.AddBinding(ViewModel.Entity, e => e.FuelControlApiLogin, w => w.Text)
				.InitializeFromSource();

			yentryFuelApiPassword.Binding
				.AddBinding(ViewModel.Entity, e => e.FuelControlApiPassword, w => w.Text)
				.InitializeFromSource();

			yentryFuelApiKey.Binding
				.AddBinding(ViewModel.Entity, e => e.FuelControlApiKey, w => w.Text)
				.InitializeFromSource();

			yentrySessionId.Binding
				.AddBinding(ViewModel.Entity, e => e.FuelControlApiSessionId, w => w.Text)
				.InitializeFromSource();

			datepickerFuelApiSessionExpirationDate.Binding
				.AddBinding(ViewModel.Entity, e => e.FuelControlApiSessionExpirationDate, w => w.DateOrNull)
				.InitializeFromSource();

			ybuttonLogin.Binding
				.AddBinding(ViewModel.Entity, e => e.IsUserHasAuthDataForFuelControlApi, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonLogin.BindCommand(ViewModel.FuelControlApiLoginCommand);

			#endregion

			#region Обновление фиксы

			lblUpdateFixedPricesTitle.LabelProp = @"<b>Обновление фиксы 19л воды</b>";
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

			documentsprintersettingsview.ViewModel = ViewModel.DocumentsPrinterSettingsViewModel;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.ProgressMessage):
					Gtk.Application.Invoke((s, args) =>
					{
						updateFixedPricesProgress.Text = ViewModel.ProgressMessage;
					});
					break;
				case nameof(ViewModel.ProgressFraction):
					Gtk.Application.Invoke((s, args) =>
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
					"Обновление может занять больше 5мин"))
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
					Gtk.Application.Invoke((s, eventArgs) =>
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
