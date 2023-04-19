using Gtk;
using QS.DomainModel.UoW;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.SidePanel.InfoProviders;
using Action = System.Action;

namespace Vodovoz.SidePanel.InfoViews
{
	[ToolboxItem(true)]
	public partial class CarsMonitoringInfoPanelView : Bin, IPanelView, INotifyPropertyChanged, IDisposable
	{
		private const string _radioButtonPrefix = "yrbtn";
		private const string _groupFilterOrdersPrefix = "FilterOrders";
		private const string _groupFastDeliveryIntervalFromPrefix = "FastDeliveryIntervalFrom";

		private readonly IUnitOfWork _unitOfWork;
		private FilterOrdersEnum _filterOrders;
		private FastDeliveryIntervalFromEnum _fastDeliveryIntervalFrom;
		private bool _isFastDeliveryOnly;

		public CarsMonitoringInfoPanelView(IUnitOfWorkFactory unitOfWorkFactory) : base()
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Панель мониторинга автомобилей");
			_unitOfWork.Session.DefaultReadOnly = true;

			Nodes = new ObservableCollection<FastDeliveryMonitoringNode>();

			SetDefaults();

			Build();

			ytvAddressesInProcess.CreateFluentColumnsConfig<FastDeliveryMonitoringNode>()
				.AddColumn("").AddTextRenderer(x => x.Column1, useMarkup: true)
				.AddColumn("").AddTextRenderer(x => x.Column2, useMarkup: true)
				.AddColumn("").AddTextRenderer(x => x.Column3, useMarkup: true)
				.Finish();

			ytvAddressesInProcess.EnableGridLines = TreeViewGridLines.Both;

			ytvAddressesInProcess.ItemsDataSource = Nodes;

			NodesChanged += ytvAddressesInProcess.YTreeModel.EmitModelChanged;

			buttonRefresh.Clicked += OnButtonRefreshClicked;

			ycheckbuttonIsFastDeliveryOnly.Binding
				.AddBinding(this, v => v.IsFastDeliveryOnly, w => w.Active)
				.InitializeFromSource();

			foreach(RadioButton button in yrbtnFilterOrdersAll.Group)
			{
				if(button.Active)
				{
					FilterOrdersGroupSelectionChanged(button, EventArgs.Empty);
				}

				button.Toggled += FilterOrdersGroupSelectionChanged;
			}

			foreach(RadioButton button in yrbtnFastDeliveryIntervalFromOrderCreated.Group)
			{
				if(button.Active)
				{
					FastDeliveryIntervalFromSelectionChanged(button, EventArgs.Empty);
				}

				button.Toggled += FastDeliveryIntervalFromSelectionChanged;
			}
		}

		private void FastDeliveryIntervalFromSelectionChanged(object sender, EventArgs empty)
		{
			if(sender is RadioButton rbtn && rbtn.Active)
			{
				var trimmedName = rbtn.Name
					.Replace(_radioButtonPrefix, string.Empty)
					.Replace(_groupFastDeliveryIntervalFromPrefix, string.Empty);

				FastDeliveryIntervalFrom = (FastDeliveryIntervalFromEnum)Enum.Parse(typeof(FastDeliveryIntervalFromEnum), trimmedName);
			}
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
			FilterOrders = FilterOrdersEnum.All;
			FastDeliveryIntervalFrom = FastDeliveryIntervalFromEnum.OrderCreated;
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

		public FastDeliveryIntervalFromEnum FastDeliveryIntervalFrom
		{
			get => _fastDeliveryIntervalFrom;
			set
			{
				if(_fastDeliveryIntervalFrom != value)
				{
					_fastDeliveryIntervalFrom = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FastDeliveryIntervalFrom)));
				}
			}
		}

		private void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			Refresh();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event Action NodesChanged;

		public ObservableCollection<FastDeliveryMonitoringNode> Nodes { get; }

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => true;

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			LoadData();
			NodesChanged?.Invoke();
		}

		public override void Dispose()
		{
			NodesChanged -= ytvAddressesInProcess.YTreeModel.EmitModelChanged;
			base.Dispose();
		}

		private void LoadData()
		{
			Nodes.Clear();

			var nodesQuery = from rl in _unitOfWork.Session.Query<RouteList>()
							 join rla in _unitOfWork.Session.Query<RouteListItem>()
							 on rl.Id equals rla.RouteList.Id
							 join o in _unitOfWork.Session.Query<Order>()
							 on rla.Order.Id equals o.Id
							 join dp in _unitOfWork.Session.Query<DeliveryPoint>()
							 on o.DeliveryPoint.Id equals dp.Id
							 join driver in _unitOfWork.Session.Query<Employee>()
							 on rl.Driver.Id equals driver.Id
							 join car in _unitOfWork.Session.Query<Car>()
							 on rl.Car.Id equals car.Id
							 where rl.Status == RouteListStatus.EnRoute
								&& rla.Status == RouteListItemStatus.EnRoute
								&& (FilterOrders == FilterOrdersEnum.All
									|| (FilterOrders == FilterOrdersEnum.WithFastDelivery && o.IsFastDelivery)
									|| (FilterOrders == FilterOrdersEnum.WithoutFastDelivery && !o.IsFastDelivery))
							 let surnameWithInitials =
								$"{driver.LastName} " +
								$"{driver.Name.Substring(0, 1)}. " +
								$"{driver.Patronymic.Substring(0, 1)}."
							 let isFastDeliveryString = o.IsFastDelivery ? "Доставка за час" : string.Empty
							 let deliveryBefore = rl.Date
							 select new
							 {
								 DriverName = surnameWithInitials,
								 RouteListId = rl.Id,
								 CarNumber = car.RegistrationNumber,
								 Address = $"{dp.Street} {dp.Building}{dp.Letter} {dp.Entrance}",
								 DeliveryType = isFastDeliveryString,
								 DeliveryBefore = deliveryBefore
							 };

			var nodesToAdd = nodesQuery
				.ToList();

			var nodeGroups = nodesToAdd
				.GroupBy(x => (x.DriverName, x.CarNumber, x.RouteListId))
				.Where(group => !IsFastDeliveryOnly || group.Any(x => x.DeliveryType != string.Empty));

			foreach(var group in nodeGroups)
			{
				Nodes.Add(new FastDeliveryMonitoringNode
				{
					Column1 = Bold(group.Key.DriverName),
					Column2 = Bold(group.Key.CarNumber),
					Column3 = Bold(group.Key.RouteListId.ToString())
				});

				foreach(var node in group)
				{
					var timeElapsed = (node.DeliveryBefore - DateTime.Now);

					var spanFormat = @"hh\:mm\:ss";

					var timeElapsedFormated = timeElapsed.ToString(spanFormat);

					var timeElapsedString = (timeElapsed.TotalMinutes <= 20 && timeElapsed.TotalMinutes > 0)
						? Blue(timeElapsedFormated)
						: (timeElapsed.TotalMilliseconds > 0)
							? timeElapsedFormated
							: Red("-" + timeElapsedFormated);

					Nodes.Add(new FastDeliveryMonitoringNode
					{
						Column1 = node.Address,
						Column2 = node.DeliveryType,
						Column3 = timeElapsedString
					});
				}
			}
		}

		private static string Bold(string input) => $"<b>{input}</b>";

		private static string Red(string input) => $"<span color=\"Red\">{input}</span>";

		private static string Blue(string input) => $"<span color=\"Blue\">{input}</span>";
	}

	public class FastDeliveryMonitoringNode
	{
		public string Column1 { get; set; }

		public string Column2 { get; set; }

		public string Column3 { get; set; }
	}

	public enum FilterOrdersEnum
	{
		All,
		WithFastDelivery,
		WithoutFastDelivery
	}

	public enum FastDeliveryIntervalFromEnum
	{
		OrderCreated,
		AddedInFirstRouteList,
		RouteListItemTransfered
	}
}
