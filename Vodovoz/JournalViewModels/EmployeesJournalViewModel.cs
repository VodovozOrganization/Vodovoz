using System;
using System.Linq;
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
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.Tools;

namespace Vodovoz.JournalViewModels
{
	public class EmployeesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Employee, EmployeeDlg, EmployeeJournalNode, EmployeeFilterViewModel>
	{
		public EmployeesJournalViewModel(
			EmployeeFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices
		) : base(
			filterViewModel,
			unitOfWorkFactory,
			commonServices
		)
		{
			this.TabName = "Журнал сотрудников";
			var instantSmsService = InstantSmsServiceSetting.GetInstantSmsService();
		
			this.authorizationService = new AuthorizationService(
				new PasswordGenerator(),
				new MySQLUserRepository(
					new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()),
					new GtkInteractiveService()));
				UpdateOnChanges(typeof(Employee));
		}

		private readonly IAuthorizationService authorizationService;

		protected override Func<IUnitOfWork, IQueryOver<Employee>> ItemsSourceQueryFunction => (uow) => {
			EmployeeJournalNode resultAlias = null;
			Employee employeeAlias = null;
			DriverWorkSchedule drvWorkScheduleAlias = null;
			DeliveryDaySchedule dlvDayScheduleAlias = null;
			DeliveryShift shiftAlias = null;

			var query = uow.Session.QueryOver(() => employeeAlias);

			if(FilterViewModel?.Status != null)
				query.Where(e => e.Status == FilterViewModel.Status);

			if(FilterViewModel?.DrvStartTime != null && FilterViewModel.DrvEndTime != null && FilterViewModel.WeekDay != null) {
				query.Left.JoinAlias(() => employeeAlias.WorkDays, () => drvWorkScheduleAlias)
					 .Left.JoinAlias(() => drvWorkScheduleAlias.DaySchedule, () => dlvDayScheduleAlias)
					 .Left.JoinAlias(() => dlvDayScheduleAlias.Shifts, () => shiftAlias)
					 .Where(() => (int)drvWorkScheduleAlias.WeekDay == (int)FilterViewModel.WeekDay.Value.DayOfWeek
								   && shiftAlias.StartTime >= FilterViewModel.DrvStartTime
								   && shiftAlias.StartTime <= FilterViewModel.DrvEndTime);
			}

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
			var passGenerator = new PasswordGenerator();
			var result = authorizationService.ResetPassword(employee, passGenerator.GeneratePassword(5));
			if (result.MessageStatus == SmsMessageStatus.Ok)
			{
				MessageDialogHelper.RunInfoDialog("Sms с паролем отправлена успешно");
			} else {
				MessageDialogHelper.RunErrorDialog(result.ErrorDescription, "Ошибка при отправке Sms");
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
						commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
							"К сотруднику не привязан пользователь!");
						
						return;
					}
					
					if (string.IsNullOrEmpty(employee.User.Login))
					{
						commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
							"У пользователя не заполнен логин!");
						
						return;
					}

					if (commonServices.InteractiveService.Question("Вы уверены?"))
					{
						ResetPasswordForEmployee(employee);
					}
				}
			});
			
			PopupActionsList.Add(resetPassAction);
			NodeActionsList.Add(resetPassAction);
		}

		protected override Func<EmployeeDlg> CreateDialogFunction => () => new EmployeeDlg();

		protected override Func<EmployeeJournalNode, EmployeeDlg> OpenDialogFunction => 
			n => new EmployeeDlg(n.Id);
	}
}
