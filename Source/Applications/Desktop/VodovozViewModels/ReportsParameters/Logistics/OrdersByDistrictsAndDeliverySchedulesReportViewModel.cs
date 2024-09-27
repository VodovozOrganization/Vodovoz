using QS.Commands;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.ReportsParameters.Logistics
{
	public class OrdersByDistrictsAndDeliverySchedulesReportViewModel : ReportParametersUowViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		private DateTime _startDate;
		private DateTime _endDate;
		private IEnumerable<GeoGroup> _geoGroups;
		private GeoGroup _geoGroup;
		private IEnumerable<TariffZone> _tariffZones;
		private TariffZone _tariffZone;

		public OrdersByDistrictsAndDeliverySchedulesReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory
		) : base(rdlViewerViewModel, uowFactory, reportInfoFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			Title = "Заказы по районам и интервалам";
			Identifier = "Logistic.OrdersByDistrictsAndDeliverySchedules";

			_geoGroups = UoW.GetAll<GeoGroup>().ToList();
			_tariffZones = UoW.GetAll<TariffZone>().OrderBy(x => x.Name).ToList();

			_startDate = DateTime.Today;
			_endDate = DateTime.Today;

			GenerateReportCommand = new DelegateCommand(LoadReport);
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

		public virtual IEnumerable<GeoGroup> GeoGroups
		{
			get => _geoGroups;
			set => SetField(ref _geoGroups, value);
		}

		public virtual GeoGroup GeoGroup
		{
			get => _geoGroup;
			set => SetField(ref _geoGroup, value);
		}

		public virtual IEnumerable<TariffZone> TariffZones
		{
			get => _tariffZones;
			set => SetField(ref _tariffZones, value);
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
						{ "start_date", StartDate },
						{ "end_date", EndDate },
						{ "geographic_group_id", GeoGroup?.Id },
						{ "geographic_group_name", GeoGroup?.Name },
						{ "tariff_zone_id", TariffZone?.Id },
						{ "tariff_zone_name",TariffZone?.Name }
					};

				return parameters;
			}
		}
	}
}
