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

            ytreeviewAddressTypes.ColumnsConfig = FluentColumnsConfig<AddressTypeNode>.Create()
                .AddColumn("").AddToggleRenderer(x => x.Selected)
                .AddColumn("Тип адреса").AddTextRenderer(x => x.Title)
                .Finish();
            ytreeviewAddressTypes.ItemsDataSource = ViewModel.AddressTypeNodes;

            yentryreferenceShift.SubjectType = typeof(DeliveryShift);
            yEnumCmbTransport.ItemsEnum = typeof(RLFilterTransport);
            ySpecCmbGeographicGroup.ItemsList = ViewModel.GeographicGroups;
        }
    }
}
