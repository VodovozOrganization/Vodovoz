using Gamma.ColumnConfig;
using Gamma.Widgets.Additions;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListJournalFilterView : FilterViewBase<RouteListJournalFilterViewModel>
	{
		public RouteListJournalFilterView(RouteListJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			ConfigureFilter();
		}

		private void ConfigureFilter()
		{
			ytreeviewRouteListStatuses.ColumnsConfig = FluentColumnsConfig<RouteListStatusNode>.Create()
				.AddColumn("Статус").AddTextRenderer(x => x.Title)
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.Finish();

			ytreeviewRouteListStatuses.ItemsDataSource = ViewModel.StatusNodes;

			ytreeviewRouteListStatuses.Binding.AddBinding(ViewModel, vm => vm.CanSelectStatuses, w => w.Sensitive).InitializeFromSource();

			ytreeviewAddressTypes.ColumnsConfig = FluentColumnsConfig<AddressTypeNode>.Create()
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.AddColumn("Тип адреса").AddTextRenderer(x => x.Title)
				.Finish();

			ytreeviewAddressTypes.ItemsDataSource = ViewModel.AddressTypeNodes;

			dateperiodOrders.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			dateperiodOrders.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();

			yentryreferenceShift.SubjectType = typeof(DeliveryShift);
			yentryreferenceShift.Binding.AddBinding(ViewModel, vm => vm.DeliveryShift, w => w.Subject).InitializeFromSource();

			ySpecCmbGeographicGroup.ItemsList = ViewModel.GeographicGroups;
			ySpecCmbGeographicGroup.Binding.AddBinding(ViewModel, vm => vm.GeographicGroup, w => w.SelectedItem).InitializeFromSource();

			checkDriversWithAttachedTerminals.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.HasAccessToDriverTerminal, w => w.Sensitive)
				.AddBinding(vm => vm.ShowDriversWithTerminal, w => w.Active)
				.InitializeFromSource();

			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.AddEnumToHideList(CarTypeOfUse.Loader);
			enumcheckCarTypeOfUse.Binding.AddBinding(ViewModel, vm => vm.RestrictedCarTypesOfUse, w => w.SelectedValuesList,
				new EnumsListConverter<CarTypeOfUse>()).InitializeFromSource();

			enumcheckCarOwnType.EnumType = typeof(CarOwnType);
			enumcheckCarOwnType.Binding.AddBinding(ViewModel, vm => vm.RestrictedCarOwnTypes, w => w.SelectedValuesList,
				new EnumsListConverter<CarOwnType>()).InitializeFromSource();

			buttonStatusNone.Clicked += (sender, args) =>
			{
				ViewModel.DeselectAllRouteListStatuses();
				ytreeviewRouteListStatuses.YTreeModel.EmitModelChanged();
			};

			buttonStatusAll.Clicked += (sender, args) =>
			{
				ViewModel.SelectAllRouteListStatuses();
				ytreeviewRouteListStatuses.YTreeModel.EmitModelChanged();
			};

			ybuttonInfo.Clicked += (sender, args) => ViewModel.InfoCommand.Execute();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.StatusNodes):
				case nameof(ViewModel.DisplayableStatuses):
					ytreeviewRouteListStatuses.YTreeModel.EmitModelChanged();
					break;
			}
		}
	}
}
