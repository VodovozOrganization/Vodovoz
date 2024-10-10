using QS.Commands;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Logistics
{
	public class ScheduleOnLinePerShiftReportViewModel : ValidatableUoWReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private GenericObservableList<GeoGroup> _geoGroups;
		private IEnumerable<Enum> _carTypeOfUseList = Enumerable.Empty<Enum>();
		private IEnumerable<Enum> _carOwnTypeList = Enumerable.Empty<Enum>();

		public ScheduleOnLinePerShiftReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			_geoGroups = new GenericObservableList<GeoGroup>();

			Title = "График выхода на линию за смену";
			Identifier = "Logistic.ScheduleOnLinePerShiftReport";

			UoW = _uowFactory.CreateWithoutRoot();

			foreach(var gg in UoW.Session.QueryOver<GeoGroup>().List())
			{
				_geoGroups.Add(gg);
			}

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;
		private readonly IUnitOfWorkFactory _uowFactory;

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

		public virtual GenericObservableList<GeoGroup> GeoGroups
		{
			get => _geoGroups;
			set => SetField(ref _geoGroups, value);
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
				var geoGroupsIds = GeoGroups.Select(g => g.Id).ToArray();
				var carTypesOfUse = CarTypeOfUseList.ToArray();
				var carOwnTypes = CarOwnTypeList.ToArray();

				var parameters = new Dictionary<string, object>
				{
					{ "start_date", StartDate },
					{ "end_date", EndDate },
					{ "geo_group_ids", geoGroupsIds.Any() ? geoGroupsIds : new[] { 0 } },
					{ "car_types_of_use", carTypesOfUse.Any() ? carTypesOfUse : new[] { (object)0 } },
					{ "car_own_types", carOwnTypes.Any() ? carOwnTypes : new[] { (object)0 } }
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
	}
}
