using System;
using QS.Dialog.GtkUI;
using QSReport;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using QS.DomainModel.UoW;
using NHibernate.Transform;
using System.Linq;
using Vodovoz.Domain.Employees;
using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.ViewModels.Reports;
using System.Collections.Generic;
using QS.Report;
using Gamma.ColumnConfig;
using QS.DomainModel.Entity;
using System.Data.Bindings.Collections.Generic;
using System.Text;
using Gamma.Utilities;

namespace Vodovoz.ReportsParameters.Employees
{
	public partial class EmployeesTaxesSumReport : SingleUoWWidgetBase, IParametersWidget
	{
		private const string _employeesParameterSet = "employees";
		private const string _subdivisionsParameterSet = "subdivisions";
		private readonly SelectableParametersReportFilter _filter;
		private readonly IReportInfoFactory _reportInfoFactory;
		private GenericObservableList<SelectableRegistrationTypeNode> _selectableRegistrationTypes;
		private GenericObservableList<SelectablePaymentFormNode> _selectablePaymentForms;

		public string Title => "Отчет по сумме налогов";
		public event EventHandler<LoadReportEventArgs> LoadReport;

		public EmployeesTaxesSumReport(IReportInfoFactory reportInfoFactory, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			UoW = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory))).CreateWithoutRoot();

			Build();
			_filter = new SelectableParametersReportFilter(UoW);
			Configure();
		}

		private void Configure()
		{
			ConfigureDatePicker();
			lblRegistrationTypes.WidthRequest = 200;
			lblPaymentForm.WidthRequest = 100;

			ConfigureTreeViews();
			SetupFilter();
		}

		private void ConfigureDatePicker()
		{
			var previousMonth = DateTime.Today.AddMonths(-1).Month;
			var year = DateTime.Today.Month == 1 ? DateTime.Today.AddYears(-1).Year : DateTime.Today.Year;
			var endDay = DateTime.Today.AddDays(-DateTime.Today.Day).Day;

			dateperiodpicker.StartDate = new DateTime(year, previousMonth, 1);
			dateperiodpicker.EndDate = new DateTime(year, previousMonth, endDay, 23, 59, 59);
		}

		private void ConfigureTreeViews()
		{
			FillSources();

			treeViewRegistrationTypes.ColumnsConfig = FluentColumnsConfig<SelectableRegistrationTypeNode>.Create()
				.AddColumn("")
					.AddToggleRenderer(n => n.IsSelected)
					.Editing()
				.AddColumn("")
					.AddEnumRenderer(n => n.RegistrationType)
					.Editing(false)
				.Finish();
			GtkScrolledWindow.HeightRequest = 120;
			treeViewRegistrationTypes.ItemsDataSource = _selectableRegistrationTypes;

			treeViewPaymentForms.ColumnsConfig = FluentColumnsConfig<SelectablePaymentFormNode>.Create()
				.AddColumn("")
					.AddToggleRenderer(n => n.IsSelected)
					.Editing()
				.AddColumn("")
					.AddEnumRenderer(n => n.PaymentForm)
					.Editing(false)
				.Finish();

			GtkScrolledWindow1.HeightRequest = 85;
			treeViewPaymentForms.ItemsDataSource = _selectablePaymentForms;
		}

		private void FillSources()
		{
			_selectableRegistrationTypes = new GenericObservableList<SelectableRegistrationTypeNode>();

			foreach(var item in Enum.GetValues(typeof(RegistrationType)).OfType<RegistrationType>())
			{
				var newItem = new SelectableRegistrationTypeNode
				{
					IsSelected = true,
					RegistrationType = item
				};
				_selectableRegistrationTypes.Add(newItem);
			}

			_selectablePaymentForms = new GenericObservableList<SelectablePaymentFormNode>();

			foreach(var item in Enum.GetValues(typeof(PaymentForm)).OfType<PaymentForm>())
			{
				var newItem = new SelectablePaymentFormNode
				{
					IsSelected = true,
					PaymentForm = item
				};
				_selectablePaymentForms.Add(newItem);
			}
		}

		private void SetupFilter()
		{
			var subdivisionsFilter = _filter.CreateParameterSet(
				"Подразделения",
				_subdivisionsParameterSet,
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Subdivision> resultAlias = null;
					var query = UoW.Session.QueryOver<Subdivision>();
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle))
						.OrderBy(x => x.Name).Asc
						.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Subdivision>>());
						
					return query.List<SelectableParameter>();
				})
			);

			var employeesFilter = _filter.CreateParameterSet(
				"Сотрудники",
				_employeesParameterSet,
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Employee> resultAlias = null;
					var query = UoW.Session.QueryOver<Employee>();

					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					var employeeProjection = CustomProjections.Concat_WS(
						" ",
						Projections.Property<Employee>(x => x.LastName),
						Projections.Property<Employee>(x => x.Name),
						Projections.Property<Employee>(x => x.Patronymic)
					);

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(employeeProjection).WithAlias(() => resultAlias.EntityTitle))
						.OrderBy(x => x.LastName).Asc
						.OrderBy(x => x.Name).Asc
						.OrderBy(x => x.Patronymic).Asc
						.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Employee>>());
					
					var paremetersSet = query.List<SelectableParameter>();
					return paremetersSet;
				})
			);
			
			employeesFilter.AddFilterOnSourceSelectionChanged(subdivisionsFilter,
				() =>
				{
					var selectedValues = subdivisionsFilter.GetSelectedValues().ToArray();

					return !selectedValues.Any()
						? Restrictions.Gt(Projections.Property<Employee>(e => e.Id), 0)
						: subdivisionsFilter.FilterType == SelectableFilterType.Include
							? Restrictions.On<Employee>(x => x.Subdivision).IsIn(selectedValues)
							: Restrictions.On<Employee>(x => x.Subdivision).Not.IsIn(selectedValues);
				}
			);

			var viewModel = new SelectableParameterReportFilterViewModel(_filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}

		private ReportInfo GetReportInfo()
		{
			var sb = new StringBuilder();
			GetSelectedRegistrationTypes(out var selectedRegistrationTypes, out var selectedRegistrationTypesString, sb);
			GetSelectedPaymentForms(out var selectedPaymentForms, out var selectedPaymentFormsString, sb);
			
			var filterParameters = _filter.GetParameters();

			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{ "registration_types", selectedRegistrationTypes },
				{ "registration_types_string", selectedRegistrationTypesString },
				{ "payment_forms", selectedPaymentForms },
				{ "payment_forms_string", selectedPaymentFormsString },
				{ "selected_employees",
					GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(_employeesParameterSet), sb) },
				{ "selected_subdivisions",
					GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(_subdivisionsParameterSet), sb) }
			};

			foreach(var item in filterParameters)
			{
				parameters.Add(item.Key, item.Value);
			}

			var title = $"{Title} с {dateperiodpicker.StartDate:dd-MM-yyyy} по {dateperiodpicker.EndDate:dd-MM-yyyy}";
			var reportInfo = _reportInfoFactory.Create("Employees.EmployeesTaxesSumReport", title, parameters);
			return reportInfo;
		}

		private void GetSelectedRegistrationTypes(out IList<string> selectedNodes, out string selectedNodesString, StringBuilder sb)
		{
			selectedNodes = new List<string>();
			selectedNodesString = string.Empty;
			sb.Clear();
			if(_selectableRegistrationTypes.Any(x => x.IsSelected))
			{
				foreach(var node in _selectableRegistrationTypes.Where(x => x.IsSelected))
				{
					FillSelectedValues(selectedNodes, sb, node.RegistrationType);
				}
				selectedNodesString = sb.ToString().TrimEnd(' ', ',');
			}
			else
			{
				FillEmptyValues(selectedNodes, out selectedNodesString);
			}
		}

		private void GetSelectedPaymentForms(out IList<string> selectedNodes, out string selectedNodesString, StringBuilder sb)
		{
			selectedNodes = new List<string>();
			selectedNodesString = string.Empty;
			sb.Clear();
			if(_selectablePaymentForms.Any(x => x.IsSelected))
			{
				foreach(var node in _selectablePaymentForms.Where(x => x.IsSelected))
				{
					FillSelectedValues(selectedNodes, sb, node.PaymentForm);
				}
				selectedNodesString = sb.ToString().TrimEnd(' ', ',');
			}
			else
			{
				FillEmptyValues(selectedNodes, out selectedNodesString);
			}
		}
		
		private void FillSelectedValues(IList<string> selectedNodes, StringBuilder stringBuilder, Enum node)
		{
			selectedNodes.Add(node.ToString());
			stringBuilder.Append($"{node.GetEnumTitle()}, ");
		}
		
		private void FillEmptyValues(IList<string> selectedNodes, out string selectedNodesString)
		{
			selectedNodes.Add("Empty");
			selectedNodesString = "Нет";
		}

		private string GetSelectedParametersTitles(IDictionary<string, string> selectedParametersTitles, StringBuilder sb)
		{
			sb.Clear();
			
			if(selectedParametersTitles.Any())
			{
				foreach(var item in selectedParametersTitles)
				{
					sb.AppendLine($"{item.Key}{item.Value}");
				}
			}

			return sb.ToString().TrimEnd('\n');
		}
	}

	public class SelectableRegistrationTypeNode : PropertyChangedBase
	{
		private bool _isSelected;
		private RegistrationType _registrationType;

		public bool IsSelected
		{
			get => _isSelected;
			set => SetField(ref _isSelected, value);
		}

		public RegistrationType RegistrationType
		{
			get => _registrationType;
			set => SetField(ref _registrationType, value);
		}
	}

	public class SelectablePaymentFormNode : PropertyChangedBase
	{
		private bool _isSelected;
		private PaymentForm _paymentForm;

		public bool IsSelected
		{
			get => _isSelected;
			set => SetField(ref _isSelected, value);
		}

		public PaymentForm PaymentForm
		{
			get => _paymentForm;
			set => SetField(ref _paymentForm, value);
		}
	}
}
