using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Deletion;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class CarEventJournalViewModel : FilterableSingleEntityJournalViewModelBase<CarEvent, CarEventViewModel, CarEventJournalNode,
		CarEventFilterViewModel>
	{
		private readonly ICarJournalFactory _carJournalFactory;
		private readonly ICarEventTypeJournalFactory _carEventTypeJournalFactory;
		private readonly ICarEventJournalFactory _carEventJournalFactory;
		private readonly IEmployeeService _employeeService;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICarEventSettings _carEventSettings;
		private readonly ILifetimeScope _lifetimeScope;
		private bool _canChangeWithClosedPeriod;
		private int _startNewPeriodDay;

		public CarEventJournalViewModel(
			CarEventFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICarJournalFactory carJournalFactory,
			ICarEventTypeJournalFactory carEventTypeJournalFactory,
			ICarEventJournalFactory carEventJournalFactory,
			IEmployeeService employeeService,
			IEmployeeJournalFactory employeeJournalFactory,
			ICarEventSettings carEventSettings,
			ILifetimeScope lifetimeScope)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал событий ТС";

			_carJournalFactory = carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory));
			_carEventTypeJournalFactory = carEventTypeJournalFactory ?? throw new ArgumentNullException(nameof(carEventTypeJournalFactory));
			_carEventJournalFactory = carEventJournalFactory ?? throw new ArgumentNullException(nameof(carEventJournalFactory));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_canChangeWithClosedPeriod = commonServices.CurrentPermissionService.ValidatePresetPermission("can_create_edit_car_events_in_closed_period");
			_startNewPeriodDay = _carEventSettings.CarEventStartNewPeriodDay;

			UpdateOnChanges(
				typeof(CarEvent),
				typeof(CarEventType));
		}

		protected override void CreateNodeActions()
		{
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
			CreateCustomDeleteAction();
		}

		private void CreateCustomDeleteAction()
		{
			var deleteAction = new JournalAction("Удалить",
				   (selected) =>
				   {
					   var selectedNodes = selected.OfType<CarEventJournalNode>();
					   if(selectedNodes == null || selectedNodes.Count() != 1)
					   {
						   return false;
					   }
					   CarEventJournalNode selectedNode = selectedNodes.First();
					   if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					   {
						   return false;
					   }
					   if(!CanDelete(selectedNode.EndDate))
					   {
						   return false;
					   }
					   var config = EntityConfigs[selectedNode.EntityType];
					   return config.PermissionResult.CanDelete;
				   },
				   (selected) => true,
				   (selected) =>
				   {
					   var selectedNodes = selected.OfType<CarEventJournalNode>();
					   if(selectedNodes == null || selectedNodes.Count() != 1)
					   {
						   return;
					   }
					   CarEventJournalNode selectedNode = selectedNodes.First();
					   if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					   {
						   return;
					   }
					   if(!CanDelete(selectedNode.EndDate))
					   {
						   return;
					   }
					   var config = EntityConfigs[selectedNode.EntityType];
					   if(config.PermissionResult.CanDelete)
					   {
						   DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
					   }
				   },
				   "Delete"
			   );
			NodeActionsList.Add(deleteAction);
		}

		protected override Func<IUnitOfWork, IQueryOver<CarEvent>> ItemsSourceQueryFunction => (uow) =>
		{
			CarEvent carEventAlias = null;
			CarEventType carEventTypeAlias = null;
			Employee authorAlias = null;
			Employee driverAlias = null;
			Car carAlias = null;
			GeoGroup geographicGroupAlias = null;
			CarEventJournalNode resultAlias = null;
			CarVersion carVersionAlias = null;
			CarModel carModelAlias = null;

			var authorProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var driverProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => driverAlias.LastName),
				Projections.Property(() => driverAlias.Name),
				Projections.Property(() => driverAlias.Patronymic)
			);

			var geographicGroupsProjection = CustomProjections.GroupConcat(
				() => geographicGroupAlias.Name,
				orderByExpression: () => geographicGroupAlias.Name, separator: ", "
			);

			var itemsQuery = uow.Session.QueryOver(() => carEventAlias);
			itemsQuery.Left.JoinAlias(x => x.CarEventType, () => carEventTypeAlias);
			itemsQuery.Left.JoinAlias(x => x.Author, () => authorAlias);
			itemsQuery.Left.JoinAlias(x => x.Driver, () => driverAlias);
			itemsQuery.Left.JoinAlias(x => x.Car, () => carAlias);
			itemsQuery.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias);
			itemsQuery.Left.JoinAlias(() => carAlias.GeographicGroups, () => geographicGroupAlias);
			itemsQuery.JoinEntityAlias(
				() => carVersionAlias,
				() => carVersionAlias.Car.Id == carAlias.Id
					&& carVersionAlias.StartDate <= carEventAlias.StartDate
					&& (carVersionAlias.EndDate == null || carVersionAlias.EndDate >= carEventAlias.StartDate),
				JoinType.LeftOuterJoin);

			if(FilterViewModel.CarEventType != null)
			{
				itemsQuery.Where(x => x.CarEventType == FilterViewModel.CarEventType);
			}

			if(FilterViewModel.CreateEventDateFrom != null && FilterViewModel.CreateEventDateTo != null)
			{
				itemsQuery.Where(x => x.CreateDate >= FilterViewModel.CreateEventDateFrom.Value.Date.Add(new TimeSpan(0, 0, 0, 0)) &&
									  x.CreateDate <= FilterViewModel.CreateEventDateTo.Value.Date.Add(new TimeSpan(0, 23, 59, 59)));
			}

			if(FilterViewModel.StartEventDateFrom != null && FilterViewModel.StartEventDateTo != null)
			{
				itemsQuery.Where(x => x.StartDate >= FilterViewModel.StartEventDateFrom &&
									  x.StartDate <= FilterViewModel.StartEventDateTo);
			}

			if(FilterViewModel.EndEventDateFrom != null && FilterViewModel.EndEventDateTo != null)
			{
				itemsQuery.Where(x => x.EndDate >= FilterViewModel.EndEventDateFrom &&
									  x.EndDate <= FilterViewModel.EndEventDateTo);
			}

			if(FilterViewModel.Author != null)
			{
				itemsQuery.Where(x => x.Author == FilterViewModel.Author);
			}

			if(FilterViewModel.Car != null)
			{
				itemsQuery.Where(() => carAlias.Id == FilterViewModel.Car.Id);
			}

			if(FilterViewModel.Driver != null)
			{
				itemsQuery.Where(x => x.Driver == FilterViewModel.Driver);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => carEventAlias.Id,
				() => carEventAlias.Comment,
				() => carEventTypeAlias.Name,
				() => carEventTypeAlias.ShortName,
				() => carModelAlias.Name,
				() => carAlias.RegistrationNumber,
				() => driverProjection)
			);

			itemsQuery.OrderBy(() => carEventAlias.CreateDate).Desc();

			itemsQuery
				.SelectList(list => list
					.SelectGroup(() => carEventAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => carEventAlias.CreateDate).WithAlias(() => resultAlias.CreateDate)
					.Select(() => carEventAlias.StartDate).WithAlias(() => resultAlias.StartDate)
					.Select(() => carEventAlias.EndDate).WithAlias(() => resultAlias.EndDate)
					.Select(() => carEventAlias.RepairCost).WithAlias(() => resultAlias.RepairCost)
					.Select(() => carEventAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => carEventTypeAlias.Name).WithAlias(() => resultAlias.CarEventTypeName)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarRegistrationNumber)
					.Select(() => carAlias.OrderNumber).WithAlias(() => resultAlias.CarOrderNumber)
					.Select(() => carModelAlias.CarTypeOfUse).WithAlias(() => resultAlias.CarTypeOfUse)
					.Select(() => carVersionAlias.CarOwnType).WithAlias(() => resultAlias.CarOwnType)
					.Select(authorProjection).WithAlias(() => resultAlias.AuthorFullName)
					.Select(driverProjection).WithAlias(() => resultAlias.DriverFullName)
					.Select(geographicGroupsProjection).WithAlias(() => resultAlias.GeographicGroups)
				)
				.TransformUsing(Transformers.AliasToBean<CarEventJournalNode>());

			return itemsQuery;
		};

		protected override Func<CarEventViewModel> CreateDialogFunction =>
			() => new CarEventViewModel(
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory,
				commonServices,
				_carJournalFactory,
				_carEventTypeJournalFactory,
				_carEventJournalFactory,
				_employeeService,
				_employeeJournalFactory,
				_carEventSettings,
				_lifetimeScope);

		protected override Func<CarEventJournalNode, CarEventViewModel> OpenDialogFunction =>
			node => new CarEventViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				UnitOfWorkFactory,
				commonServices,
				_carJournalFactory,
				_carEventTypeJournalFactory,
				_carEventJournalFactory,
				_employeeService,
				_employeeJournalFactory,
				_carEventSettings,
				_lifetimeScope);

		private bool CanDelete(DateTime endDate)
		{
			if(_canChangeWithClosedPeriod)
			{
				return true;
			}

			var today = DateTime.Now;
			DateTime startCurrentMonth = new DateTime(today.Year, today.Month, 1);
			DateTime startPreviousMonth = startCurrentMonth.AddMonths(-1);
			if(today.Day <= _startNewPeriodDay && endDate > startPreviousMonth)
			{
				return true;
			}

			if(today.Day > _startNewPeriodDay && endDate >= startCurrentMonth)
			{
				return true;
			}

			return false;
		}
	}
}
