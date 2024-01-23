using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModel
{
	public class DriverRouteListAddressesVM : RepresentationModelEntityBase<RouteListItem, DriverRouteListAddressVMNode>
	{
		#region IRepresentationModel implementation
		private readonly int _driverId;

		public override void UpdateNodes()
		{
			DriverRouteListAddressVMNode resultAlias = null;
			Employee driverAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Order orderAlias = null;

			var result = UoW.Session.QueryOver<RouteListItem>(() => routeListItemAlias)
				.JoinAlias(rli => rli.RouteList, () => routeListAlias)
				.JoinAlias(rli => rli.RouteList.Driver, () => driverAlias)
				.JoinAlias(rli => rli.Order, () => orderAlias)
				.Where(() => routeListAlias.Status == RouteListStatus.EnRoute)
				.Where(() => routeListAlias.Driver.Id == _driverId)
				.SelectList(list => list
					.Select(() => routeListItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(Projections.Entity(() => orderAlias)).WithAlias(() => resultAlias.Order)
					.Select(Projections.Entity(() => routeListItemAlias)).WithAlias(() => resultAlias.RouteListItem)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListNumber)
					.Select(() => routeListItemAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(() => orderAlias.DeliverySchedule).WithAlias(() => resultAlias.Time)
					.Select(() => orderAlias.DeliveryPoint).WithAlias(() => resultAlias.DeliveryPoint))
				.TransformUsing(Transformers.AliasToBean<DriverRouteListAddressVMNode>())
				.List<DriverRouteListAddressVMNode>();

			SetItemsSource(result);
		}

		private readonly IColumnsConfig _columnsConfig = FluentColumnsConfig<DriverRouteListAddressVMNode>.Create()
			.AddColumn("МЛ №").AddTextRenderer(node => node.RouteListNumber.ToString())
			.AddColumn("Время").AddTextRenderer(node => node.Time.DeliveryTime)
			.AddColumn("Статус").AddTextRenderer(node => node.Status.GetEnumTitle())
			.AddColumn("Адрес").AddTextRenderer(node => node.DeliveryPoint.CompiledAddress)
			.Finish();

		public override IColumnsConfig ColumnsConfig => _columnsConfig;

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(RouteListItem updatedSubject)
		{
			return true;
		}

		#endregion

		public DriverRouteListAddressesVM(int driverId)
			: this(ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot(), driverId)
		{
		}

		public DriverRouteListAddressesVM(IUnitOfWork uow, int driverId)
		{
			_driverId = driverId;
			UoW = uow;
		}
	}

	public class DriverRouteListAddressVMNode
	{
		public int Id { get; set; }
		public Order Order { get; set; }
		public RouteListItem RouteListItem { get; set; }
		public DeliveryPoint DeliveryPoint { get; set; }
		public DeliverySchedule Time { get; set; }
		public RouteListItemStatus Status { get; set; }
		public int RouteListNumber { get; set; }
	}
}
