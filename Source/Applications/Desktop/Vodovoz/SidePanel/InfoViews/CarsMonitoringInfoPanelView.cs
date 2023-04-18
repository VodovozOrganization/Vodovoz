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
	public partial class CarsMonitoringInfoPanelView : Gtk.Bin, IPanelView, INotifyPropertyChanged, IDisposable
	{
		private readonly IUnitOfWork _unitOfWork;

		public CarsMonitoringInfoPanelView(IUnitOfWorkFactory unitOfWorkFactory)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Панель мониторинга автомобилей");
			_unitOfWork.Session.DefaultReadOnly = true;

			Nodes = new ObservableCollection<FastDeliveryMonitoringNode>();

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
								&& o.IsFastDelivery
							 let surnameWithInitials =
								$"{driver.LastName} " +
								$"{driver.Name.Substring(0, 1)}. " +
								$"{driver.Patronymic.Substring(0, 1)}."
							 let isFastDeliveryString = o.IsFastDelivery ? "Доставка за час" : string.Empty
							 select new
							 {
								 DriverName = surnameWithInitials,
								 RouteListId = rl.Id,
								 CarNumber = car.RegistrationNumber,
								 Address = $"{dp.Street} {dp.Building}{dp.Letter} {dp.Entrance}",
								 DeliveryType = isFastDeliveryString,
								 DeliveryBefore = rla.CreationDate.AddHours(1)
							 };

			var nodesToAdd = nodesQuery.ToList();

			var nodeGroups = nodesToAdd.GroupBy(x => (x.DriverName, x.CarNumber, x.RouteListId));

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

					var timeElapsedString = timeElapsed.TotalMinutes < 20
						? Blue(timeElapsedFormated)
						: timeElapsed.TotalMilliseconds > 0
							? timeElapsedFormated
							: Red(timeElapsedFormated);

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
}
