using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using QSOrmProject;
using NHibernate.Transform;
using Gamma.ColumnConfig;
using System.Linq;
using QSProjectsLib;

namespace Vodovoz.ViewModel
{
	public class WorkingDriversVM : RepresentationModelEntityBase<RouteList, WorkingDriverVMNode>
	{
		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			WorkingDriverVMNode resultAlias = null;
			Employee driverAlias = null;
			RouteList routeListAlias = null;
			Car carAlias = null;

			var query = UoW.Session.QueryOver<RouteList>(() => routeListAlias);

			var result = query
				.JoinAlias(rl => rl.Driver, () => driverAlias)
				.JoinAlias(rl => rl.Car, () => carAlias)

				.Where (rl => rl.Status == RouteListStatus.EnRoute)
				.Where (rl => rl.Driver != null)
				.Where (rl => rl.Car != null)
				.SelectList(list => list
					.Select(() => driverAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.LastName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListNumber)
			             )
				.TransformUsing(Transformers.AliasToBean<WorkingDriverVMNode>())
				.List<WorkingDriverVMNode>();

			for (int i = 0; i < result.Count; i++)
			{
				WorkingDriverVMNode item;
				item = result.FirstOrDefault(d => d.Id == result[i].Id &&
					d.RouteListNumber != result[i].RouteListNumber);
				if (item != null)
				{
					result[i].RouteListNumbers += "; " + item.RouteListNumbers;
					result.Remove(item);
					i--;
				}
			}

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<WorkingDriverVMNode>.Create()
			.AddColumn("Имя").SetDataProperty(node => node.ShortName)
			.AddColumn("Машина").SetDataProperty(node => node.CarNumber)
			.AddColumn("Маршрутные листы").SetDataProperty(node => node.RouteListNumbers)
			.Finish();

		public override IColumnsConfig ColumnsConfig
		{
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(RouteList updatedSubject)
		{
			return true;
		}

		#endregion

		public WorkingDriversVM()
			: this(UnitOfWorkFactory.CreateWithoutRoot())
		{
		}

		public WorkingDriversVM(IUnitOfWork uow)
			: base()
		{
			this.UoW = uow;
		}
	}

	public class WorkingDriverVMNode
	{
		public int Id{ get; set; }

		public string Name { get; set; }
		public string LastName { get; set; }
		public string Patronymic { get; set; }

		public string ShortName
		{ 
			get { return StringWorks.PersonNameWithInitials (LastName, Name, Patronymic);}
		}

		public string CarNumber { get; set; }

		private int routeListNumber;

		public int RouteListNumber
		{ 
			get { return routeListNumber; } 
			set
			{ 
				routeListNumber = value;
				this.RouteListNumbers = value.ToString();
			}
		}

		public string RouteListNumbers { get; set; }
	}
}