using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModel
{
	public class ReadyForReceptionVM : RepresentationModelWithoutEntityBase<ReadyForReceptionVMNode>
	{
		public ReadyForReceptionVM () : this (UnitOfWorkFactory.CreateWithoutRoot ())
		{
		}

		public ReadyForReceptionVM (IUnitOfWork uow) : base (typeof(RouteList), typeof(Vodovoz.Domain.Orders.Order))
		{
			this.UoW = uow;
		}

		public ReadyForReceptionFilter Filter {
			get {
				return RepresentationFilter as ReadyForReceptionFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}
		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes ()
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Equipment equipmentAlias = null;
			CarUnloadDocument carUnloadDocAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null, OrderEquipmentNomenclatureAlias = null, OrderNewEquipmentNomenclatureAlias = null;

			RouteList routeListAlias = null;
			RouteListItem routeListAddressAlias = null;
			ReadyForReceptionVMNode resultAlias = null;
			Employee employeeAlias = null;
			Car carAlias = null;

			List<ReadyForReceptionVMNode> items = new List<ReadyForReceptionVMNode> ();

			var orderitemsSubqury = QueryOver.Of<OrderItem> (() => orderItemsAlias)
				.Where (() => orderItemsAlias.Order.Id == orderAlias.Id)
				.JoinAlias (() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Where (() => OrderItemNomenclatureAlias.Warehouse == Filter.RestrictWarehouse)
				.Select (i => i.Order);
			var orderEquipmentSubquery = QueryOver.Of<OrderEquipment> (() => orderEquipmentAlias)
				.Where(() => orderEquipmentAlias.Order.Id == orderAlias.Id)
				.JoinAlias (() => orderEquipmentAlias.Equipment, () => equipmentAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias (() => equipmentAlias.Nomenclature, () => OrderEquipmentNomenclatureAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias (() => orderEquipmentAlias.Nomenclature, () => OrderNewEquipmentNomenclatureAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(() => OrderEquipmentNomenclatureAlias.Warehouse == Filter.RestrictWarehouse || OrderNewEquipmentNomenclatureAlias.Warehouse == Filter.RestrictWarehouse)
				.Select (i => i.Order);

				var queryRoutes = UoW.Session.QueryOver<RouteList> (() => routeListAlias)
					.JoinAlias (rl => rl.Driver, () => employeeAlias)
					.JoinAlias (rl => rl.Car, () => carAlias)
				.Where (r => routeListAlias.Status == RouteListStatus.OnClosing);

				if (Filter.RestrictWarehouse != null) {

					queryRoutes.JoinAlias (rl => rl.Addresses, () => routeListAddressAlias)
						.JoinAlias (() => routeListAddressAlias.Order, () => orderAlias)
						.Where (new Disjunction ()
							.Add (Subqueries.WhereExists (orderitemsSubqury))
							.Add (Subqueries.WhereExists (orderEquipmentSubquery))
						);
				}

				if (Filter.RestrictWithoutUnload == true)
				{
				var HasUnload = QueryOver.Of<CarUnloadDocument> (() => carUnloadDocAlias)
					.Where (() => carUnloadDocAlias.RouteList.Id == routeListAlias.Id)
					.Select (i => i.RouteList);
				
				queryRoutes.WithSubquery.WhereNotExists(HasUnload);
				}

				items.AddRange (
					queryRoutes.SelectList (list => list
						.SelectGroup (() => routeListAlias.Id).WithAlias (() => resultAlias.Id)						
						.Select (() => employeeAlias.Name).WithAlias (() => resultAlias.Name)
						.Select (() => employeeAlias.LastName).WithAlias (() => resultAlias.LastName)
						.Select (() => employeeAlias.Patronymic).WithAlias (() => resultAlias.Patronymic)
						.Select (() => carAlias.RegistrationNumber).WithAlias (() => resultAlias.Car)
						.Select (() => routeListAlias.Date).WithAlias(()=> resultAlias.Date)
					)
					.OrderBy(x => x.Date).Desc
					.TransformUsing (Transformers.AliasToBean <ReadyForReceptionVMNode> ())
					.List<ReadyForReceptionVMNode> ());

			SetItemsSource (items);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ReadyForReceptionVMNode>.Create ()
			.AddColumn ("Маршрутный лист").AddTextRenderer (node => node.Id.ToString())
			.AddColumn ("Водитель").AddTextRenderer (node => node.Driver)
			.AddColumn ("Машина").AddTextRenderer (node => node.Car)
			.AddColumn ("Дата").AddTextRenderer (node => node.Date.ToShortDateString())
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return true;
		}

		#endregion
	}

	public class ReadyForReceptionVMNode{
		[UseForSearch]
		[SearchHighlight]
		public int Id{ get; set; }

		public string Name { get; set; }

		public string LastName { get; set; }

		public string Patronymic { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Driver { get { return String.Format ("{0} {1} {2}", LastName, Name, Patronymic); } }

		[UseForSearch]
		[SearchHighlight]
		public string Car { get; set; }

		public DateTime Date { get; set; }
	}
}

