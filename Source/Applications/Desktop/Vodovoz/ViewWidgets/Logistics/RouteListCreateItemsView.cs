using Autofac;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate.Criterion;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Extensions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.JournalViewModels;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz
{
	[ToolboxItem(true)]
	public partial class RouteListCreateItemsView : WidgetOnTdiTabBase
	{
		private IRouteColumnRepository _routeColumnRepository;
		private IOrderRepository _orderRepository;
		private IRouteListItemRepository _routeListItemRepository;

		private int _goodsColumnsCount = -1;
		private bool _isEditable = true;
		private bool _canOpenOrder = true;
		private bool _isLogistician;
		private bool _disableColumnsUpdate;

		public bool CanCreate { get; private set; }
		public bool CanUpdate { get; private set; }

		private RouteListItem[] _selectedRouteListItems;
		private IList<RouteColumn> _columnsInfo;

		private IUnitOfWorkGeneric<RouteList> _routeListUoW;
		private IList<RouteColumn> ColumnsInfo => _columnsInfo ?? _routeColumnRepository.ActiveColumns(RouteListUoW);

		public DialogViewModelBase ParentViewModel { get; set; }
		public INavigationManager NavigationManager { get; set; }

		private IPage<OrderForRouteListJournalViewModel> _orderToAddSelectionPage;

		public IUnitOfWorkGeneric<RouteList> RouteListUoW
		{
			get => _routeListUoW;
			set
			{
				if(_routeListUoW == value)
				{
					return;
				}

				_routeListUoW = value;
				if(RouteListUoW.Root.Addresses == null)
				{
					RouteListUoW.Root.Addresses = new List<RouteListItem>();
				}

				_items = RouteListUoW.Root.ObservableAddresses;

				SubscribeOnChanges();

				UpdateColumns();

				ytreeviewItems.ItemsDataSource = _items;
				ytreeviewItems.Reorderable = true;
				UpdateInfo();
			}
		}

		public RouteListCreateItemsView()
		{
			ResolveDependencies();
			Build();
			enumbuttonAddOrder.ItemsEnum = typeof(AddOrderEnum);
			ytreeviewItems.Selection.Changed += OnSelectionChanged;
			ytreeviewItems.Selection.Mode = SelectionMode.Multiple;
		}

		[Obsolete("Удалить при разрешении проблем с контейнером")]
		private void ResolveDependencies()
		{
			_routeColumnRepository = ScopeProvider.Scope.Resolve<IRouteColumnRepository>();
			_orderRepository = ScopeProvider.Scope.Resolve<IOrderRepository>();
			_routeListItemRepository = ScopeProvider.Scope.Resolve<IRouteListItemRepository>();
		}

		public void SubscribeOnChanges()
		{
			RouteListUoW.Root.ObservableAddresses.ElementChanged -= Items_ElementChanged;
			RouteListUoW.Root.ObservableAddresses.ElementChanged += Items_ElementChanged;
			RouteListUoW.Root.ObservableAddresses.ListChanged -= Items_ListChanged;
			RouteListUoW.Root.ObservableAddresses.ListChanged += Items_ListChanged;
			RouteListUoW.Root.ObservableAddresses.ElementAdded -= Items_ElementAdded;
			RouteListUoW.Root.ObservableAddresses.ElementAdded += Items_ElementAdded;
			RouteListUoW.Root.PropertyChanged -= RouteListOnPropertyChanged;
			RouteListUoW.Root.PropertyChanged += RouteListOnPropertyChanged;

			if(RouteListUoW.Root.RouteListProfitability != null)
			{
				RouteListUoW.Root.RouteListProfitability.PropertyChanged += RouteListProfitabilityOnPropertyChanged;
			}

			if(RouteListUoW.Root.AdditionalLoadingDocument != null)
			{
				SubscribeToAdditionalLoadingDocumentItemsUpdates();
			}

			_items = RouteListUoW.Root.ObservableAddresses;
			ytreeviewItems.ItemsDataSource = _items;
			ytreeviewItems.Reorderable = true;
			ytreeviewItems?.YTreeModel?.EmitModelChanged();
		}

		private void RouteListProfitabilityOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateInfo();
		}

		private void RouteListOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(RouteList.Car))
			{
				UpdateInfo();
			}
			if(e.PropertyName == nameof(RouteList.AdditionalLoadingDocument))
			{
				SubscribeToAdditionalLoadingDocumentItemsUpdates();
			}
		}

		private void SubscribeToAdditionalLoadingDocumentItemsUpdates()
		{
			var additionalLoadingItems = RouteListUoW?.Root?.AdditionalLoadingDocument?.ObservableItems;
			if(additionalLoadingItems != null)
			{
				additionalLoadingItems.ElementAdded -= AdditionalLoadItemsOnElementAdded;
				additionalLoadingItems.ElementAdded += AdditionalLoadItemsOnElementAdded;
				additionalLoadingItems.ElementRemoved -= AdditionalLoadItemsOnElementRemoved;
				additionalLoadingItems.ElementRemoved += AdditionalLoadItemsOnElementRemoved;
				additionalLoadingItems.ElementChanged -= AdditionalLoadItemsOnElementChanged;
				additionalLoadingItems.ElementChanged += AdditionalLoadItemsOnElementChanged;
			}
		}

		private void AdditionalLoadItemsOnElementChanged(object alist, int[] aidx)
		{
			UpdateInfo();
		}

		private void AdditionalLoadItemsOnElementRemoved(object alist, int[] aidx, object aobject)
		{
			UpdateInfo();
		}

		private void AdditionalLoadItemsOnElementAdded(object alist, int[] aidx)
		{
			UpdateInfo();
		}

		private bool CanEditRows => _isLogistician
			&& (CanCreate && RouteListUoW.Root.Id == 0
				|| CanUpdate)
			&& RouteListUoW.Root.Status != RouteListStatus.Closed
			&& RouteListUoW.Root.Status != RouteListStatus.MileageCheck;

		public bool DisableColumnsUpdate
		{
			get => _disableColumnsUpdate;
			set
			{
				if(_disableColumnsUpdate == value)
				{
					return;
				}

				_disableColumnsUpdate = value;
				if(!_disableColumnsUpdate)
				{
					UpdateColumns();
				}
			}
		}

		private void Items_ElementAdded(object aList, int[] aIdx)
		{
			UpdateColumns();
			UpdateInfo();
		}

		private void Items_ListChanged(object aList)
		{
			UpdateColumns();
		}

		public void OnForwarderChanged()
		{
			UpdateColumns();
		}

		private void UpdateColumns()
		{
			if(_disableColumnsUpdate)
			{
				return;
			}

			var goodsColumns = _items.SelectMany(i => i.GoodsByRouteColumns.Keys).Distinct().ToArray();

			var config = ColumnsConfigFactory.Create<RouteListItem>()
			.AddColumn("Заказ").AddTextRenderer(node => node.Order.Id.ToString())
			.AddColumn("Адрес").AddTextRenderer(node => node.Order.DeliveryPoint == null ? "Точка доставки не установлена" : string.Format("{0} д.{1}", node.Order.DeliveryPoint.Street, node.Order.DeliveryPoint.Building))
			.AddColumn("Время").AddTextRenderer(node => node.Order.DeliverySchedule == null ? string.Empty : node.Order.DeliverySchedule.Name);
			if(_goodsColumnsCount != goodsColumns.Length)
			{
				_goodsColumnsCount = goodsColumns.Length;

				foreach(var column in ColumnsInfo)
				{
					if(!goodsColumns.Contains(column.Id))
					{
						continue;
					}

					int id = column.Id;
					config = config.AddColumn(column.Name).AddTextRenderer(a => a.GetGoodsAmountForColumn(id).ToString("N0"));
				}
			}
			if(RouteListUoW.Root.Forwarder != null)
			{
				config
					.AddColumn("C экспедитором")
					.AddToggleRenderer(node => node.WithForwarder)
					.AddSetter((cell, node) => cell.Activatable = CanEditRows);
			}
			config
				.AddColumn("Товары").AddTextRenderer(x => ShowAdditional(x.Order.OrderItems))
				.AddColumn("К клиенту")
				.AddTextRenderer(x => x.EquipmentsToClientText, expand: false)
				.AddColumn("От клиента")
				.AddTextRenderer(x => x.EquipmentsFromClientText, expand: false)
				.AddColumn("Доставка за час")
					.AddToggleRenderer(x => x.Order.IsFastDelivery).Editing(false)
				.AddColumn("");
			ytreeviewItems.ColumnsConfig =
				config.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.Order.PreviousOrder == null ? GdkColors.PrimaryText : GdkColors.DangerText)
				.Finish();
		}

		private string ShowAdditional(IList<OrderItem> orderItems)
		{
			List<string> stringParts = new List<string>();

			var additionalItems = orderItems
				.Where(x => x.Nomenclature.Category != NomenclatureCategory.water
					&& x.Nomenclature.Category != NomenclatureCategory.equipment
					&& x.Nomenclature.Category != NomenclatureCategory.service
					&& x.Nomenclature.Category != NomenclatureCategory.deposit
					&& x.Nomenclature.Category != NomenclatureCategory.master);

			foreach(var item in additionalItems)
			{
				var nomCount = item.Count.ToString($"N{item.Nomenclature.Unit.Digits}");
				stringParts.Add($"{item.Nomenclature.Name}: {nomCount}");
			}

			return string.Join("\n", stringParts);
		}

		public void IsEditable(bool isEditable, bool canOpenOrder = true)
		{
			_isEditable = isEditable;
			enumbuttonAddOrder.Sensitive = isEditable;
			_canOpenOrder = canOpenOrder;
			UpdateSensitivity();
		}

		private void Items_ElementChanged(object aList, int[] aIdx)
		{
			UpdateColumns();
			UpdateInfo();
		}

		public void SetPermissionParameters(bool canCreate, bool canUpdate, bool isLogistician)
		{
			CanCreate = canCreate;
			CanUpdate = canUpdate;
			_isLogistician = isLogistician;
		}

		private void OnSelectionChanged(object sender, EventArgs e)
		{
			_selectedRouteListItems = ytreeviewItems.GetSelectedObjects<RouteListItem>();
			UpdateSensitivity();
		}

		private void UpdateSensitivity()
		{
			buttonOpenOrder.Sensitive = _selectedRouteListItems?.Length == 1 && _canOpenOrder;
			buttonDelete.Sensitive = _selectedRouteListItems != null && _isEditable;
		}

		private GenericObservableList<RouteListItem> _items;

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			foreach(var selectedRouteListItem in _selectedRouteListItems)
			{
				if(!RouteListUoW.Root.TryRemoveAddress(selectedRouteListItem, out string message, _routeListItemRepository))
				{
					ServicesConfig.CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, message, "Невозможно удалить");
				}
			}

			UpdateInfo();
		}

		protected void OnEnumbuttonAddOrderEnumItemClicked(object sender, QS.Widgets.EnumItemClickedEventArgs e)
		{
			AddOrderEnum choice = (AddOrderEnum)e.ItemEnum;
			switch(choice)
			{
				case AddOrderEnum.AddOrders:
					AddOrders();
					break;
				case AddOrderEnum.AddAllForRegion:
					AddOrdersFromRegion();
					break;
				default:
					break;
			}
		}

		protected void AddOrders()
		{
			var geoGrpIds = RouteListUoW.Root.GeographicGroups.Select(x => x.Id).ToArray();

			if(_orderToAddSelectionPage != null)
			{
				NavigationManager.SwitchOn(_orderToAddSelectionPage);
				return;
			}

			_orderToAddSelectionPage = NavigationManager.OpenViewModel<OrderForRouteListJournalViewModel, Action<OrderJournalFilterViewModel>>(
				ParentViewModel,
				filter =>
				{
					filter.ExceptIds = RouteListUoW.Root.Addresses.Select(address => address.Order.Id).ToArray();
					filter.RestrictStartDate = RouteListUoW.Root.Date.Date;
					filter.RestrictEndDate = RouteListUoW.Root.Date.Date;
					filter.RestrictFilterDateType = OrdersDateFilterType.DeliveryDate;
					filter.RestrictStatus = OrderStatus.Accepted;
					filter.RestrictWithoutSelfDelivery = true;
					filter.RestrictOnlySelfDelivery = false;
					filter.RestrictHideService = true;
					filter.FilterClosingDocumentDeliverySchedule = false;

					if(geoGrpIds.Any())
					{
						GeoGroup geographicGroupAlias = null;
						var districtIds = RouteListUoW.Session.QueryOver<District>()
							.Left.JoinAlias(d => d.GeographicGroup, () => geographicGroupAlias)
							.Where(() => geographicGroupAlias.Id.IsIn(geoGrpIds))
							.Select
							  (
								  Projections.Distinct(
								  Projections.Property<District>(x => x.Id)
							  )
							)
							.List<int>()
							.ToArray();

						filter.IncludeDistrictsIds = districtIds;
					}
				},
				OpenPageOptions.AsSlave,
				vm => vm.SelectionMode = JournalSelectionMode.Multiple);

			//Selected Callback
			_orderToAddSelectionPage.ViewModel.OnEntitySelectedResult += OnOrdersSelected;
			_orderToAddSelectionPage.PageClosed += OnOrderToAddSelectionPageClosed;
		}

		private void OnOrderToAddSelectionPageClosed(object sender, PageClosedEventArgs e)
		{
			_orderToAddSelectionPage.ViewModel.OnEntitySelectedResult -= OnOrdersSelected;
			_orderToAddSelectionPage.PageClosed -= OnOrderToAddSelectionPageClosed;

			_orderToAddSelectionPage = null;
		}

		private void OnOrdersSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedIds = e.SelectedNodes.Select(x => x.Id).Distinct();

			foreach(var selectedId in selectedIds)
			{
				if(RouteListUoW.Root.Addresses.Any(routeListAddtess => routeListAddtess.Order.Id == selectedId))
				{
					continue;
				}

				var order = RouteListUoW.GetById<Order>(selectedId);

				if(order == null)
				{
					continue;
				}

				RouteListUoW.Root.AddAddressFromOrder(order);
			}
		}

		protected void AddOrdersFromRegion()
		{
			var filter = new DistrictJournalFilterViewModel { Status = DistrictsSetStatus.Active, OnlyWithBorders = true };
			var journalViewModel = new DistrictJournalViewModel(filter, ServicesConfig.UnitOfWorkFactory, ServicesConfig.CommonServices)
			{
				SelectionMode = JournalSelectionMode.Single,
				EnableDeleteButton = false,
				EnableEditButton = false,
				EnableAddButton = false
			};
			journalViewModel.OnEntitySelectedResult += (o, args) =>
			{
				var selectedDistrict = args.SelectedNodes.FirstOrDefault();
				if(selectedDistrict != null)
				{
					var orders = _orderRepository.GetAcceptedOrdersForRegion(RouteListUoW, RouteListUoW.Root.Date, selectedDistrict.Id);

					foreach(var order in orders)
					{
						if(RouteListUoW.Root.ObservableAddresses.All(a => a.Order.Id != order.Id))
						{
							RouteListUoW.Root.AddAddressFromOrder(order);
						}
					}
				}
			};
			MyTab.TabParent.AddSlaveTab(MyTab, journalViewModel);
		}

		public void UpdateInfo()
		{
			var total =
				_routeListUoW.Root.Addresses.SelectMany(a => a.Order.OrderItems)
					.Where(i => i.Nomenclature.Category == NomenclatureCategory.water && i.Nomenclature.TareVolume == TareVolume.Vol19L)
					.Sum(i => i.Count)
				+ (_routeListUoW.Root.AdditionalLoadingDocument?.Items
					.Where(i => i.Nomenclature.Category == NomenclatureCategory.water && i.Nomenclature.TareVolume == TareVolume.Vol19L)
					.Sum(x => x.Amount) ?? 0);

			labelSum.LabelProp = $"Всего бутылей: {total:N0}";
			UpdateProfitabilityInfo();
			UpdateWeightInfo();
			UpdateVolumeInfo();
		}

		public virtual void UpdateWeightInfo()
		{
			if(RouteListUoW?.Root.Car != null)
			{
				var maxWeight = RouteListUoW.Root.Car.CarModel.MaxWeight > 0
					? RouteListUoW.Root.Car.CarModel.MaxWeight.ToString()
					: " ?";
				var weight = RouteListUoW.Root.HasOverweight()
					? $"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">Перегруз на {RouteListUoW.Root.Overweight():0.###} кг.</span>"
					: $"<span foreground=\"{GdkColors.SuccessText.ToHtmlColor()}\">Вес: {RouteListUoW.Root.GetTotalWeight():0.###}/{maxWeight} кг.</span>";
				lblWeight.LabelProp = weight;
			}
			if(RouteListUoW?.Root?.Car == null)
			{
				lblWeight.LabelProp = "";
			}
		}

		public void UpdateProfitabilityInfo()
		{
			if(RouteListUoW?.Root?.RouteListProfitability is null)
			{
				lblProfitability.LabelProp = "";
				return;
			}

			var prefix = $"<span foreground=\"{GdkColors.SuccessText.ToHtmlColor()}\">";

			var postfix = "</span>";

			if(RouteListUoW.Root.RouteListProfitability.GrossMarginSum < 0)
			{
				prefix = $"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">";
			}

			lblProfitability.LabelProp =
				$"{prefix}Вал. Маржа, руб: {RouteListUoW.Root.RouteListProfitability.GrossMarginSum:F2}{postfix} " +
				$"{prefix}​​Вал. Маржа, %: {RouteListUoW.Root.RouteListProfitability.GrossMarginPercents:F2}{postfix}";
		}

		public virtual void UpdateVolumeInfo()
		{
			if(RouteListUoW?.Root.Car != null)
			{
				var maxVolume = RouteListUoW.Root.Car.CarModel.MaxVolume > 0
					? RouteListUoW.Root.Car.CarModel.MaxVolume.ToString("0.###")
					: " ?";
				var volume = RouteListUoW.Root.HasVolumeExecess()
					? $"<span foreground = \"{GdkColors.DangerText.ToHtmlColor()}\">Объём превышен на {RouteListUoW.Root.VolumeExecess():0.###} м<sup>3</sup>.</span>"
					: $"<span foreground = \"{GdkColors.SuccessText.ToHtmlColor()}\">Объём: {RouteListUoW.Root.GetTotalVolume():0.###}/{maxVolume} м<sup>3</sup>.</span>";
				var reverseVolume = RouteListUoW.Root.HasReverseVolumeExcess()
					? $"<span foreground = \"{GdkColors.DangerText.ToHtmlColor()}\">Объём возврата превышен на {RouteListUoW.Root.ReverseVolumeExecess():0.###} м<sup>3</sup>.</span>"
					: $"<span foreground = \"{GdkColors.SuccessText.ToHtmlColor()}\">Объём возврата: {RouteListUoW.Root.GetTotalReverseVolume():0.###}/{maxVolume} м<sup>3</sup>.</span>";
				lblVolume.LabelProp = volume + " " + reverseVolume;
			}
			if(RouteListUoW?.Root?.Car == null)
			{
				lblVolume.LabelProp = "";
			}
		}

		protected void OnButtonOpenOrderClicked(object sender, EventArgs e)
		{
			if(_selectedRouteListItems?.Length == 1)
			{
				MyTab.TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<Order>(_selectedRouteListItems[0].Order.Id),
					() => new OrderDlg(_selectedRouteListItems[0].Order)
				);
			}
		}

		protected void OnYtreeviewItemsRowActivated(object o, RowActivatedArgs args)
		{
			if(_canOpenOrder)
			{
				buttonOpenOrder.Click();
			}
		}

		public override void Destroy()
		{
			_routeColumnRepository = null;
			_routeListItemRepository = null;
			_orderRepository = null;
			base.Destroy();
		}
	}
}
