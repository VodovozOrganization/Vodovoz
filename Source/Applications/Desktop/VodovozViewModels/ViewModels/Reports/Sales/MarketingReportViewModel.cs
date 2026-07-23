using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Orders;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel : DialogTabViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;

		private IncludeExludeFiltersViewModel _filtersViewModel;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private MarketingReport _report;
		private bool _splitByAbc;
		private bool _splitByOrderAuthor;
		private bool _isSaving;
		private bool _canSave;
		private bool _isGenerating;
		private bool _canCancelGenerate;
		private IEnumerable<string> _lastGenerationErrors;

		public MarketingReportViewModel(
										IInteractiveService interactiveService,
										IUnitOfWorkFactory unitOfWorkFactory,
										INavigationManager navigation,
										ICurrentPermissionService currentPermissionService) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}
			if(!currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ReportPermissions.Sales.CanAccessSalesReports))
			{
				throw new AbortCreatingPageException("У вас нет разрешения на доступ в этот отчет", "Доступ запрещен");
			}
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();

			TabName = "Маркетинговый отчет";
			StartDate = DateTime.Now.Date.AddMonths(-3);  //Если это бизнес-правило, то его бы вынести в отдельную сущность
			EndDate = DateTime.Now.Date;

			SplitByAbc = true;
			_splitByOrderAuthor = true;

			_lastGenerationErrors = Enumerable.Empty<string>();

			ConfigureFilter();
		}

		public CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }

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
		public virtual bool SplitByAbc
		{
			get => _splitByAbc;
			set => SetField(ref _splitByAbc, value);
		}
		public virtual bool SplitByOrderAuthor
		{
			get => _splitByOrderAuthor;
			set => SetField(ref _splitByOrderAuthor, value);
		}

		public IncludeExludeFiltersViewModel FilterViewModel => _filtersViewModel;

		public MarketingReport Report
		{
			get => _report;
			set
			{
				SetField(ref _report, value);
				CanSave = _report != null;
			}
		}
		public bool CanSave
		{
			get => _canSave;
			set => SetField(ref _canSave, value);
		}
		public bool IsSaving
		{
			get => _isSaving;
			set
			{
				SetField(ref _isSaving, value);
				CanSave = !IsSaving;
			}
		}
		public bool CanGenerate => !_isGenerating;

		public bool CanCancelGenerate
		{
			get => _canCancelGenerate;
			set => SetField(ref _canCancelGenerate, value);
		}
		public bool IsGenerating
		{
			get => _isGenerating;
			set
			{
				SetField(ref _isGenerating, value);
				OnPropertyChanged(nameof(CanGenerate));
				CanCancelGenerate = value;
			}
		}
		public IEnumerable<string> LastGenerationErrors
		{
			get => _lastGenerationErrors;
			set => SetField(ref _lastGenerationErrors, value);
		}

		private void ConfigureFilter()
		{
			_filtersViewModel = new IncludeExludeFiltersViewModel(_interactiveService);
			var statusesToSelect = new[]
			{
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};
			_filtersViewModel.AddFilter<OrderStatus>(config =>
			{
				config.RefreshFilteredElements();
				foreach(var element in config.FilteredElements)
				{
					if(element is IncludeExcludeElement<OrderStatus, OrderStatus> enumElement && statusesToSelect.Contains(enumElement.Id))
					{
						enumElement.Include = true;
					}
				}
			});
			_filtersViewModel.AddFilter<CounterpartyCompositeClassification>(config =>
			{
				config.RefreshFilteredElements();
			});
			_filtersViewModel.AddFilter<OrderAuthorSubtype>(config =>
			{
				config.RefreshFilteredElements();
			});
		}
		public void ShowWarning(string message)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Warning, message);
		}

		public async Task<MarketingReport> ActionGenerateReport(CancellationToken cancellationToken)
		{
			try
			{
				return await Generate(cancellationToken);
			}
			finally
			{
				_unitOfWork.Session.Clear();
			}
		}

		public async Task<MarketingReport> Generate(CancellationToken cancellationToken)
		{
			var includedAbc = _filtersViewModel.GetFilter<IncludeExcludeEnumFilter<CounterpartyCompositeClassification>>().GetIncluded().ToList();
			var includedAuthorSubtypes = _filtersViewModel.GetFilter<IncludeExcludeEnumFilter<OrderAuthorSubtype>>().GetIncluded().ToList();
			var includedStatuses = _filtersViewModel.GetFilter<IncludeExcludeEnumFilter<OrderStatus>>().GetIncluded().ToArray();

			var startDate = StartDate.Value;
			var endDate = EndDate.Value;
			var splitByAbc = SplitByAbc;
			var splitByOrderAuthor = SplitByOrderAuthor;

			return await Task.Run(() =>
			{
				return MarketingReport.Create(
					startDate,
					endDate,
					splitByAbc,
					splitByOrderAuthor,
					includedAbc,
					includedAuthorSubtypes,
					GetData,
					includedStatuses,
					GetTotalCounterpartiesCount);
			}, cancellationToken);
		}

		private int GetTotalCounterpartiesCount()
		{
			return _unitOfWork.Session.QueryOver<Vodovoz.Domain.Client.Counterparty>()
				.Where(c => !c.IsArchive)
				.RowCount();
		}

		public void ExportReport(string path)
		{
			Report.Export(path);
		}
		public override void Dispose()
		{
			base.Dispose();
		}
	}

}

