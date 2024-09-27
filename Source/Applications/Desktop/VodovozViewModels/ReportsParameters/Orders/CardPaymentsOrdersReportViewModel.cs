using QS.Commands;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Orders
{
	public class CardPaymentsOrdersReportViewModel : ValidatableUoWReportViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private IEnumerable<GeoGroup> _geoGroups;
		private GeoGroup _geoGroup;
		private bool _allGeoGroupsSelected;
		private IEnumerable<PaymentFrom> _paymentsFrom;
		private PaymentFrom _paymentFrom;
		private bool _allPaymentsFromSelected;
		private bool _showArchive;

		public CardPaymentsOrdersReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			Title = "Отчет по оплатам по картам";
			Identifier = "Orders.CardPaymentsOrdersReport";

			_startDate = DateTime.Today;
			_endDate = DateTime.Today;
			_allGeoGroupsSelected = true;
			_allPaymentsFromSelected = true;

			UoW = _uowFactory.CreateWithoutRoot();

			_geoGroups = UoW.GetAll<GeoGroup>().ToList();
			_paymentsFrom = UoW.GetAll<PaymentFrom>().ToList();

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

		public virtual bool AllGeoGroupsSelected
		{
			get => _allGeoGroupsSelected;
			set => SetField(ref _allGeoGroupsSelected, value);
		}

		public virtual IEnumerable<PaymentFrom> PaymentsFrom
		{
			get => _paymentsFrom;
			set => SetField(ref _paymentsFrom, value);
		}

		public virtual PaymentFrom PaymentFrom
		{
			get => _paymentFrom;
			set => SetField(ref _paymentFrom, value);
		}

		public virtual bool AllPaymentsFromSelected
		{
			get => _allPaymentsFromSelected;
			set
			{
				SetField(ref _allPaymentsFromSelected, value);
				if(!_allPaymentsFromSelected)
				{
					ShowArchive = false;
				}
			}
		}

		public virtual bool ShowArchive
		{
			get => _showArchive;
			set => SetField(ref _showArchive, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "end_date", EndDate },
						{ "payment_from_id", AllPaymentsFromSelected ? "" : PaymentFrom.Id.ToString() },
						{ "geo_group_id", AllGeoGroupsSelected ? "" : GeoGroup.Id.ToString() },
						{ "ShowArchived", ShowArchive }
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
