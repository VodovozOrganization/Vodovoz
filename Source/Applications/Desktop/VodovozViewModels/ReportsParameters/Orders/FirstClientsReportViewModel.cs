using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Presentation.Reports;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.ReportsParameters.Orders
{
	public class FirstClientsReportViewModel : ValidatableUoWReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private IEnumerable<DiscountReason> _discountReasons;
		private DiscountReason _discountReason;
		private OrderStatus? _orderStatus;
		private PaymentType? _paymentType;
		private District _district;
		private bool _withPromosets;

		public FirstClientsReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IDiscountReasonRepository discountReasonRepository,
			IDistrictJournalFactory districtJournalFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_districtJournalFactory = districtJournalFactory ?? throw new ArgumentNullException(nameof(districtJournalFactory));

			Title = "Отчет по первичным клиентам";
			Identifier = "Orders.FirstClients";

			UoW = _uowFactory.CreateWithoutRoot();

			DiscountReasons = _discountReasonRepository.GetActiveDiscountReasons(UoW);
			DiscountReason = DiscountReasons?.OrderByDescending(r => r.Id).FirstOrDefault();

			var districtFilter = new DistrictJournalFilterViewModel { Status = DistrictsSetStatus.Active };
			DistrictSelectorFactory = _districtJournalFactory.CreateDistrictAutocompleteSelectorFactory(districtFilter);

			_startDate = DateTime.Today;
			_endDate = DateTime.Today;

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly IDistrictJournalFactory _districtJournalFactory;

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

		public virtual IEnumerable<DiscountReason> DiscountReasons
		{
			get => _discountReasons;
			set => SetField(ref _discountReasons, value);
		}

		public virtual DiscountReason DiscountReason
		{
			get => _discountReason;
			set => SetField(ref _discountReason, value);
		}

		public Type OrderStatusType => typeof(OrderStatus);

		public virtual OrderStatus? OrderStatus
		{
			get => _orderStatus;
			set => SetField(ref _orderStatus, value);
		}

		public Type PaymentTypeType => typeof(PaymentType);

		public virtual PaymentType? PaymentType
		{
			get => _paymentType;
			set => SetField(ref _paymentType, value);
		}

		public IEntityAutocompleteSelectorFactory DistrictSelectorFactory { get; }

		public virtual District District
		{
			get => _district;
			set => SetField(ref _district, value);
		}

		public virtual bool WithPromosets
		{
			get => _withPromosets;
			set => SetField(ref _withPromosets, value);
		}


		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "end_date", EndDate },
						{ "discount_id", DiscountReason?.Id ?? 0 },
						{ "order_status", OrderStatus == null ? "All" : OrderStatus.ToString() },
						{ "payment_type", PaymentType == null ? "All" : PaymentType.ToString()  },
						{ "district_id", District?.Id },
						{ "has_promotional_sets", WithPromosets }
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
