using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Logistic;
using QS.Project.Services;
using Gamma.ColumnConfig;
using System.ComponentModel;
using Gamma.GtkWidgets;
using Gamma.Widgets.Additions;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	[ToolboxItem(true)]
	public partial class RouteListsFilter : RepresentationFilterBase<RouteListsFilter>
	{
		private bool _showDriversWithTerminal;
		private RouteListStatus[] _onlyStatuses;

		private EnumsListConverter<CarTypeOfUse> _carTypeOfUseConverter;
		private EnumsListConverter<CarOwnType> _carOwnTypeConverter;

		protected override void ConfigureWithUow()
		{
			SetAndRefilterAtOnce(
				x => x.yentryreferenceShift.SubjectType = typeof(DeliveryShift),
				x => x.enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse),
				x => x.enumcheckCarTypeOfUse.SelectAll(),
				x => x.enumcheckCarOwnType.EnumType = typeof(CarOwnType),
				x => x.enumcheckCarOwnType.SelectAll(),
				x => x.ySpecCmbGeographicGroup.ItemsList = UoW.Session.QueryOver<GeoGroup>().List()
			);

			_carTypeOfUseConverter = new EnumsListConverter<CarTypeOfUse>();
			_carOwnTypeConverter = new EnumsListConverter<CarOwnType>();

			enumcheckCarTypeOfUse.CheckStateChanged += (sender, args) => OnRefiltered();
			enumcheckCarOwnType.CheckStateChanged += (sender, args) => OnRefiltered();

			ySpecCmbGeographicGroup.Changed += OnYSpecCmbGeographicGroupChanged;
			yentryreferenceShift.Changed += OnYentryreferenceShiftChanged;
			dateperiodOrders.PeriodChanged += OnDateperiodOrdersPeriodChanged;
			buttonStatusNone.Clicked += OnButtonStatusNoneClicked;
			buttonStatusAll.Clicked += OnButtonStatusAllClicked;

			LoadAddressesTypesDefaults();
			LoadRouteListStatusesDefaults();
		}

		public RouteListsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public RouteListsFilter()
		{
			Build();

			var hasAccessToDriverTerminal = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.RoleCashier)
				|| ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
			checkDriversWithAttachedTerminals.Sensitive = hasAccessToDriverTerminal;
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

		public GeoGroup RestrictGeographicGroup {
			get => ySpecCmbGeographicGroup.SelectedItem as GeoGroup;
			set {
				ySpecCmbGeographicGroup.SelectedItem = value;
				ySpecCmbGeographicGroup.Sensitive = false;
			}
		}

		public IList<CarTypeOfUse> RestrictedCarTypesOfUse
		{
			get => (IList<CarTypeOfUse>)_carTypeOfUseConverter.ConvertBack(enumcheckCarTypeOfUse.SelectedValuesList, null, null, null);
			set => enumcheckCarTypeOfUse.SelectedValuesList = (IList<Enum>)_carTypeOfUseConverter.Convert(value, null, null, null);
		}

		public IList<CarOwnType> RestrictedCarOwnTypes
		{
			get => (IList<CarOwnType>)_carOwnTypeConverter.ConvertBack(enumcheckCarOwnType.SelectedValuesList, null, null, null);
			set => enumcheckCarOwnType.SelectedValuesList = (IList<Enum>)_carOwnTypeConverter.Convert(value, null, null, null);
		}

		/// <summary>
		/// Показывать только МЛ со статусом из массива
		/// </summary>
		/// <value>массив отображаемых статусов</value>
		public RouteListStatus[] OnlyStatuses {
			get => _onlyStatuses;
			set{
				_onlyStatuses = value;
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
				if(!onlyStatus || _onlyStatuses.Contains(status)) {
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

