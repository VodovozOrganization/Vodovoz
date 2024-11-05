using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Tdi;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.ReportsParameters
{
	public class RevisionReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private ITdiTab _tdiTab;

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

		public Counterparty Counterparty { get; set; }

		public IUnitOfWork UnitOfWork { get; private set; }
		public INavigationManager NavigationManager { get; }
		public ILifetimeScope LifetimeScope { get; private set; }
		public RdlViewerViewModel RdlViewerViewModel { get; }

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
