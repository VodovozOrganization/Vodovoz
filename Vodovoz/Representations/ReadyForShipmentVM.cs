using System;
using System.Linq;
using Gamma.ColumnConfig;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModel
{
	public class ReadyForShipmentVM : RepresentationModelWithoutEntityBase<ReadyForShipmentVMNode>
	{
		public ReadyForShipmentVM() : this(UnitOfWorkFactory.CreateWithoutRoot()) { }

		public ReadyForShipmentVM(IUnitOfWork uow) : base(
			typeof(RouteList),
			typeof(Vodovoz.Domain.Orders.Order),
			typeof(CarLoadDocument)
		)
		{
			this.UoW = uow;
		}

		public ReadyForShipmentFilter Filter {
			get => RepresentationFilter as ReadyForShipmentFilter;
			set => RepresentationFilter = value as IRepresentationFilter;
		}

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
			RouteList routeListAlias = null;
			ReadyForShipmentVMNode resultAlias = null;
			Employee employeeAlias = null;
			Car carAlias = null;
			DeliveryShift shiftAlias = null;
			Warehouse warehouseAlias = null;

			var queryRoutes = UoW.Session.QueryOver<RouteList>(() => routeListAlias)
				.Where(rl => rl.CurrentWarehouse != null)
				.Where(r => routeListAlias.Status == RouteListStatus.InLoading)
				.JoinAlias(rl => rl.Driver, () => employeeAlias)
				.JoinAlias(rl => rl.Car, () => carAlias)
				.Left.JoinAlias(rl => rl.Shift, () => shiftAlias)
				.Left.JoinAlias(rl => rl.CurrentWarehouse, () => warehouseAlias)
				;

			if(Filter.RestrictWarehouse != null)
				queryRoutes.Where(rl => rl.CurrentWarehouse == Filter.RestrictWarehouse);

			var dirtyList = queryRoutes.SelectList(list => list
				   .SelectGroup(() => routeListAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.Name)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LastName)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
				   .Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.Car)
				   .Select(() => routeListAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => shiftAlias.Name).WithAlias(() => resultAlias.Shift)
				   .Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.WarehouseName)
				)
				.TransformUsing(Transformers.AliasToBean<ReadyForShipmentVMNode>())
				.List<ReadyForShipmentVMNode>();

			SetItemsSource(dirtyList.OrderByDescending(x => x.Date).ToList());
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ReadyForShipmentVMNode>
			.Create()
			.AddColumn("Тип").SetDataProperty(node => node.TypeString)
			.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Водитель").SetDataProperty(node => node.Driver)
			.AddColumn("Машина").SetDataProperty(node => node.Car)
			.AddColumn("Дата").AddTextRenderer(node => node.Date.ToShortDateString())
			.AddColumn("Смена").AddTextRenderer(node => node.Shift)
			.AddColumn("Склад").AddTextRenderer(node => node.WarehouseName)
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		protected override bool NeedUpdateFunc(object updatedSubject) => true;

		#endregion
	}

	public class ReadyForShipmentVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public string TypeString => "Маршрутный лист";

		public string Name { get; set; }

		public string LastName { get; set; }

		public string Patronymic { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Driver => String.Format("{0} {1} {2}", LastName, Name, Patronymic);

		[UseForSearch]
		[SearchHighlight]
		public string Car { get; set; }

		public DateTime Date { get; set; }

		public string Shift { get; set; }
		public string WarehouseName { get; set; }
	}
}

