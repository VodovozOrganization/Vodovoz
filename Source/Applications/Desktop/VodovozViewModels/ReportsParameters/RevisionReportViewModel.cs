using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Tdi;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.ViewModels.ReportsParameters
{
	public class RevisionReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private ITdiTab _tdiTab;
		private bool _sendRevision;
		private bool _sendBillsForNotPaidOrder;
		private bool _sendGeneralBill;

		public RevisionReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
			) : base(rdlViewerViewModel, reportInfoFactory)
		{
			UnitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			RdlViewerViewModel = rdlViewerViewModel ?? throw new ArgumentNullException(nameof(rdlViewerViewModel));

			SendByEmailCommand = new DelegateCommand(() => Console.WriteLine()
			);
			RunCommand = new DelegateCommand(() => this.LoadReport());

			Title = "Акт сверки";
			Identifier = "Client.Revision";
		}

		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

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

		private Counterparty _counterparty;
		public Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value, () => Counterparty);
		}

		public Email Email { get; set; }

		public IUnitOfWork UnitOfWork { get; private set; }
		public INavigationManager NavigationManager { get; }
		public ILifetimeScope LifetimeScope { get; private set; }
		public RdlViewerViewModel RdlViewerViewModel { get; }
		public DelegateCommand SendByEmailCommand { get; }
		public DelegateCommand RunCommand { get; }

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
	}
}
