using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModel
{
	public class ReadyForReceptionVM : RepresentationModelWithoutEntityBase<ReadyForReceptionVMNode>
	{
		public ReadyForReceptionVM() : this(UnitOfWorkFactory.CreateWithoutRoot()) { }

		public ReadyForReceptionVM(IUnitOfWork uow) : base(typeof(RouteList), typeof(Vodovoz.Domain.Orders.Order))
		{
			this.UoW = uow;
		}

		public ReadyForReceptionFilter Filter {
			get => RepresentationFilter as ReadyForReceptionFilter;
			set => RepresentationFilter = value as IRepresentationFilter;
		}
		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
			RouteList routeListAlias = null;
			ReadyForReceptionVMNode resultAlias = null;
			Employee employeeAlias = null;
			Car carAlias = null;
			Warehouse warehouseAlias = null;

			List<ReadyForReceptionVMNode> items = new List<ReadyForReceptionVMNode>();

			var queryRoutes = UoW.Session.QueryOver<RouteList>(() => routeListAlias)
				.Where(rl => rl.CurrentWarehouse != null)
				.Where(rl => rl.Status == RouteListStatus.OnClosing)
				.JoinAlias(rl => rl.Driver, () => employeeAlias)
				.JoinAlias(rl => rl.Car, () => carAlias)
				.Left.JoinAlias(rl => rl.CurrentWarehouse, () => warehouseAlias)
				;

			if(Filter.RestrictWarehouse != null)
				queryRoutes.Where(rl => rl.CurrentWarehouse == Filter.RestrictWarehouse);

			if(Filter.RestrictWithoutUnload) {
				var hasOtherUnloadDocuments = QueryOver.Of<CarUnloadDocument>()
					.Where(u => u.RouteList.Id == routeListAlias.Id)
					.Select(u => u.RouteList);

				queryRoutes.WithSubquery.WhereNotExists(hasOtherUnloadDocuments);
			}

			items.AddRange(
				queryRoutes.SelectList(list => list
				   .SelectGroup(() => routeListAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.Name)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LastName)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
				   .Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.Car)
				   .Select(() => routeListAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.WarehouseName)
				)
				.OrderBy(x => x.Date).Desc
				.TransformUsing(Transformers.AliasToBean<ReadyForReceptionVMNode>())
				.List<ReadyForReceptionVMNode>());

			SetItemsSource(items);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ReadyForReceptionVMNode>.Create()
			.AddColumn("Маршрутный лист").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Водитель").AddTextRenderer(node => node.Driver)
			.AddColumn("Машина").AddTextRenderer(node => node.Car)
			.AddColumn("Дата").AddTextRenderer(node => node.Date.ToShortDateString())
			.AddColumn("Склад").AddTextRenderer(node => node.WarehouseName)
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		protected override bool NeedUpdateFunc(object updatedSubject) => true;

		#endregion
	}

	public class ReadyForReceptionVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

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

		public string WarehouseName { get; set; }
	}
}