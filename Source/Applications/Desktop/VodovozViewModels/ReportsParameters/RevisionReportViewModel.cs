using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Tdi;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.ViewModels.ReportsParameters
{
	public class RevisionReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private ITdiTab _tdiTab;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _sendRevision;
		private bool _sendBillsForNotPaidOrder;
		private bool _sendGeneralBill;
		private bool _reportIsLoaded;
		private bool _counterpartySelected;
		private bool _canRunReport;
		private Counterparty _counterparty;
		private IList<Email> _emails;
		private Email _selectedEmail;

		private readonly IInteractiveService _interactiveService;

		public RevisionReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IInteractiveService interactiveService
			) : base(rdlViewerViewModel, reportInfoFactory)
		{
			UnitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			RdlViewerViewModel = rdlViewerViewModel ?? throw new ArgumentNullException(nameof(rdlViewerViewModel));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			SendByEmailCommand = new DelegateCommand(() => SendByEmail());
			RunCommand = new DelegateCommand(() =>
			{
				this.LoadReport();
				ReportIsLoaded = true;
			});

			Title = "Акт сверки";
			Identifier = "Client.Revision";
		}

		#region Properties
		public DateTime? StartDate
		{
			get => _startDate;
			set
			{
				if(SetField(ref _startDate, value, () => StartDate))
				{
					CanRunReport = CounterpartySelected && value.HasValue && EndDate.HasValue;
					ReportIsLoaded = false;
				}
			}
		}
		public DateTime? EndDate
		{
			get => _endDate;
			set
			{
				if(SetField(ref _endDate, value, () => EndDate))
				{
					CanRunReport = CounterpartySelected && StartDate.HasValue && value.HasValue;
					ReportIsLoaded = false;
				}
			}
		}

		public ITdiTab TdiTab
		{
			get => _tdiTab;
			set => SetField(ref _tdiTab, value);
		}

		public bool SendRevision
		{
			get => _sendRevision;
			set => SetField(ref _sendRevision, value, () => SendRevision);
		}

		public bool SendBillsForNotPaidOrder
		{
			get => _sendBillsForNotPaidOrder;
			set => SetField(ref _sendBillsForNotPaidOrder, value, () => SendBillsForNotPaidOrder);
		}

		public bool SendGeneralBill
		{
			get => _sendGeneralBill;
			set => SetField(ref _sendGeneralBill, value, () => SendGeneralBill);
		}

		public Counterparty Counterparty
		{
			get => _counterparty;
			set
			{
				if(SetField(ref _counterparty, value, () => Counterparty))
				{
					CounterpartySelected = value != null;
					Emails = value?.Emails ?? new List<Email>();
					ReportIsLoaded = false;
				}
			}
		}
		public bool ReportIsLoaded
		{
			get => _reportIsLoaded;
			set => SetField(ref _reportIsLoaded, value, () => ReportIsLoaded);
		}
		public bool CounterpartySelected
		{
			get => _counterpartySelected;
			set
			{
				if(SetField(ref _counterpartySelected, value, () => CounterpartySelected))
				{
					CanRunReport = value && StartDate.HasValue && EndDate.HasValue;
				}
			}
		}
		public bool CanRunReport
		{
			get => _canRunReport;
			set => SetField(ref _canRunReport, value, () => CanRunReport);
		}

		public IList<Email> Emails
		{
			get => _emails;
			set => SetField(ref _emails, value, () => Emails);
		}
		public Email SelectedEmail
		{
			get => _selectedEmail;
			set => SetField(ref _selectedEmail, value, () => SelectedEmail);
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public ILifetimeScope LifetimeScope { get; private set; }
		public INavigationManager NavigationManager { get; }
		public RdlViewerViewModel RdlViewerViewModel { get; }
		public DelegateCommand SendByEmailCommand { get; }
		public DelegateCommand RunCommand { get; }
		#endregion
		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>
		{
			{ "StartDate", StartDate },
			{ "EndDate", EndDate },
			{ "CounterpartyID", Counterparty?.Id }
		};

		public void Dispose()
		{
			LifetimeScope?.Dispose();
			LifetimeScope = null;
			UnitOfWork?.Dispose();
			UnitOfWork = null;
		}
		private void SendByEmail()
		{
			if(Emails.Count == 0 || Emails == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "У контрагента не указан адрес электронной почты");
				return;
			}

			if(SelectedEmail == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбрана почта для отправки.");
				return;
			}


			if(!SendRevision && !SendBillsForNotPaidOrder && !SendGeneralBill)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбран ни один документ для отправки.");
				return;
			}

			//Console.WriteLine("Отправка письма на " + SelectedEmail.Address);
		}
	}
}
