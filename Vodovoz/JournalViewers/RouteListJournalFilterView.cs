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

            ytreeviewRouteListStatuses.Binding.AddBinding(ViewModel, vm => vm.CanSelectStatuses, w => w.Sensitive);

            ytreeviewAddressTypes.ColumnsConfig = FluentColumnsConfig<AddressTypeNode>.Create()
                .AddColumn("").AddToggleRenderer(x => x.Selected)
                .AddColumn("Тип адреса").AddTextRenderer(x => x.Title)
                .Finish();
            ytreeviewAddressTypes.ItemsDataSource = ViewModel.AddressTypeNodes;

            yentryreferenceShift.SubjectType = typeof(DeliveryShift);
            yEnumCmbTransport.ItemsEnum = typeof(RLFilterTransport);
            ySpecCmbGeographicGroup.ItemsList = ViewModel.GeographicGroups;
        }

        protected void OnButtonStatusAllActivated(object sender, System.EventArgs e)
        {
        }

        protected void OnButtonStatusNoneActivated(object sender, System.EventArgs e)
        {
        }

        protected void OnYSpecCmbGeographicGroupItemSelected(object sender, System.EventArgs e)
        {
        }

        protected void OnDateperiodOrdersPeriodChangedByUser(object sender, System.EventArgs e)
        {
        }

        protected void OnYEnumCmbTransportChangedByUser(object sender, System.EventArgs e)
        {
        }

        protected void OnYentryreferenceShiftChangedByUser(object sender, System.EventArgs e)
        {
        }

        private void OnStatusCheckChanged(object sender, PropertyChangedEventArgs e)
        {

        }
    }
}
