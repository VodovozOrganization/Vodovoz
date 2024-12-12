using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModel
{
	public class ReadyForReceptionVM : RepresentationModelWithoutEntityBase<ReadyForReceptionVMNode>
	{
		public ReadyForReceptionVM() : this(ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot()) { }

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
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			CarUnloadDocument carUnloadDocAlias = null;

			RouteList routeListAlias = null;
			RouteListItem routeListAddressAlias = null;
			ReadyForReceptionVMNode resultAlias = null;
			Employee employeeAlias = null;
			Car carAlias = null;

			List<ReadyForReceptionVMNode> items = new List<ReadyForReceptionVMNode>();

			var queryRoutes = UoW.Session.QueryOver(() => routeListAlias)
				.JoinAlias(rl => rl.Driver, () => employeeAlias)
				.JoinAlias(rl => rl.Car, () => carAlias)
				.Where(r => routeListAlias.Status == RouteListStatus.OnClosing 
						 || routeListAlias.Status == RouteListStatus.MileageCheck
						 || routeListAlias.Status == RouteListStatus.Delivered);
			
			var startDate = Filter.StartDate;
			var endDate = Filter.EndDate;

			if(startDate.HasValue)
			{
				queryRoutes.Where(() => routeListAlias.Date >= startDate);
			}

			if(endDate.HasValue)
			{
				queryRoutes.Where(() => routeListAlias.Date <= endDate);
			}

			if(Filter.Warehouse != null)
			{
				queryRoutes
					.Left.JoinAlias(rl => rl.Addresses, () => routeListAddressAlias)
					.JoinEntityAlias(
						() => orderItemsAlias,
						() => orderItemsAlias.Order.Id == routeListAddressAlias.Order.Id,
						JoinType.LeftOuterJoin)
					.JoinEntityAlias(
						() => orderEquipmentAlias,
						() => orderEquipmentAlias.Order.Id == routeListAddressAlias.Order.Id,
						JoinType.LeftOuterJoin)
					.Where(() => orderItemsAlias.Id != null || orderEquipmentAlias.Id != null);
			}

			if(Filter.RestrictWithoutUnload == true) {
				var HasUnload = QueryOver.Of<CarUnloadDocument>(() => carUnloadDocAlias)
					.Where(() => carUnloadDocAlias.RouteList.Id == routeListAlias.Id)
					.Select(i => i.RouteList);

				queryRoutes.WithSubquery.WhereNotExists(HasUnload);
			}

			items.AddRange(
				queryRoutes.SelectList(list => list
				   .SelectGroup(() => routeListAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.Name)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LastName)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
				   .Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.Car)
				   .Select(() => routeListAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => routeListAlias.Status).WithAlias(() => resultAlias.Status)
				)
				.OrderBy(x => x.Date).Desc
				.ThenBy(x => x.Id).Desc
				.TransformUsing(Transformers.AliasToBean<ReadyForReceptionVMNode>())
				.List<ReadyForReceptionVMNode>());

			SetItemsSource(items);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ReadyForReceptionVMNode>.Create()
			.AddColumn("Маршрутный лист").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Водитель").AddTextRenderer(node => node.Driver)
			.AddColumn("Машина").AddTextRenderer(node => node.Car)
			.AddColumn("Дата").AddTextRenderer(node => node.Date.ToShortDateString())
			.AddColumn("Статус МЛ").AddTextRenderer(node => node.Status.GetEnumTitle())
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

		public RouteListStatus Status { get; set; }

		public string Patronymic { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Driver => String.Format("{0} {1} {2}", LastName, Name, Patronymic);

		[UseForSearch]
		[SearchHighlight]
		public string Car { get; set; }

		public DateTime Date { get; set; }
	}
}
