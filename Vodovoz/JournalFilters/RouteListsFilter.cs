using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Widgets;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.EntityRepositories;
using QS.Project.Services;
using Gamma.ColumnConfig;
using System.ComponentModel;
using Gamma.GtkWidgets;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListsFilter : RepresentationFilterBase<RouteListsFilter>
	{
		private bool _showDriversWithTerminal;
		private bool _hasAccessToDriverTerminal;

		protected override void ConfigureWithUow()
		{
			SetAndRefilterAtOnce(
				x => x.yentryreferenceShift.SubjectType = typeof(DeliveryShift),
				x => x.yEnumCmbTransport.ItemsEnum = typeof(RLFilterTransport),
				x => x.ySpecCmbGeographicGroup.ItemsList = UoW.Session.QueryOver<GeographicGroup>().List()
			);
			LoadAddressesTypesDefaults();
			LoadRouteListStatusesDefaults();
		}

		public RouteListsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public RouteListsFilter()
		{
			this.Build();

			_hasAccessToDriverTerminal = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("role_сashier") ||
							             ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
			checkDriversWithAttachedTerminals.Sensitive = _hasAccessToDriverTerminal;
			checkDriversWithAttachedTerminals.Toggled += (sender, args) => { ShowDriversWithTerminal = ((yCheckButton)sender).Active; };
		}

		public bool ShowDriversWithTerminal
		{
			get => _showDriversWithTerminal;
			set
			{
				_showDriversWithTerminal = value;
				OnRefiltered();
			}
		}

		public RouteListStatus[] RestrictedStatuses {
			get { return OnlyStatuses; }
			set {
				OnlyStatuses = value;
				ytreeviewRouteListStatuses.Sensitive = OnlyStatuses.Length > 0;
			}
		}

		public DeliveryShift RestrictShift {
			get { return yentryreferenceShift.Subject as DeliveryShift; }
			set {
				yentryreferenceShift.Subject = value;
				yentryreferenceShift.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate {
			get { return dateperiodOrders.StartDateOrNull; }
			set {
				dateperiodOrders.StartDateOrNull = value;
				dateperiodOrders.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate {
			get { return dateperiodOrders.EndDateOrNull; }
			set {
				dateperiodOrders.EndDateOrNull = value;
				dateperiodOrders.Sensitive = false;
			}
		}

		public GeographicGroup RestrictGeographicGroup {
			get => ySpecCmbGeographicGroup.SelectedItem as GeographicGroup;
			set {
				ySpecCmbGeographicGroup.SelectedItem = value;
				ySpecCmbGeographicGroup.Sensitive = false;
			}
		}

		RouteListStatus[] onlyStatuses;

		/// <summary>
		/// Показывать только МЛ со статусом из массива
		/// </summary>
		/// <value>массив отображаемых статусов</value>
		public RouteListStatus[] OnlyStatuses {
			get => onlyStatuses;
			set{
				onlyStatuses = value;
				UpdateAvailableStatuses();
				SelectedStatuses = value;
			}
		}

		public RouteListStatus[] SelectedStatuses {
			get => RouteListStatuses.Where(x => x.Selected).Select(x => x.RouteListStatus).ToArray();
			set{
				UnsubscribeOnStatusesChandged();
				foreach(var status in RouteListStatuses) {
					if(value.Contains(status.RouteListStatus)) { status.Selected = true; }
                    else { status.Selected = false; }
                }
                SubscribeOnStatusesChandged();
                ytreeviewRouteListStatuses.YTreeModel?.EmitModelChanged();
            }
        }

		public IEnumerable<AddressTypeNode> AddressTypes { get; } = new[] {
			new AddressTypeNode(AddressType.Delivery),
			new AddressTypeNode(AddressType.Service),
			new AddressTypeNode(AddressType.ChainStore)
		};

		private void LoadAddressesTypesDefaults()
		{
			var currentUserSettings = CurrentUserSettings.Settings;
			
			foreach(var addressTypeNode in AddressTypes) {
				switch(addressTypeNode.AddressType) {
					case AddressType.Delivery:
						addressTypeNode.Selected = currentUserSettings.LogisticDeliveryOrders;
						break;
					case AddressType.Service:
						addressTypeNode.Selected = currentUserSettings.LogisticServiceOrders;
						break;
					case AddressType.ChainStore:
						addressTypeNode.Selected = currentUserSettings.LogisticChainStoreOrders;
						break;
				}
			}

			ytreeviewAddressTypes.ColumnsConfig = FluentColumnsConfig<AddressTypeNode>.Create()
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.AddColumn("Тип адреса").AddTextRenderer(x => x.Title)
				.Finish();
			ytreeviewAddressTypes.ItemsDataSource = AddressTypes;
			foreach(var at in AddressTypes) {
				at.PropertyChanged += (sender, e) => OnRefiltered();
			}
		}

		public List<RouteListStatusNode> RouteListStatuses { get; private set; } = new List<RouteListStatusNode>();

		private void UpdateAvailableStatuses()
		{
			bool onlyStatus = OnlyStatuses?.Length > 0;

			UnsubscribeOnStatusesChandged();
			RouteListStatuses.Clear();

			foreach(RouteListStatus status in Enum.GetValues(typeof(RouteListStatus))) {
				if(!onlyStatus || onlyStatuses.Contains(status)) {
					RouteListStatuses.Add(new RouteListStatusNode(status));
				}
			}

			ytreeviewRouteListStatuses.YTreeModel?.EmitModelChanged();

			SubscribeOnStatusesChandged();
		}

		private void SubscribeOnStatusesChandged()
		{
			foreach(var status in RouteListStatuses) {
				status.PropertyChanged += OnStatusCheckChanged;
			}
		}

		private void UnsubscribeOnStatusesChandged()
		{
			foreach(var status in RouteListStatuses) {
				status.PropertyChanged -= OnStatusCheckChanged;
			}
		}

		private void OnStatusCheckChanged(object sender, PropertyChangedEventArgs e)
		{
			OnRefiltered();
		}

		private void LoadRouteListStatusesDefaults()
		{
			UpdateAvailableStatuses();
			ytreeviewRouteListStatuses.ColumnsConfig = FluentColumnsConfig<RouteListStatusNode>.Create()
				.AddColumn("Статус").AddTextRenderer(x => x.Title)
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.Finish();
			ytreeviewRouteListStatuses.ItemsDataSource = RouteListStatuses;
		}

		public bool WithDeliveryAddresses => AddressTypes.Any(x => x.AddressType == AddressType.Delivery && x.Selected);

		public bool WithServiceAddresses => AddressTypes.Any(x => x.AddressType == AddressType.Service && x.Selected);

		public bool WithChainStoreAddresses => AddressTypes.Any(x => x.AddressType == AddressType.ChainStore && x.Selected);

		protected void OnEnumcomboStatusEnumItemSelected(object sender, ItemSelectedEventArgs e)
		{
			OnRefiltered();
		}

		protected void OnDateperiodOrdersPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYentryreferenceShiftChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		public void SetFilterDates(DateTime? startDate, DateTime? endDate)
		{
			dateperiodOrders.StartDateOrNull = startDate;
			dateperiodOrders.EndDateOrNull = endDate;
		}

		//возврат выбранного значения в списке ТС и засерение списка в случае программной установки значения
		public RLFilterTransport? RestrictTransport {
			get { return yEnumCmbTransport.SelectedItem as RLFilterTransport?; }
			set {
				yEnumCmbTransport.SelectedItemOrNull = value;
				yEnumCmbTransport.Sensitive = false;
			}
		}

		protected void OnYEnumCmbTransportChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYSpecCmbGeographicGroupChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

        protected void OnButtonStatusAllClicked(object sender, EventArgs e)
        {
            SelectedStatuses = Enum.GetValues(typeof(RouteListStatus)).Cast<RouteListStatus>().ToArray();
            OnRefiltered();
        }

        protected void OnButtonStatusNoneClicked(object sender, EventArgs e)
        {
            SelectedStatuses = new RouteListStatus[] { };
            OnRefiltered();
        }
    }
}

