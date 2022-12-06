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

namespace Vodovoz.ReportsParameters.Employees
{
	public partial class EmployeesTaxesSumReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly SelectableParametersReportFilter _filter;
		private GenericObservableList<SelectableRegistrationTypeNode> _selectableRegistrationTypes;
		private GenericObservableList<SelectablePaymentFormNode> _selectablePaymentForms;

		public string Title => "Отчет по сумме налогов";
		public event EventHandler<LoadReportEventArgs> LoadReport;

		public EmployeesTaxesSumReport(
			IUnitOfWorkFactory unitOfWorkFactory)
		{
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
			int year;

			if(DateTime.Today.Month == 1)
			{
				year = DateTime.Today.AddYears(-1).Year;
			}
			else
			{
				year = DateTime.Today.Year;
			}

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
			_filter.CreateParameterSet(
				"Подразделения",
				"subdivisions",
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
						.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Subdivision>>());
					return query.List<SelectableParameter>();
				})
			);

			_filter.CreateParameterSet(
				"Сотрудники",
				"employees",
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
						.Select(employeeProjection).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Employee>>());
					var paremetersSet = query.List<SelectableParameter>();

					return paremetersSet;
				})
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
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{ "registration_types", _selectableRegistrationTypes.Where(x => x.IsSelected).Select(x => x.RegistrationType)},
				{ "payment_forms", _selectablePaymentForms.Where(x => x.IsSelected).Select(x => x.PaymentForm)},
			};

			foreach(var item in _filter.GetParameters())
			{
				parameters.Add(item.Key, item.Value);
			}

			return new ReportInfo
			{
				Identifier = "Employee.EmployeesTaxesSumReport",
				Parameters = parameters
			};
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
