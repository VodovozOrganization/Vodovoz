using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class EmployeesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Employee, EmployeeViewModel, EmployeeJournalNode, EmployeeFilterViewModel>
	{
		private readonly IAuthorizationServiceFactory _authorizationServiceFactory;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IAuthorizationService _authorizationService;

		public EmployeesJournalViewModel(
			EmployeeFilterViewModel filterViewModel,
			IAuthorizationServiceFactory authorizationServiceFactory,
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager,
			Action<EmployeeFilterViewModel> filterparams = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices, false, false, navigationManager)
		{
			TabName = "Журнал сотрудников";

			_authorizationServiceFactory =
				authorizationServiceFactory ?? throw new ArgumentNullException(nameof(authorizationServiceFactory));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_authorizationService = _authorizationServiceFactory.CreateNewAuthorizationService();

			filterViewModel.JournalViewModel = this;
			JournalFilter = filterViewModel;

			if(filterparams != null)
			{
				FilterViewModel.SetAndRefilterAtOnce(filterparams);
			}

			UpdateOnChanges(typeof(Employee));
		}

		protected override Func<IUnitOfWork, IQueryOver<Employee>> ItemsSourceQueryFunction => (uow) => 
		{
			EmployeeJournalNode resultAlias = null;
			Employee employeeAlias = null;
			EmployeeRegistrationVersion employeeRegistrationVersionAlias = null;
			EmployeeRegistration employeeRegistrationAlias = null;
			var currentDateTime = DateTime.Now;
				
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

			if(FilterViewModel?.RegistrationType != null)
			{
				query.JoinAlias(e => e.EmployeeRegistrationVersions, () => employeeRegistrationVersionAlias, JoinType.InnerJoin,
					Restrictions.Where(() => employeeRegistrationVersionAlias.StartDate <= currentDateTime
						&& (employeeRegistrationVersionAlias.EndDate == null || employeeRegistrationVersionAlias.EndDate >= currentDateTime)))
					.JoinAlias(() => employeeRegistrationVersionAlias.EmployeeRegistration, () => employeeRegistrationAlias)
					.Where(() => employeeRegistrationAlias.RegistrationType == FilterViewModel.RegistrationType);
			}

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
			CreateCustomAddActions();
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

					NavigationManager.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(selectedNode.Id));
				}
			);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			NodeActionsList.Add(editAction);
		}

		private void CreateCustomAddActions()
		{
			var createAction = new JournalAction("Создать",
				(selected) =>
				{
					var config = EntityConfigs[typeof(Employee)];

					return config.PermissionResult.CanCreate;
				},
				(selected) => true,
				(selected) =>
				{
					if(!EntityConfigs.ContainsKey(typeof(Employee)))
					{
						return;
					}

					NavigationManager.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
				}
			);

			NodeActionsList.Add(createAction);
		}

		protected override Func<EmployeeViewModel> CreateDialogFunction =>
			() => _lifetimeScope.Resolve<EmployeeViewModel>(new TypedParameter[] { new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForCreate()) });

		protected override Func<EmployeeJournalNode, EmployeeViewModel> OpenDialogFunction =>
			(node) => _lifetimeScope.Resolve<EmployeeViewModel>(new TypedParameter[] { new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(node.Id)) });

		public ILifetimeScope Scope => _lifetimeScope;

		public override void Dispose()
		{
			FilterViewModel = null;
			base.Dispose();
		}
	}
}
