using Autofac;
using GLib;
using Gtk;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using System;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Source = GLib.Source;

namespace Vodovoz.SidePanel.InfoViews
{
	[ToolboxItem(true)]
	public partial class CarsMonitoringInfoPanelView : Bin, IPanelView, INotifyPropertyChanged, IDisposable
	{
		private const string _radioButtonPrefix = "yrbtn";
		private const string _groupFilterOrdersPrefix = "FilterOrders";
		private const string _groupFastDeliveryIntervalFromPrefix = "FastDeliveryIntervalFrom";

		private DelegateCommand _refreshMonitoring;

		private readonly IUnitOfWork _unitOfWork;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly INavigationManager _navigationManager;
		private FilterOrdersEnum _filterOrders;
		private bool _isFastDeliveryOnly;
		private IInfoProvider _infoProvider;

		private IOrderedEnumerable<IGrouping<(string DriverName, string CarNumber, int RouteListId), DataNode>> _cachedNodeGroups;
		private TimeoutHandler _timeoutTimerHandler;
		private uint _timerId;

		private readonly FastDeliveryIntervalFromEnum _fastDeliveryIntervalFrom;

		public CarsMonitoringInfoPanelView(
			IUnitOfWorkFactory unitOfWorkFactory,
			IDeliveryRulesSettings deliveryRulesSettings,
			INavigationManager navigationManager,
			IGeneralSettings generalSettings)
			: base()
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			if(generalSettings is null)
			{
				throw new ArgumentNullException(nameof(generalSettings));
			}

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Панель мониторинга автомобилей");
			_unitOfWork.Session.DefaultReadOnly = true;

			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			Nodes = new GenericObservableList<FastDeliveryMonitoringNode>();

			SetDefaults();

			Build();

			ytvAddressesInProcess.CreateFluentColumnsConfig<FastDeliveryMonitoringNode>()
				.AddColumn("").AddTextRenderer(x => x.Column1, useMarkup: true)
				.AddColumn("").AddTextRenderer(x => x.Column2, useMarkup: true)
				.AddColumn("").AddTextRenderer(x => x.Column3, useMarkup: true)
				.Finish();

			ytvAddressesInProcess.HeadersVisible = false;
			ytvAddressesInProcess.EnableGridLines = TreeViewGridLines.Both;
			ytvAddressesInProcess.RowActivated += (s, e) => OpenFastDeliveryTransferDialog();

			ytvAddressesInProcess.ItemsDataSource = Nodes;

			buttonRefresh.Clicked += OnButtonRefreshClicked;

			ytbtnShowFilters.Toggled += OnYtbtnShowFiltersToggled;

			ycheckbuttonIsFastDeliveryOnly.Binding
				.AddBinding(this, v => v.IsFastDeliveryOnly, w => w.Active)
				.InitializeFromSource();

			_fastDeliveryIntervalFrom = generalSettings.FastDeliveryIntervalFrom;

			foreach(RadioButton button in yrbtnFilterOrdersAll.Group)
			{
				button.Active =
					button.Name == _radioButtonPrefix +
						_groupFilterOrdersPrefix +
						Enum.GetName(typeof(FilterOrdersEnum), FilterOrders);

				if(button.Active)
				{
					FilterOrdersGroupSelectionChanged(button, EventArgs.Empty);
				}

				button.Toggled += FilterOrdersGroupSelectionChanged;
			}			

			_timeoutTimerHandler = new TimeoutHandler(RefreshNodesTimerHandler);
			StartTimer(_timeoutTimerHandler);
		}

		private void OpenFastDeliveryTransferDialog()
		{
			if(ytvAddressesInProcess.SelectedRow is FastDeliveryMonitoringNode selectedRow)
			{
				if(selectedRow.RouteListItemId == null 
					|| selectedRow.IsFastDeliveryOrder == null
					|| selectedRow.IsFastDeliveryOrder == false)
				{
					return;
				}

				_navigationManager.OpenViewModel<FastDeliveryOrderTransferViewModel, int>(null, selectedRow.RouteListItemId.Value);
			}
		}

		private void OnYtbtnShowFiltersToggled(object sender, EventArgs e)
		{
			vboxFilterContainer.Visible = ytbtnShowFilters.Active;
		}

		private void StartTimer(TimeoutHandler timeoutHandler)
		{
			_timerId = GLib.Timeout.Add((uint)TimeSpan.FromSeconds(1).TotalMilliseconds, timeoutHandler);
		}

		private bool RefreshNodesTimerHandler()
		{
			RefreshDisplayNodes();
			return true;
		}

		private void FilterOrdersGroupSelectionChanged(object sender, EventArgs e)
		{
			if(sender is RadioButton rbtn && rbtn.Active)
			{
				var trimmedName = rbtn.Name
					.Replace(_radioButtonPrefix, string.Empty)
					.Replace(_groupFilterOrdersPrefix, string.Empty);

				FilterOrders = (FilterOrdersEnum)Enum.Parse(typeof(FilterOrdersEnum), trimmedName);
			}
		}

		private void SetDefaults()
		{
			IsFastDeliveryOnly = true;
			FilterOrders = FilterOrdersEnum.WithFastDelivery;			
		}

		public FilterOrdersEnum FilterOrders
		{
			get => _filterOrders;
			set
			{
				if(_filterOrders != value)
				{
					_filterOrders = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterOrders)));
				}
			}
		}

		public bool IsFastDeliveryOnly
		{
			get => _isFastDeliveryOnly;
			set
			{
				if(_isFastDeliveryOnly != value)
				{
					_isFastDeliveryOnly = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFastDeliveryOnly)));
				}
			}
		}



		private void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			Refresh();
			_refreshMonitoring?.Execute();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public GenericObservableList<FastDeliveryMonitoringNode> Nodes { get; }

		public IInfoProvider InfoProvider
		{
			get => _infoProvider;
			set
			{
				_infoProvider = value;
				_refreshMonitoring = (_infoProvider as CarsMonitoringViewModel)?.RefreshAllCommand;
			}
		}

		public bool VisibleOnPanel => true;

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			LoadData();
		}

		private void LoadData()
		{
			var nodesQuery = from rl in _unitOfWork.Session.Query<RouteList>()
							 join rla in _unitOfWork.Session.Query<RouteListItem>()
							 on rl.Id equals rla.RouteList.Id
							 join o in _unitOfWork.Session.Query<Order>()
							 on rla.Order.Id equals o.Id
							 join dp in _unitOfWork.Session.Query<DeliveryPoint>()
							 on o.DeliveryPoint.Id equals dp.Id
							 join schedule in _unitOfWork.Session.Query<DeliverySchedule>()
							 on o.DeliverySchedule.Id equals schedule.Id
							 join driver in _unitOfWork.Session.Query<Employee>()
							 on rl.Driver.Id equals driver.Id
							 join car in _unitOfWork.Session.Query<Car>()
							 on rl.Car.Id equals car.Id
							 where rl.Status == RouteListStatus.EnRoute
								&& rla.Status == RouteListItemStatus.EnRoute
								&& (FilterOrders == FilterOrdersEnum.All
									|| (FilterOrders == FilterOrdersEnum.WithFastDelivery && o.IsFastDelivery)
									|| (FilterOrders == FilterOrdersEnum.WithoutFastDelivery && !o.IsFastDelivery))
								&& rl.Date > DateTime.Today.AddMonths(-1)
							 let surnameWithInitials =
								$"{driver.LastName} " +
								$"{driver.Name.Substring(0, 1)}. " +
								$"{driver.Patronymic.Substring(0, 1)}."
							 let isFastDeliveryString = o.IsFastDelivery ? "Доставка за час" : string.Empty
							 let deliveryBefore = o.IsFastDelivery
								? _fastDeliveryIntervalFrom == FastDeliveryIntervalFromEnum.OrderCreated
									? o.CreateDate.Value.Add(_deliveryRulesSettings.MaxTimeForFastDelivery)
									: _fastDeliveryIntervalFrom == FastDeliveryIntervalFromEnum.AddedInFirstRouteList
										? (from rlaFirst in _unitOfWork.Session.Query<RouteListItem>()
											where rlaFirst.Order.Id == o.Id
											orderby rlaFirst.CreationDate ascending
											select rlaFirst.CreationDate).First().Add(_deliveryRulesSettings.MaxTimeForFastDelivery)
										: _fastDeliveryIntervalFrom == FastDeliveryIntervalFromEnum.RouteListItemTransfered
											? rla.CreationDate.Add(_deliveryRulesSettings.MaxTimeForFastDelivery)
											: rl.Date.Add(schedule.To)
								: rl.Date.Add(schedule.To)
							 let address = $"{dp.Street} {dp.Building}{dp.Letter}"
							 select new DataNode
							 {
								 DriverName = surnameWithInitials,
								 RouteListId = rl.Id,
								 CarNumber = car.RegistrationNumber,
								 Address = address,
								 DeliveryType = isFastDeliveryString,
								 DeliveryBefore = deliveryBefore,
								 RouteListItemId = rla.Id,
								 IsFastDeliveryOrder = o.IsFastDelivery
							 };

			var nodesToAdd = nodesQuery
				.ToList();

			_cachedNodeGroups = nodesToAdd
				.GroupBy(x => (x.DriverName, x.CarNumber, x.RouteListId))
				.Where(group => !IsFastDeliveryOnly || group.Any(x => x.DeliveryType != string.Empty))
				.OrderBy(x => x.Key.DriverName);

			RefreshDisplayNodes();
		}

		private void RefreshDisplayNodes()
		{
			Nodes.Clear();
			ytvAddressesInProcess.ItemsDataSource = Nodes;

			foreach(var group in _cachedNodeGroups)
			{
				Nodes.Add(new FastDeliveryMonitoringNode
				{
					Column1 = Bold(group.Key.DriverName),
					Column2 = Bold(group.Key.CarNumber),
					Column3 = Bold(group.Key.RouteListId.ToString())
				});

				var orderedGroup = group.OrderBy(x => x.DeliveryBefore);

				foreach(var node in orderedGroup)
				{
					var timeElapsed = (node.DeliveryBefore - DateTime.Now);

					var spanFormat = @"hh\:mm\:ss";

					var timeElapsedFormated = timeElapsed.ToString(spanFormat);

					var timeElapsedString = string.Empty;

					if(timeElapsed.TotalMinutes >= 50)
					{
						timeElapsedString = Green(timeElapsedFormated);
					}
					else
					{
						timeElapsedString = (timeElapsed.TotalMinutes <= 20 && timeElapsed.TotalMinutes > 0)
							? Blue(timeElapsedFormated)
							: (timeElapsed.TotalMilliseconds > 0)
								? timeElapsedFormated
								: Red("-" + timeElapsedFormated);
					}

					Nodes.Add(new FastDeliveryMonitoringNode
					{
						Column1 = node.Address.Length > 35 ? node.Address.Substring(0, 32) + "..." : node.Address,
						Column2 = node.DeliveryType,
						Column3 = timeElapsedString,
						RouteListItemId = node.RouteListItemId,
						IsFastDeliveryOrder = node.IsFastDeliveryOrder
					});
				}
			}
		}

		private static string Bold(string input) => $"<b>{input}</b>";

		private static string Red(string input) => $"<span color=\"{GdkColors.DangerText.ToHtmlColor()}\">{input}</span>";

		private static string Blue(string input) => $"<span color=\"{GdkColors.InfoText.ToHtmlColor()}\">{input}</span>";

		private static string Green(string input) => $"<span color=\"{GdkColors.SuccessText.ToHtmlColor()}\">{input}</span>";

		public override void Destroy()
		{
			Source.Remove(_timerId);
			ytvAddressesInProcess.Destroy();
			base.Destroy();
		}

		public class FastDeliveryMonitoringNode
		{
			public string Column1 { get; set; }

			public string Column2 { get; set; }

			public string Column3 { get; set; }
			public int? RouteListItemId { get; set; }
			public bool? IsFastDeliveryOrder { get; set; }
		}

		public enum FilterOrdersEnum
		{
			All,
			WithFastDelivery,
			WithoutFastDelivery
		}
	}
}
