using MoreLinq;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.ReportsParameters.Cash
{
	public class DayOfSalaryGiveoutReportViewModel : ReportParametersViewModelBase
	{
		private DelegateCommand _unselectAllCommand;
		private DelegateCommand _selectAllCommand;
		private DelegateCommand _generateReportCommand;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWorkFactory _uowFactory;
		private DateTime? _startDateTime = DateTime.Today;

		public DayOfSalaryGiveoutReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IReportInfoFactory reportInfoFactory
		) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Дата выдачи ЗП водителей и экспедиторов";
			Identifier = "Cash.DayOfSalaryGiveout";

			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_interactiveService = (commonServices ?? throw new ArgumentNullException(nameof(commonServices))).InteractiveService;
			var hasAccess = commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_salary_reports_for_logistics");

			if(!hasAccess)
			{
				throw new AbortCreatingPageException("Нет права на просмотр этого отчета", "Недостаточно прав");
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				EmployeeNode resultAlias = null;
				Employee employeeAlias = null;

				EmployeeNodes = uow.Session.QueryOver(() => employeeAlias)
					.Where(() => employeeAlias.Status != EmployeeStatus.IsFired
					             && employeeAlias.Status != EmployeeStatus.OnCalculation
					             && (employeeAlias.Category == EmployeeCategory.driver
					                 || employeeAlias.Category == EmployeeCategory.forwarder)
					             && !employeeAlias.VisitingMaster
					)
					.SelectList(list => list
						.Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LastName)
						.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
						.Select(() => employeeAlias.Category).WithAlias(() => resultAlias.Category)
					)
					.OrderBy(e => e.LastName).Asc.ThenBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<EmployeeNode>())
					.List<EmployeeNode>();
			}
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
				{
					{ "start_date", StartDateTime },
					{ "creation_date", DateTime.Now },
					{ "employees", EmployeeNodes.Where(dn => dn.IsSelected).Select(dn => dn.Id) }
				};

				return parameters;
			}
		}

		public IList<EmployeeNode> EmployeeNodes { get; }

		[PropertyChangedAlso(nameof(CanGenerate))]
		public virtual DateTime? StartDateTime
		{
			get => _startDateTime;
			set => SetField(ref _startDateTime, value);
		}

		public bool CanGenerate => StartDateTime != null;

		public void ShowInfo()
		{
			var info = "В фильтре сотрудников представлены две группы сотрудников: водители и экспедиторы."
			           + "\nЭкспедиторы в фильтре выделены серым цветом."
			           + "\nВ фильтр и отчет не попадают сотрудники:"
			           + "\n\t- Выездные мастера;"
			           + "\n\t- Офисные сотрудники;"
			           + "\n\t- Уволенные или на расчете;"
			           + "\n\t- Сотрудники, для которых разница дат понедельника следующей недели от выбранной даты "
			           + "\n\t\tи первого рабочего дня меньше двух недель.";
			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Справка по работе с отчётом");
		}

		public DelegateCommand SelectAllCommand =>
			_selectAllCommand
			?? (_selectAllCommand = new DelegateCommand(
				() => EmployeeNodes.ForEach(dn => dn.IsSelected = true),
				() => EmployeeNodes.Any(dn => !dn.IsSelected)));

		public DelegateCommand UnselectAllCommand =>
			_unselectAllCommand
			?? (_unselectAllCommand = new DelegateCommand(
				() => EmployeeNodes.ForEach(dn => dn.IsSelected = false),
				() => EmployeeNodes.Any(dn => dn.IsSelected)));

		public DelegateCommand GenerateReportCommand =>
			_generateReportCommand
			?? (_generateReportCommand = new DelegateCommand(
				LoadReport,
				() =>
				{
					var anySelected = EmployeeNodes.Any(dn => dn.IsSelected);
					if(!anySelected)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не указан ни один сотрудник",
							"Нельзя сгенерировать отчет");
					}

					return anySelected;
				}));

		public class EmployeeNode
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public string LastName { get; set; }
			public string Patronymic { get; set; }
			public bool IsSelected { get; set; }
			public EmployeeCategory Category { get; set; }
			public string FullName => LastName + " " + Name + (string.IsNullOrWhiteSpace(Patronymic) ? "" : " " + Patronymic);
		}
	}
}
