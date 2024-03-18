using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Sale;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.ViewModels.ReportsParameters.Logistic
{
	public partial class DeliveriesLateReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private IUnitOfWork _uow;
		private bool _isOnlyFastSelect;
		private bool _allOrderSelectMode;
		private bool _isWithoutFastSelect;
		private IInteractiveService _interactiveService { get; }

		public DeliveriesLateReportViewModel(RdlViewerViewModel rdlViewerViewModel, IUnitOfWorkFactory uowFactory, IInteractiveService interactiveService)
			: base(rdlViewerViewModel)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_uow = (uowFactory ?? throw new ArgumentNullException(nameof(uowFactory))).CreateWithoutRoot(Title);

			Title = "Отчет по опозданиям";
			Identifier = "Logistic.DeliveriesLate";

			GenerateReportCommand = new DelegateCommand(GenerateReport);

			ConfigureFilters();
		}

		private void GenerateReport()
		{
			if(StartDate == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Необходимо выбрать дату");
				return;
			}

			LoadReport();
		}

		private IntervalSelectMode GetIntervalSelectedMode()
		{
			if(IsIntervalFromFirstAddress)
			{
				return IntervalSelectMode.FirstAddress;
			}
			if(IsIntervalFromTransferTime)
			{
				return IntervalSelectMode.Transfer;
			}

			return IntervalSelectMode.Create;
		}

		private OrderSelectMode GetOrderSelectMode()
		{
			if(IsOnlyFastSelect)
			{
				return OrderSelectMode.DeliveryInAnHour;
			}
			if(IsWithoutFastSelect)
			{
				return OrderSelectMode.WithoutDeliveryInAnHour;
			}

			return OrderSelectMode.All;
		}

		private void ConfigureFilters()
		{
			GeoGroups = _uow.GetAll<GeoGroup>().ToList();			
			AllOrderSelect = true;

			IncludeFilterViewModel = new IncludeExludeFiltersViewModel(_interactiveService)
			{
				WithExcludes = false
			};

			IncludeFilterViewModel.AddFilter<RouteListOwnType>(config =>
			{
				config.RefreshFilteredElements();
			});
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
				{
					{ "start_date", StartDate },
					{ "end_date", EndDate.AddHours(3) },
					{ "is_driver_sort", IsDriverSort},
					{ "geographic_group_id", GeoGroup?.Id ?? 0 },
					{ "geographic_group_name", GeoGroup?.Name ?? "Все" },
					{ "exclude_truck_drivers_office_employees", false },
					{ "order_select_mode", GetOrderSelectMode().ToString() },
					{ "interval_select_mode", GetIntervalSelectedMode().ToString() },
				};

				foreach(var item in IncludeFilterViewModel.GetReportParametersSet())
				{
					parameters.Add(item.Key, item.Value);
				}

				return parameters;
			}
		}

		private enum OrderSelectMode
		{
			All,
			DeliveryInAnHour,
			WithoutDeliveryInAnHour
		}

		private enum IntervalSelectMode
		{
			Create,
			Transfer,
			FirstAddress
		}

		[PropertyChangedAlso(nameof(IsIntervalVisible))]
		public bool IsOnlyFastSelect
		{
			get => _isOnlyFastSelect;
			set => SetField(ref _isOnlyFastSelect, value);
		}

		[PropertyChangedAlso(nameof(IsIntervalVisible))]
		public bool IsWithoutFastSelect
		{
			get => _isWithoutFastSelect;
			set => SetField(ref _isWithoutFastSelect, value);
		}

		[PropertyChangedAlso(nameof(IsIntervalVisible))]
		public bool AllOrderSelect
		{
			get => _allOrderSelectMode;
			set => SetField(ref _allOrderSelectMode, value);
		}

		public bool IsIntervalVisible => IsOnlyFastSelect || AllOrderSelect;

		public bool IsIntervalFromFirstAddress { get; set; }
		public bool IsIntervalFromTransferTime { get; set; }

		public GeoGroup GeoGroup { get; set; }
		public IList<GeoGroup> GeoGroups { get; set; }

		public DateTime? StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public bool IsDriverSort { get; set; }

		public DelegateCommand GenerateReportCommand { get; }

		public IncludeExludeFiltersViewModel IncludeFilterViewModel { get; private set; }

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
