using QS.Commands;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Logistics
{
	public class ShipmentReportViewModel : ValidatableUoWReportViewModelBase
	{
		private DateTime? _startDate;
		private Warehouse _warehouse;
		private bool _all;
		private bool _sortByCash;
		private bool _sortByWarehouse;

		public ShipmentReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			Title = "Отчёт по отгрузке автомобилей";
			Identifier = "Logistic.ShipmentReport";

			UoW = _uowFactory.CreateWithoutRoot();
			_all = true;

			_startDate = DateTime.Today;

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;
		private readonly IUnitOfWorkFactory _uowFactory;

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
		}

		public virtual bool All
		{
			get => _all;
			set => SetField(ref _all, value);
		}

		public virtual bool SortByCash
		{
			get => _sortByCash;
			set => SetField(ref _sortByCash, value);
		}

		public virtual bool SortByWarehouse
		{
			get => _sortByWarehouse;
			set => SetField(ref _sortByWarehouse, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "department", GetDepartment() }
					};

				return parameters;
			}
		}

		string GetDepartment()
		{
			if(All)
			{
				return "-1";
			}

			if(SortByCash)
			{
				return "Касса";
			}

			return Warehouse.Name;
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == null)
			{
				yield return new ValidationResult("Необходимо выбрать период.", new[] { nameof(StartDate) });
			}

			if(SortByWarehouse && Warehouse == null)
			{
				yield return new ValidationResult("Необходимо выбрать склад.", new[] { nameof(Warehouse) });
			}
		}
	}
}

