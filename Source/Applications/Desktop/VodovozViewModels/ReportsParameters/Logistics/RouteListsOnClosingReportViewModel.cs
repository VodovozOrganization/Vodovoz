using DateTimeHelpers;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.Presentation.Reports;
namespace Vodovoz.ViewModels.ReportsParameters.Logistics
{
	public class RouteListsOnClosingReportViewModel : ValidatableUoWReportViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _showTodayRouteLists;
		private bool? _includeVisitingMasters;
		private GeoGroup _geoGroup;
		private IEnumerable<GeoGroup> _geoGroups = Enumerable.Empty<GeoGroup>();
		private IEnumerable<Enum> _carTypeOfUseList = Enumerable.Empty<Enum>();
		private IEnumerable<Enum> _carOwnTypeList = Enumerable.Empty<Enum>();

		public RouteListsOnClosingReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			Title = "Отчет по незакрытым МЛ";
			Identifier = "Logistic.RouteListOnClosing";

			UoW = _uowFactory.CreateWithoutRoot();

			GeoGroups = UoW.GetAll<GeoGroup>().ToList();
			_showTodayRouteLists = true;


			_endDate = DateTime.Now.FirstDayOfMonth();

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;

		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public virtual bool ShowTodayRouteLists
		{
			get => _showTodayRouteLists;
			set => SetField(ref _showTodayRouteLists, value);
		}

		public virtual bool? IncludeVisitingMasters
		{
			get => _includeVisitingMasters;
			set => SetField(ref _includeVisitingMasters, value);
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

		public Type CarTypeOfUseType => typeof(CarTypeOfUse);

		public Enum[] HiddenCarTypeOfUse => new Enum[] { CarTypeOfUse.Loader };

		public virtual IEnumerable<Enum> CarTypeOfUseList
		{
			get => _carTypeOfUseList;
			set => SetField(ref _carTypeOfUseList, value);
		}

		public Type CarOwnTypeType => typeof(CarOwnType);

		public virtual IEnumerable<Enum> CarOwnTypeList
		{
			get => _carOwnTypeList;
			set => SetField(ref _carOwnTypeList, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var carTypesOfUse = CarTypeOfUseList.ToArray();
				var carOwnTypes = CarOwnTypeList.ToArray();

				var parameters = new Dictionary<string, object>
					{
						{ "geographic_group_id", GeoGroup?.Id ?? 0 },
						{ "car_types_of_use", carTypesOfUse.Any() ? carTypesOfUse : new[] { (object)0 } },
						{ "car_own_types", carOwnTypes.Any() ? carOwnTypes : new[] { (object)0 } },
						{ "show_today_route_lists", ShowTodayRouteLists },
						{ "include_visiting_masters", IncludeVisitingMasters },
						{ "end_date", EndDate.Value }
					};

				return parameters;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(EndDate == null)
			{
				yield return new ValidationResult("Необходимо выбрать дату.", new[] { nameof(EndDate) });
			}
		}
	}
}
