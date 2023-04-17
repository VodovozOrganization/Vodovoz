using QS.DomainModel.UoW;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Vodovoz.Domain.Orders;
using Vodovoz.SidePanel.InfoProviders;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

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

			var nodesToAddQuery = from rl in _unitOfWork.Session.Query<RouteList>()
								  join driver in _unitOfWork.Session.Query<Employee>()
								  on rl.Driver.Id equals driver.Id
								  join car in _unitOfWork.Session.Query<Car>()
								  on rl.Car.Id equals car.Id
								  where rl.Date == DateTime.Today
									 && rl.Status == RouteListStatus.EnRoute
								  let initials = driver.Name.Substring(0, 1) + "."
									 + driver.Patronymic.Substring(0, 1) + "."
								  let surnameWithInitials = driver.LastName
									 + initials
								  select new FastDeliveryMonitoringNode
								  {
									  Column1 = "<b>" + surnameWithInitials + "</b>",
									  Column2 = "<b>" + car.RegistrationNumber + "</b>",
									  Column3 = "<b>" + rl.Id.ToString() + "</b>"
								  };

			var nodesToAdd = nodesToAddQuery.ToList();

			foreach(var node in nodesToAdd)
			{
				Nodes.Add(node);
			}
		}
	}

	public class FastDeliveryMonitoringNode
	{
		public string Column1 { get; set; }

		public string Column2 { get; set; }

		public string Column3 { get; set; }
	}
}
