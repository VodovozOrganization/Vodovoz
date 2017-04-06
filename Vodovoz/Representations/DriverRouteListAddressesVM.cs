using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using QSOrmProject;
using NHibernate.Transform;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModel
{
	public class DriverRouteListAddressesVM : RepresentationModelEntityBase<RouteListItem, DriverRouteListAddressVMNode>
	{
		#region IRepresentationModel implementation
		private int driverId;

		public override void UpdateNodes()
		{
			DriverRouteListAddressVMNode resultAlias = null;
			Employee driverAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Order orderAlias = null;

			var query = UoW.Session.QueryOver<RouteListItem>(() => routeListItemAlias);

			var result = query
				.JoinAlias(rli => rli.RouteList, () => routeListAlias)
				.JoinAlias(rli => rli.RouteList.Driver, () => driverAlias)
				.JoinAlias(rli => rli.Order, () => orderAlias)

				.Where (() => routeListAlias.Status == RouteListStatus.EnRoute)
				.Where (() => routeListAlias.Driver.Id == driverId)
				.SelectList(list => list
					.Select(() => routeListItemAlias.Id).WithAlias(() => resultAlias.Id)
				    .Select (() => orderAlias.Id).WithAlias (() => resultAlias.OrderId)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListNumber)
					.Select(() => routeListItemAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(() => orderAlias.DeliverySchedule).WithAlias(() => resultAlias.Time)
					.Select(() => orderAlias.DeliveryPoint).WithAlias(() => resultAlias.Address)

				)
				.TransformUsing(Transformers.AliasToBean<DriverRouteListAddressVMNode>())
				.List<DriverRouteListAddressVMNode>();

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<DriverRouteListAddressVMNode>.Create()
			.AddColumn("МЛ №").SetDataProperty(node => node.RouteListNumber.ToString())
			.AddColumn("Время").SetDataProperty(node => node.Time.DeliveryTime)
			.AddColumn("Статус").SetDataProperty(node => node.Status.GetEnumTitle())
			.AddColumn("Адрес").SetDataProperty(node => node.Address.CompiledAddress)
			.Finish();

		public override IColumnsConfig ColumnsConfig
		{
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(RouteListItem updatedSubject)
		{
			return true;
		}

		#endregion

		public DriverRouteListAddressesVM(int driverId)
			: this(UnitOfWorkFactory.CreateWithoutRoot(), driverId)
		{
		}

		public DriverRouteListAddressesVM(IUnitOfWork uow, int driverId)
			: base()
		{
			this.driverId = driverId;
			this.UoW = uow;
		}
	}

	public class DriverRouteListAddressVMNode
	{
		public int Id{ get; set; }

		public int OrderId { get; set; }

		public DeliveryPoint Address { get; set; }

		public DeliverySchedule Time { get; set; }

		public RouteListItemStatus Status { get; set; }

		public int RouteListNumber { get; set; }
	}
}