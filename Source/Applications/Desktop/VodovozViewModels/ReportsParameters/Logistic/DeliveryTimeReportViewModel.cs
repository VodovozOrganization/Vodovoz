using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Sale;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.ReportsParameters.Logistic
{
	public partial class DeliveryTimeReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private readonly IEnumerable<RouteListOwnType> _selectedRouteListOwnTypes = new[]
		{
			RouteListOwnType.Delivery,
			RouteListOwnType.ChainStore
		};

		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _uow;

		public DeliveryTimeReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			IReportInfoFactory reportInfoFactory
		) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Отчет 'Время доставки";

			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			_uow = _uowFactory.CreateWithoutRoot(Title);

			GeoGroupList = _uow.GetAll<GeoGroup>()
				.Select(x => new SelectableParameter { GeographicGroup = x, IsSelected = true })
				.ToArray();

			RouteListTypeOfUseList = Enum.GetValues(typeof(RouteListOwnType)).Cast<RouteListOwnType>()
				.Select(x => new SelectableParameter { RouteListOwnType = x, IsSelected = _selectedRouteListOwnTypes.Contains(x) })
				.ToList();

			OrdersEnRouteCountList = Enumerable.Range(0, 8);

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		private void GenerateReport()
		{
			if(Time == TimeSpan.Zero)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Сначала необходимо указать интервал");
				return;
			}

			if(!SelectedGeoGroups.Any())
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбрана ни одна часть города");
				return;
			}
			if(!SelectedRouteListTypesOfUse.Any())
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбрана ни одна принадлежность МЛ");
				return;
			}

			LoadReport();
		}

		private string GetSelectedFilters()
		{
			var selectedGeoGroups = String.Join(", ", SelectedGeoGroups.Select(x => x.Name));
			var selectedRouteListTypeOfUses = String.Join(", ", SelectedRouteListTypesOfUse.Select(x => x.GetEnumDisplayName()));

			return "Выбранные фильтры:\n" +
				$"Время доставки до: {Time}\n" +
				$"Часть города: {selectedGeoGroups}\n" +
				$"Принадлежность МЛ: {selectedRouteListTypeOfUses}\n" +
				$"Кол-во заказов в пути: {SelectedOrdersEnRouteCount}\n";
		}

		private IEnumerable<GeoGroup> SelectedGeoGroups => GeoGroupList
			.Where(x => x.IsSelected)
			.Select(x => x.GeographicGroup);

		private IEnumerable<RouteListOwnType> SelectedRouteListTypesOfUse => RouteListTypeOfUseList
			.Where(x => x.IsSelected)
			.Select(x => x.RouteListOwnType);

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				switch(SelectedReportType)
				{
					case ReportType.Numbering:
						Identifier = "Logistic.DeliveryTime";
						break;
					case ReportType.Grouped:
						Identifier = "Logistic.DeliveryTimeGrouped";
						break;
				}

				var parameters = new Dictionary<string, object>
				{
					{ "beforeTime", Time.ToString("hh\\:mm") },
					{ "geographic_groups",  SelectedGeoGroups.Select(x => x.Id) },
					{ "rl_type_of_use", SelectedRouteListTypesOfUse },
					{ "filters_text", GetSelectedFilters() },
					{ "creation_date", DateTime.Now },
					{ "orders_en_route_count", SelectedOrdersEnRouteCount }
				};

				return parameters;
			}
		}

		public IList<SelectableParameter> RouteListTypeOfUseList { get; }
		public IEnumerable<int> OrdersEnRouteCountList { get; }
		public SelectableParameter[] GeoGroupList { get; }
		public int SelectedOrdersEnRouteCount { get; set; }
		public ReportType SelectedReportType { get; set; }
		public TimeSpan Time { get; set; }
		public DelegateCommand GenerateReportCommand { get; }

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
