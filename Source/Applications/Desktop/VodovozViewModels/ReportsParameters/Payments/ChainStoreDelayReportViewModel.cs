using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.Reports;
using Vodovoz.Settings.Counterparty;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ReportsParameters.Payments
{
	public class ChainStoreDelayReportViewModel : ValidatableUoWReportViewModelBase
	{
		private readonly ICounterpartySettings _counterpartySettings;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IUnitOfWorkFactory _uowFactory;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private KeyValuePair<string, string> _mode;
		private bool _modeAllowed;
		private Counterparty _counterparty;
		private Employee _sellManager;
		private Employee _orderAuthor;

		public ChainStoreDelayReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			ICounterpartySettings counterpartySettings,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ILifetimeScope autofacScope,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_counterpartySettings = counterpartySettings ?? throw new ArgumentNullException(nameof(counterpartySettings));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			Title = "Отсрочка сети";
			Identifier = "Payments.PaymentsDelayNetwork";

			UoW = _uowFactory.CreateWithoutRoot();

			Mode = Modes.First();
			_modeAllowed = true;

			CounterpartySelectorFactory = _counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory(autofacScope);
			SellManagerSelectorFactory = _employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();
			OrderAuthorSelectorFactory = _employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();

			_startDate = DateTime.Today;

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public KeyValuePair<string, string> Mode
		{
			get => _mode;
			set
			{
				if(!_mode.Equals(value))
				{
					_mode = value;
				}
			}
		}

		public Dictionary<string, string> Modes { get; set; } = new Dictionary<string, string>
		{
			{ "Networks", "Сетям" },
			{ "Tenders", "Тендерам" }
		};

		public virtual bool ModeAllowed
		{
			get => _modeAllowed;
			set => SetField(ref _modeAllowed, value);
		}

		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set
			{
				SetField(ref _counterparty, value);
				ModeAllowed = _counterparty == null;
			}
		}

		public IEntityAutocompleteSelectorFactory SellManagerSelectorFactory { get; }

		public virtual Employee SellManager
		{
			get => _sellManager;
			set => SetField(ref _sellManager, value);
		}

		public IEntityAutocompleteSelectorFactory OrderAuthorSelectorFactory { get; }

		public virtual Employee OrderAuthor
		{
			get => _orderAuthor;
			set => SetField(ref _orderAuthor, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "date", StartDate?.Date },
						{ "mode", Mode.Key },
						{ "counterparty_id", Counterparty?.Id ?? -1},
						{ "sell_manager_id", SellManager?.Id ?? -1},
						{ "order_author_id", OrderAuthor?.Id ?? -1},
						{ "counterparty_from_tender_id", _counterpartySettings.CounterpartyFromTenderId }
					};

				return parameters;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == null)
			{
				yield return new ValidationResult("Необходимо выбрать дату.", new[] { nameof(StartDate) });
			}
		}
	}
}
