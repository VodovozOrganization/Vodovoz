using Gamma.ColumnConfig;
using Pango;
using QS.Views.GtkUI;
using QSProjectsLib;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Logistic.DriversStopLists;
using static Vodovoz.ViewModels.Logistic.DriversStopLists.DriversStopListsViewModel;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriversStopListsView : TabViewBase<DriversStopListsViewModel>
	{
		public DriversStopListsView(DriversStopListsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			if(ViewModel == null)
			{
				return;
			}

			yvboxMain.Visible = ViewModel.DialogVisibility;

			entityentrySubdivision.ViewModel = ViewModel.FilterSubdivisionEntityEntryViewModel;

			yenumcomboStatus.ItemsEnum = typeof(EmployeeStatus);
			yenumcomboStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.FilterEmployeeStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumcomboCarTypeOfUse.ItemsEnum = typeof(CarTypeOfUse);
			yenumcomboCarTypeOfUse.AddEnumToHideList(CarTypeOfUse.Loader);
			yenumcomboCarTypeOfUse.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.FilterCarTypeOfUse, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumcomboCarOwnType.ItemsEnum = typeof(CarOwnType);
			yenumcomboCarOwnType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.FilterCarOwnType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumcomboDriversSortOrder.ItemsEnum = typeof(DriversSortOrder);
			yenumcomboDriversSortOrder.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CurrentDriversListSortOrder, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ycheckbuttonExcludeVisitingMasters.Binding
				.AddBinding(ViewModel, vm => vm.IsExcludeVisitingMasters, w => w.Active)
				.InitializeFromSource();

			yhboxFilter.Binding
				.AddBinding(ViewModel, wm => wm.FilterVisibility, w => w.Visible)
				.InitializeFromSource();

			ytreeviewCurrent.ColumnsConfig = FluentColumnsConfig<DriverNode>
				.Create()
					.AddColumn("Водитель").AddTextRenderer(x => x.DriverFullName)
					.AddColumn("Автомобиль").AddTextRenderer(x => x.CarRegistrationNumber)
					.AddColumn("Общий долг по МЛ").AddTextRenderer(x => x.RouteListsDebtsSum.ToShortCurrencyString())
					.AddColumn("Кол-во незакрытых МЛ").AddTextRenderer(x => x.UnclosedRouteListsWithDebtCount.ToString())
					.AddColumn("Стоп-лист").AddToggleRenderer(d => d.IsDriverInStopList).Editing(false)
					.AddColumn("")
				.Finish();

			ytreeviewCurrent.Binding.AddBinding(ViewModel, vm => vm.CurrentDriversList, w => w.ItemsDataSource).InitializeFromSource();
			ytreeviewCurrent.Binding.AddBinding(ViewModel, vm => vm.SelectedDriverNode, w => w.SelectedRow).InitializeFromSource();
			ytreeviewCurrent.RowActivated += (sender, e) => ViewModel.RemoveStopListCommand?.Execute();

			ytreeviewHystory.ColumnsConfig = FluentColumnsConfig<DriverStopListRemoval>
				.Create()
					.AddColumn("Водитель").AddTextRenderer(x => x.Driver.FullName)
					.AddColumn("Снят с").AddTextRenderer(x => x.DateFrom.ToString("dd.MM.yyyy HH:mm"))
					.AddColumn("Снят по").AddTextRenderer(x => x.DateTo.ToString("dd.MM.yyyy HH:mm"))
					.AddColumn("Комментарий")
						.AddTextRenderer(x => x.Comment)
						.WrapWidth(330)
						.WrapMode(WrapMode.WordChar)
					.AddColumn("ФИО снявшего").AddTextRenderer(x => x.Author.FullName)
				.Finish();

			ytreeviewHystory.Binding.AddBinding(ViewModel, vm => vm.StopListsRemovalHistory, w => w.ItemsDataSource).InitializeFromSource();

			ybuttonRemoveStopList.Binding
				.AddBinding(ViewModel, vm => ViewModel.CanCreateStopListRemoval, v => v.Sensitive)
				.InitializeFromSource();

			ybuttonRemoveStopList.Clicked += (s, e) => ViewModel.RemoveStopListCommand?.Execute();
			ybuttonFilter.Clicked += (s, e) => ViewModel.CloseFilterCommand?.Execute();
			ybuttonRefresh.Clicked += (s, e) => ViewModel.UpdateCommand?.Execute();
		}

		public override void Destroy()
		{
			ytreeviewCurrent?.Destroy();
			ytreeviewHystory?.Destroy();

			base.Destroy();
		}
	}
}
