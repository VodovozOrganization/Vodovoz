using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Orders
{
	public class OrdersByDistrictReportViewModel : ValidatableUoWReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _isAllDistricts;
		private District _district;

		public OrdersByDistrictReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			Title = "Отчет по районам";

			UoW = uowFactory.CreateWithoutRoot();

			DistrictsSelectorFactory = new EntityAutocompleteSelectorFactory<DistrictJournalViewModel>(typeof(District), () =>
			{
				var filter = new DistrictJournalFilterViewModel
				{
					Status = DistrictsSetStatus.Active
				};
				return new DistrictJournalViewModel(filter, uowFactory, commonServices)
				{
					EnableDeleteButton = false,
					EnableAddButton = false,
					EnableEditButton = false
				};
			});

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;

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

		public virtual bool IsAllDistricts
		{
			get => _isAllDistricts;
			set => SetField(ref _isAllDistricts, value);
		}

		public virtual District District
		{
			get => _district;
			set => SetField(ref _district, value);
		}

		public IEntityAutocompleteSelectorFactory DistrictsSelectorFactory { get; }

		public override string Identifier
		{
			get
			{
				if(IsAllDistricts)
				{
					return "Orders.OrdersByAllDistrict";
				}
				else
				{
					return "Orders.OrdersByDistrict";
				}
			}
		}

		public override ReportInfo ReportInfo
		{
			get
			{
				var reportInfo = base.ReportInfo;
				reportInfo.UseUserVariables = true;
				return reportInfo;
			}
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate?.Date },
						{ "end_date", EndDate?.Date.AddHours(23).AddMinutes(59).AddSeconds(59) }
					};

				if(!IsAllDistricts && District != null)
				{
					parameters.Add("id_district", District.Id);
				}

				return parameters;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(District == null && !IsAllDistricts)
			{
				yield return new ValidationResult("Необходимо выбрать район.", new[] { nameof(District) });
			}

			if(StartDate == null || EndDate == null)
			{
				yield return new ValidationResult("Необходимо выбрать период.", new[] { nameof(StartDate), nameof(EndDate) });
			}
		}
	}
}
