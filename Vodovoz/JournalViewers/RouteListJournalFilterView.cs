using System.ComponentModel;
using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic;
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

            yentryreferenceShift.SubjectType = typeof(DeliveryShift);
            yentryreferenceShift.Binding.AddBinding(ViewModel, vm => vm.DeliveryShift, w => w.Subject).InitializeFromSource();

            yEnumCmbTransport.ItemsEnum = typeof(RLFilterTransport);
            //yEnumCmbTransport.Binding.AddBinding(ViewModel, vm => vm.TransportType, w => w.SelectedItemOrNull).InitializeFromSource();

            ySpecCmbGeographicGroup.ItemsList = ViewModel.GeographicGroups;
            ySpecCmbGeographicGroup.Binding.AddBinding(ViewModel, vm => vm.GeographicGroup, w => w.SelectedItem).InitializeFromSource();

            //buttonStatusAll.Clicked += (s, a) => { ViewModel.SelectAllRouteListStatuses(); };
            //buttonStatusNone.Clicked += (s, a) => { ViewModel.DeselectAllRouteListStatuses(); };
        }

        protected void OnButtonStatusAllActivated(object sender, System.EventArgs e)
        {
            ViewModel.SelectAllRouteListStatuses();
            ytreeviewRouteListStatuses.YTreeModel.EmitModelChanged();
        }

        protected void OnButtonStatusNoneActivated(object sender, System.EventArgs e)
        {
            ViewModel.DeselectAllRouteListStatuses();
            ytreeviewRouteListStatuses.YTreeModel.EmitModelChanged();
        }

        protected void OnYSpecCmbGeographicGroupItemSelected(object sender, System.EventArgs e)
        {
            ViewModel.Update();
        }

        protected void OnDateperiodOrdersPeriodChangedByUser(object sender, System.EventArgs e)
        {
            ViewModel.Update();
        }

        protected void OnYEnumCmbTransportChangedByUser(object sender, System.EventArgs e)
        {
            ViewModel.TransportType = (RouteListJournalFilterViewModel.RLFilterTransport?)(yEnumCmbTransport.SelectedItemOrNull as RLFilterTransport?);
            ViewModel.Update();
        }

        protected void OnYentryreferenceShiftChangedByUser(object sender, System.EventArgs e)
        {
            ViewModel.Update();
        }

        
    }
}
