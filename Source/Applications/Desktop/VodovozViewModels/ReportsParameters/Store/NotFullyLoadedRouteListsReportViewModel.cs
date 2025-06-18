using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Presentation.Reports;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.ViewModels.ReportsParameters.Store
{
	public class NotFullyLoadedRouteListsReportViewModel : ValidatableUoWReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Warehouse _warehouse;

		public NotFullyLoadedRouteListsReportViewModel(
			ILifetimeScope lifetimeScope,
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_rdlViewerViewModel = rdlViewerViewModel ?? throw new ArgumentNullException(nameof(rdlViewerViewModel));

			Title = "Отчет по не полностью погруженным МЛ";
			Identifier = "Store.NotFullyLoadedRouteLists";

			GenerateReportCommand = new DelegateCommand(GenerateReport);

			_navigationManager = _lifetimeScope.Resolve<INavigationManager>();
			var uowFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();

			UoW = uowFactory.CreateWithoutRoot();
			
			WarehouseEntryViewModel = new CommonEEVMBuilderFactory<NotFullyLoadedRouteListsReportViewModel>(_rdlViewerViewModel, this, UoW, _navigationManager, _lifetimeScope)
				.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			_startDate = DateTime.Today;
			_endDate = DateTime.Today;
		}

		public DelegateCommand GenerateReportCommand;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly RdlViewerViewModel _rdlViewerViewModel;
		private readonly INavigationManager _navigationManager;

		public IEntityEntryViewModel WarehouseEntryViewModel { get; }

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
		}


		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "end_date", EndDate },
						{ "warehouse_id", Warehouse?.Id ?? 0 }
					};

				return parameters;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == null || EndDate == null)
			{
				yield return new ValidationResult("Необходимо выбрать период.", new[] { nameof(StartDate), nameof(EndDate) });
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			_lifetimeScope?.Dispose();
		}
	}
}
