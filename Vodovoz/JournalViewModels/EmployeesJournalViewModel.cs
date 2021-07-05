using System;
using System.Linq;
using EmailService;
using InstantSmsService;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI.ServiceDlg;
using QS.Project.Journal;
using QS.Project.Repositories;
using QS.Project.Services.GtkUI;
using QS.Services;
using Vodovoz.Additions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Journals.JournalActionsViewModels;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;

namespace Vodovoz.JournalViewModels
{
	public class EmployeesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Employee, EmployeeDlg, EmployeeJournalNode, EmployeeFilterViewModel>
	{
		private readonly EmployeesJournalActionsViewModel _journalActionsViewModel;
		
		public EmployeesJournalViewModel(
			EmployeesJournalActionsViewModel journalActionsViewModel,
			EmployeeFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices
		) : base(
			journalActionsViewModel,
			filterViewModel,
			unitOfWorkFactory,
			commonServices
		)
		{
			_journalActionsViewModel = journalActionsViewModel;
			
			TabName = "Журнал сотрудников";
			var instantSmsService = InstantSmsServiceSetting.GetInstantSmsService();
		
			authorizationService = new AuthorizationService(
				new PasswordGenerator(),
				new MySQLUserRepository(
					new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()),
					new GtkInteractiveService()),
				EmailServiceSetting.GetEmailService());

			UpdateOnChanges(typeof(Employee));
			SetResetPasswordAction();
		}

		private readonly IAuthorizationService authorizationService;

		protected override Func<IUnitOfWork, IQueryOver<Employee>> ItemsSourceQueryFunction => (uow) => {
			EmployeeJournalNode resultAlias = null;
			Employee employeeAlias = null;

			var query = uow.Session.QueryOver(() => employeeAlias);

			if(FilterViewModel?.Status != null)
				query.Where(e => e.Status == FilterViewModel.Status);

			if(FilterViewModel?.Category != null)
				query.Where(e => e.Category == FilterViewModel.Category);

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
			
			var employeeProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => employeeAlias.LastName),
				Projections.Property(() => employeeAlias.Name),
				Projections.Property(() => employeeAlias.Patronymic)
			);

			query.Where(GetSearchCriterion(
				() => employeeAlias.Id,
				() => employeeProjection
			));

			var result = query
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
				.TransformUsing(Transformers.AliasToBean<EmployeeJournalNode>())
				;
			return result;
		};

		private void ResetPasswordForEmployee(Employee employee)
		{
            if (string.IsNullOrWhiteSpace(employee.Email))
            {
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
					"Нельзя сбросить пароль.\n У сотрудника не заполнено поле Email");
				return;
            }
            
			if (authorizationService.ResetPasswordToGenerated(employee.User.Login, employee.Email))
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Email с паролем отправлена успешно");
			} 
			else 
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Ошибка при отправке Email");
			}
		}

		protected override void CreatePopupActions()
		{
			base.CreatePopupActions();
			
			var resetPassAction = new JournalAction(
				"Сбросить пароль",
				x => x.FirstOrDefault() != null,
				x => true, 
				selectedItems =>
			{
				var selectedNodes = selectedItems.Cast<EmployeeJournalNode>();
				var selectedNode = selectedNodes.FirstOrDefault();
				if (selectedNode != null)
				{
					var employee = UoW.GetById<Employee>(selectedNode.Id);

					if (employee.User == null)
					{
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
							"К сотруднику не привязан пользователь!");
						
						return;
					}
					
					if (string.IsNullOrEmpty(employee.User.Login))
					{
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
							"У пользователя не заполнен логин!");
						
						return;
					}

					if (CommonServices.InteractiveService.Question("Вы уверены?"))
					{
						ResetPasswordForEmployee(employee);
					}
				}
			});
			
			PopupActionsList.Add(resetPassAction);
		}

		private void SetResetPasswordAction()
		{
			_journalActionsViewModel?.SetResetPasswordForEmployeeAction(ResetPasswordForEmployee);
		}

		protected override Func<EmployeeDlg> CreateDialogFunction => () => new EmployeeDlg();

		protected override Func<JournalEntityNodeBase, EmployeeDlg> OpenDialogFunction => 
			n => new EmployeeDlg(n.Id);
	}
}
