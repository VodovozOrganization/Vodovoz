using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModel
{
	public class RouteListsVM : RepresentationModelEntityBase<RouteList, RouteListsVMNode>
	{
		public RouteListsFilter Filter {
			get {
				return RepresentationFilter as RouteListsFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			RouteListsVMNode resultAlias = null;
			RouteList routeListAlias = null;
			DeliveryShift shiftAlias = null;

			Car carAlias = null;
			Employee driverAlias = null;

			var query = UoW.Session.QueryOver<RouteList> (() => routeListAlias);

			if(Filter.RestrictStatus != null)
			{
				query.Where (o => o.Status == Filter.RestrictStatus);
			}

			if(Filter.RestrictShift != null)
			{
				query.Where (o => o.Shift == Filter.RestrictShift);
			}

			if(Filter.RestrictStartDate != null)
			{
				query.Where (o => o.Date >= Filter.RestrictStartDate);
			}

			if(Filter.RestrictEndDate != null)
			{
				query.Where (o => o.Date <= Filter.RestrictEndDate.Value.AddDays (1).AddTicks (-1));
			}

			var result = query
				.JoinAlias (o => o.Shift, () => shiftAlias)
				.JoinAlias (o => o.Car, () => carAlias)
				.JoinAlias (o => o.Driver, () => driverAlias)
				.SelectList (list => list
					.Select (() => routeListAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => routeListAlias.Date).WithAlias (() => resultAlias.Date)
					.Select (() => routeListAlias.Status).WithAlias (() => resultAlias.StatusEnum)
					.Select (() => shiftAlias.Name).WithAlias (() => resultAlias.Shift)
					.Select (() => carAlias.Model).WithAlias (() => resultAlias.CarModel)
					.Select (() => carAlias.RegistrationNumber).WithAlias (() => resultAlias.CarNumber)
					.Select (() => driverAlias.LastName).WithAlias (() => resultAlias.DriverSurname)
					.Select (() => driverAlias.Name).WithAlias (() => resultAlias.DriverName)
					.Select (() => driverAlias.Patronymic).WithAlias (() => resultAlias.DriverPatronymic)
				)
				.TransformUsing (Transformers.AliasToBean<RouteListsVMNode> ())
				.List<RouteListsVMNode> ();

			SetItemsSource (result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig <RouteListsVMNode>.Create ()
			.AddColumn ("Номер").SetDataProperty (node => node.Id.ToString())
			.AddColumn ("Дата").SetDataProperty (node => node.Date.ToString("d"))
			.AddColumn ("Смена").SetDataProperty (node => node.Shift)
			.AddColumn ("Статус").SetDataProperty (node => node.StatusEnum.GetEnumTitle ())
			.AddColumn ("Водитель и машина").SetDataProperty (node => node.DriverAndCar)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (RouteList updatedSubject)
		{
			return true;
		}

		#endregion

		public RouteListsVM (RouteListsFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public RouteListsVM () : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new RouteListsFilter(UoW);
		}

		public RouteListsVM (IUnitOfWork uow) : base ()
		{
			this.UoW = uow;
		}
	}

	public class RouteListsVMNode
	{
		[UseForSearch]
		public int Id { get; set; }

		public RouteListStatus StatusEnum { get; set; }

		public string Shift { get; set; }

		public DateTime Date { get; set; }

		public string DriverSurname { get; set; }
		public string DriverName { get; set; }
		public string DriverPatronymic { get; set; }

		public string Driver { get{ return StringWorks.PersonFullName(DriverSurname, DriverName, DriverPatronymic);
		} }

		public string CarModel { get; set; }

		public string CarNumber { get; set; }

		[UseForSearch]
		public string DriverAndCar { get{ return String.Format("{0} - {1} ({2})", Driver, CarModel, CarNumber);
			} }
	}
}