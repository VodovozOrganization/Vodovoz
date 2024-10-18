using QS.Commands;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Sale;
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.QualityControl
{
	public class TariffZoneDebtsViewModel : ValidatableUoWReportViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		private DateTime _startDate;
		private DateTime _endDate;
		private int _debtFrom;
		private int _debtTo;
		private IEnumerable<TariffZone> _tariffZones;
		private TariffZone _tariffZone;

		public TariffZoneDebtsViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			Title = "Отчет по тарифным зонам";
			Identifier = "Client.TariffZoneDebts";

			UoW = _uowFactory.CreateWithoutRoot();

			GenerateReportCommand = new DelegateCommand(GenerateReport);

			_startDate = DateTime.Today.AddMonths(-1);
			_endDate = DateTime.Today;
			_tariffZones = UoW.GetAll<TariffZone>().ToList();
			_debtTo = 10;
		}

		public DelegateCommand GenerateReportCommand;

		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public virtual int DebtFrom
		{
			get => _debtFrom;
			set => SetField(ref _debtFrom, value);
		}

		public virtual int DebtTo
		{
			get => _debtTo;
			set => SetField(ref _debtTo, value);
		}

		public virtual IEnumerable<TariffZone> TariffZones
		{
			get => _tariffZones;
			private set => SetField(ref _tariffZones, value);
		}

		public virtual TariffZone TariffZone
		{
			get => _tariffZone;
			set => SetField(ref _tariffZone, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "date_from", StartDate },
						{ "date_to", EndDate },
						{ "debt_from", DebtFrom },
						{ "debt_to", DebtTo },
						{ "tariff_zone_id", TariffZone?.Id ?? 0 }
					};

				return parameters;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(TariffZone == null)
			{
				yield return new ValidationResult("Необходимо выбрать тарифную зону.", new[] { nameof(TariffZone) });
			}
		}
	}
}
