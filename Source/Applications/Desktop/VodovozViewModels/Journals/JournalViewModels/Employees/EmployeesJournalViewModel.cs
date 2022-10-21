using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Attachments.ViewModels.Widgets;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Employees;
using VodovozInfrastructure.Endpoints;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class EmployeesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Employee, EmployeeViewModel, EmployeeJournalNode, EmployeeFilterViewModel>
	{
		private readonly IAuthorizationServiceFactory _authorizationServiceFactory;
		private readonly IAuthorizationService _authorizationService;
		private readonly IEmployeeWageParametersFactory _employeeWageParametersFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		private readonly IEmployeePostsJournalFactory _employeePostsJournalFactory;
		private readonly ICashDistributionCommonOrganisationProvider _cashDistributionCommonOrganisationProvider;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
		private readonly IWageCalculationRepository _wageCalculationRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IValidationContextFactory _validationContextFactory;
		private readonly IPhonesViewModelFactory _phonesViewModelFactory;
		private readonly DriverApiUserRegisterEndpoint _driverApiUserRegisterEndpoint;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly UserSettings _userSettings;
		private readonly IAttachmentsViewModelFactory _attachmentsViewModelFactory;

		//Новые зависимости создаем в журнале, скоро внедрим autofac
		private readonly IUserRepository _userRepository = new UserRepository();
		private readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(new ParametersProvider());

		public EmployeesJournalViewModel(
			EmployeeFilterViewModel filterViewModel,
			IAuthorizationServiceFactory authorizationServiceFactory,
			IEmployeeWageParametersFactory employeeWageParametersFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IEmployeePostsJournalFactory employeePostsJournalFactory,
			ICashDistributionCommonOrganisationProvider cashDistributionCommonOrganisationProvider,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			IWageCalculationRepository wageCalculationRepository,
			IEmployeeRepository employeeRepository,
			IWarehouseRepository warehouseRepository,
			IRouteListRepository routeListRepository,
			UserSettings userSettings,
			IValidationContextFactory validationContextFactory,
			IPhonesViewModelFactory phonesViewModelFactory,
			DriverApiUserRegisterEndpoint driverApiUserRegisterEndpoint,
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			IAttachmentsViewModelFactory attachmentsViewModelFactory) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал сотрудников";

			_authorizationServiceFactory =
				authorizationServiceFactory ?? throw new ArgumentNullException(nameof(authorizationServiceFactory));
			_authorizationService = _authorizationServiceFactory.CreateNewAuthorizationService();
			_employeeWageParametersFactory =
				employeeWageParametersFactory ?? throw new ArgumentNullException(nameof(employeeWageParametersFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_employeePostsJournalFactory =
				employeePostsJournalFactory ?? throw new ArgumentNullException(nameof(employeePostsJournalFactory));
			_cashDistributionCommonOrganisationProvider =
				cashDistributionCommonOrganisationProvider ??
				throw new ArgumentNullException(nameof(cashDistributionCommonOrganisationProvider));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			_wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));
			_phonesViewModelFactory = phonesViewModelFactory ?? throw new ArgumentNullException(nameof(phonesViewModelFactory));
			_driverApiUserRegisterEndpoint = driverApiUserRegisterEndpoint ?? throw new ArgumentNullException(nameof(driverApiUserRegisterEndpoint));
			_attachmentsViewModelFactory = attachmentsViewModelFactory ?? throw new ArgumentNullException(nameof(attachmentsViewModelFactory));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));

			UpdateOnChanges(typeof(Employee));
		}

		protected override Func<IUnitOfWork, IQueryOver<Employee>> ItemsSourceQueryFunction => (uow) => 
		{
			EmployeeJournalNode resultAlias = null;
			Employee employeeAlias = null;

			var query = uow.Session.QueryOver(() => employeeAlias);

			if(FilterViewModel?.Status != null)
			{
				query.Where(e => e.Status == FilterViewModel.Status);
			}

			if(FilterViewModel?.Category != null)
			{
				query.Where(e => e.Category == FilterViewModel.Category);
			}

			if(FilterViewModel?.RestrictWageParameterItemType != null) {
				WageParameterItem wageParameterItemAlias = null;
				var subquery = QueryOver.Of<EmployeeWageParameter>()
					.Left.JoinAlias(x => x.WageParameterItem, () => wageParameterItemAlias)
					.Where(() => wageParameterItemAlias.WageParameterItemType == FilterViewModel.RestrictWageParameterItemType.Value)
					.Where(p => p.EndDate == null || p.EndDate >= DateTime.Today)
					.Select(p => p.Employee.Id)
				;
				query.WithSubquery.WhereProperty(e => e.Id).In(subquery);
			}

			if(FilterViewModel?.DriverTerminalRelation != null)
			{
				var relation = FilterViewModel?.DriverTerminalRelation;
				DriverAttachedTerminalDocumentBase baseAlias = null;
				DriverAttachedTerminalGiveoutDocument giveoutAlias = null;
				var baseQuery = QueryOver.Of(() => baseAlias)
					.Where(doc => doc.Driver.Id == employeeAlias.Id)
					.Select(doc => doc.Id).OrderBy(doc => doc.CreationDate).Desc.Take(1);
				var giveoutQuery = QueryOver.Of(() => giveoutAlias).WithSubquery.WhereProperty(giveout => giveout.Id).Eq(baseQuery)
					.Select(doc => doc.Driver.Id);

				if(relation == DriverTerminalRelation.WithTerminal)
				{
					query.WithSubquery.WhereProperty(e => e.Id).In(giveoutQuery);
				}
				else
				{
					query.WithSubquery.WhereProperty(e => e.Id).NotIn(giveoutQuery);
				}
			}

			if(FilterViewModel?.Subdivision != null)
			{
				query.Where(e => e.Subdivision.Id == FilterViewModel.Subdivision.Id);
			}

			if(FilterViewModel?.DriverOfCarTypeOfUse != null)
			{
				query.Where(e => e.DriverOfCarTypeOfUse == FilterViewModel.DriverOfCarTypeOfUse);
			}

			if(FilterViewModel?.DriverOfCarOwnType != null)
			{
				query.Where(e => e.DriverOfCarOwnType == FilterViewModel.DriverOfCarOwnType);
			}

			//TODO переделать под новую структуру
			/*if(FilterViewModel?.RegistrationType != null)
			{
				query.Where(e => e.Registration == FilterViewModel.RegistrationType);
			}*/

			if(FilterViewModel?.HiredDatePeriodStart != null)
			{
				query.Where(e => e.DateHired >= FilterViewModel.HiredDatePeriodStart);
			}

			if(FilterViewModel?.HiredDatePeriodEnd != null)
			{
				query.Where(e => e.DateHired <= FilterViewModel.HiredDatePeriodEnd);
			}

			if(FilterViewModel?.FirstDayOnWorkStart != null)
			{
				query.Where(e => e.FirstWorkDay >= FilterViewModel.FirstDayOnWorkStart);
			}

			if(FilterViewModel?.FirstDayOnWorkEnd != null)
			{
				query.Where(e => e.FirstWorkDay <= FilterViewModel.FirstDayOnWorkEnd);
			}

			if(FilterViewModel?.FiredDatePeriodStart != null)
			{
				query.Where(e => e.DateFired >= FilterViewModel.FiredDatePeriodStart);
			}

			if(FilterViewModel?.FiredDatePeriodEnd != null)
			{
				query.Where(e => e.DateFired <= FilterViewModel.FiredDatePeriodEnd);
			}

			if(FilterViewModel?.SettlementDateStart != null)
			{
				query.Where(e => e.DateCalculated >= FilterViewModel.SettlementDateStart);
			}

			if(FilterViewModel?.SettlementDateEnd != null)
			{
				query.Where(e => e.DateCalculated <= FilterViewModel.SettlementDateEnd);
			}

			if(FilterViewModel?.IsVisitingMaster ?? false)
			{
				query.Where(e => e.VisitingMaster);
			}

			if(FilterViewModel?.IsDriverForOneDay ?? false)
			{
				query.Where(e => e.IsDriverForOneDay);
			}

			if(FilterViewModel?.IsChainStoreDriver ?? false)
			{
				query.Where(e => e.IsChainStoreDriver);
			}

			if(FilterViewModel?.IsRFCitizen ?? false)
			{
				query.Where(e => e.IsRussianCitizen);
			}

			var employeeProjection = CustomProjections.Concat_WS(
				" ",
				() => employeeAlias.LastName,
				() => employeeAlias.Name,
				() => employeeAlias.Patronymic
			);

			query.Where(GetSearchCriterion(
				() => employeeAlias.Id,
				() => employeeProjection
			));

			IQueryOver<Employee, Employee> result = null;

			if(FilterViewModel?.SortByPriority ?? false)
			{
				var endDate = DateTime.Today;
				var start3 = endDate.AddMonths(-3).AddDays(-2);
				var start2 = endDate.AddMonths(-2).AddDays(-1);
				var start1 = endDate.AddMonths(-1);
				endDate = endDate.AddDays(1).AddSeconds(-1);
				var timestampDiff = new SQLFunctionTemplate(
					NHibernateUtil.Int32, "CASE WHEN TIMESTAMPDIFF(MONTH, ?1, ?2) > 2 THEN 3 ELSE TIMESTAMPDIFF(MONTH, ?1, ?2) END");
				var avgSalary = new SQLFunctionTemplate(NHibernateUtil.Decimal, "(IFNULL(?1,0)+IFNULL(?2,0)+IFNULL(?3,0))/3");
				WagesMovementOperations wmo3 = null;
				WagesMovementOperations wmo2 = null;
				WagesMovementOperations wmo1 = null;
				const WagesType opType = WagesType.AccrualWage;

				result = query
						.SelectList(list => list
							.Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
							.Select(() => employeeAlias.Status).WithAlias(() => resultAlias.Status)
							.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmpFirstName)
							.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmpLastName)
							.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmpMiddleName)
							.Select(() => employeeAlias.Category).WithAlias(() => resultAlias.EmpCatEnum)
						//расчет стажа работы в месяцах
							.Select(Projections.SqlFunction(timestampDiff, NHibernateUtil.Int32,
								Projections.Property(() => employeeAlias.CreationDate),
								Projections.Constant(endDate))
							).WithAlias(() => resultAlias.TotalMonths)
						//расчет средней зп за последние три месяца
							.Select(Projections.SqlFunction(avgSalary, NHibernateUtil.Decimal,
									Projections.SubQuery(QueryOver.Of(() => wmo3)
										.Where(() => wmo3.Employee.Id == employeeAlias.Id)
										.And(Restrictions.Ge(Projections.Property(() => wmo3.OperationTime), start3))
										.And(Restrictions.Lt(Projections.Property(() => wmo3.OperationTime), start2))
										.And(() => wmo3.OperationType == opType)
										.Select(Projections.Sum(() => wmo3.Money))),

									Projections.SubQuery(QueryOver.Of(() => wmo2)
										.Where(() => wmo2.Employee.Id == employeeAlias.Id)
										.And(Restrictions.Ge(Projections.Property(() => wmo2.OperationTime), start2))
										.And(Restrictions.Lt(Projections.Property(() => wmo2.OperationTime), start1))
										.And(() => wmo2.OperationType == opType)
										.Select(Projections.Sum(() => wmo2.Money))),

									Projections.SubQuery(QueryOver.Of(() => wmo1)
										.Where(() => wmo1.Employee.Id == employeeAlias.Id)
										.And(Restrictions.Ge(Projections.Property(() => wmo1.OperationTime), start1))
										.And(Restrictions.Le(Projections.Property(() => wmo1.OperationTime), endDate))
										.And(() => wmo1.OperationType == opType)
										.Select(Projections.Sum(() => wmo1.Money))))
							).WithAlias(() => resultAlias.AvgSalary)

							.SelectGroup(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
						)
						.OrderByAlias(() => resultAlias.TotalMonths).Desc
						.ThenByAlias(() => resultAlias.AvgSalary).Desc
						.TransformUsing(Transformers.AliasToBean<EmployeeJournalNode>());
				return result;
			}

			result = query
					.SelectList(list => list
						.Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => employeeAlias.Status).WithAlias(() => resultAlias.Status)
						.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmpFirstName)
						.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmpLastName)
						.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmpMiddleName)
						.Select(() => employeeAlias.Category).WithAlias(() => resultAlias.EmpCatEnum)
						.SelectGroup(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
					)
					.OrderBy(x => x.LastName).Asc
					.OrderBy(x => x.Name).Asc
					.OrderBy(x => x.Patronymic).Asc
					.TransformUsing(Transformers.AliasToBean<EmployeeJournalNode>());
			return result;
		};

		private void ResetPasswordForEmployee(Employee employee)
		{
			if(string.IsNullOrWhiteSpace(employee.Email))
			{
				commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Нельзя сбросить пароль.\n У сотрудника не заполнено поле Email");
				return;
			}
			if(_authorizationService.ResetPasswordToGenerated(employee.User.Login, employee.Email, employee.FullName))
			{
				commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Email с паролем отправлена успешно");
			}
			else
			{
				commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Ошибка при отправке Email");
			}
		}

		protected override void CreatePopupActions()
		{
			base.CreatePopupActions();
			
			var resetPassAction = new JournalAction(
				"Сбросить пароль",
				(selected) =>
				{
					var selectedNodes = selected.OfType<EmployeeJournalNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}

					EmployeeJournalNode selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}

					var config = EntityConfigs[selectedNode.EntityType];

					return config.PermissionResult.CanUpdate;
				},
				x => true, 
				selectedItems =>
				{
					var selectedNodes = selectedItems.Cast<EmployeeJournalNode>();
					var selectedNode = selectedNodes.FirstOrDefault();
					if(selectedNode != null)
					{
						using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Сброс пароля пользователю"))
						{
							var employee = uow.GetById<Employee>(selectedNode.Id);

							if(employee.User == null)
							{
								commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
									"К сотруднику не привязан пользователь!");

								return;
							}

							if(string.IsNullOrEmpty(employee.User.Login))
							{
								commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
									"У пользователя не заполнен логин!");

								return;
							}

							if(commonServices.InteractiveService.Question("Вы уверены?"))
							{
								ResetPasswordForEmployee(employee);
							}
						}
					}
				});
			
			PopupActionsList.Add(resetPassAction);
			NodeActionsList.Add(resetPassAction);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateCustomEditAction();
			CreateDefaultDeleteAction();
		}

		private void CreateCustomEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => 
				{
					var selectedNodes = selected.OfType<EmployeeJournalNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}

					EmployeeJournalNode selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}

					var config = EntityConfigs[selectedNode.EntityType];

					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) => 
				{
					var selectedNodes = selected.OfType<EmployeeJournalNode>();
					
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}

					EmployeeJournalNode selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}

					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));
					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);

					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				}
			);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			NodeActionsList.Add(editAction);
		}

		protected override Func<EmployeeViewModel> CreateDialogFunction => () => new EmployeeViewModel(
			_authorizationServiceFactory.CreateNewAuthorizationService(),
			_employeeWageParametersFactory,
			_employeeJournalFactory,
			_subdivisionJournalFactory,
			_employeePostsJournalFactory,
			_cashDistributionCommonOrganisationProvider,
			_subdivisionParametersProvider,
			_wageCalculationRepository,
			_employeeRepository,
			EntityUoWBuilder.ForCreate().CreateUoW<Employee>(UnitOfWorkFactory),
			commonServices,
			_validationContextFactory,
			_phonesViewModelFactory,
			_warehouseRepository,
			_routeListRepository,
			_driverApiUserRegisterEndpoint,
			_userSettings,
			_userRepository,
			_baseParametersProvider,
			_attachmentsViewModelFactory);

		protected override Func<EmployeeJournalNode, EmployeeViewModel> OpenDialogFunction =>
			n => new EmployeeViewModel(
				_authorizationServiceFactory.CreateNewAuthorizationService(),
				_employeeWageParametersFactory,
				_employeeJournalFactory,
				_subdivisionJournalFactory,
				_employeePostsJournalFactory,
				_cashDistributionCommonOrganisationProvider,
				_subdivisionParametersProvider,
				_wageCalculationRepository,
				_employeeRepository,
				EntityUoWBuilder.ForOpen(n.Id).CreateUoW<Employee>(UnitOfWorkFactory),
				commonServices,
				_validationContextFactory,
				_phonesViewModelFactory,
				_warehouseRepository,
				_routeListRepository,
				_driverApiUserRegisterEndpoint,
				_userSettings,
				_userRepository,
				_baseParametersProvider,
				_attachmentsViewModelFactory);
	}
}
