using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ReportsParameters.Orders
{
	public class FirstSecondClientReportViewModel : ReportParametersUowViewModelBase
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _author;
		private DiscountReason _discountReason;
		private IEnumerable<DiscountReason> _discountReasons;
		private bool _hasPromoset;
		private bool _showOnlyClientsWithOneOrder;

		public FirstSecondClientReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IDiscountReasonRepository discountReasonRepository
		) : base(rdlViewerViewModel, uowFactory, reportInfoFactory)
		{
			var employeesFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));

			Title = "Отчёт по первичным/вторичным заказам";
			Identifier = "Bottles.FirstSecondClients";

			_startDate = DateTime.Now.AddDays(-7);
			_endDate = DateTime.Now.AddDays(1);

			AuthorSelectorFactory = employeesFactory.CreateEmployeeAutocompleteSelectorFactory();

			DiscountReasons = _discountReasonRepository.GetDiscountReasons(UoW);

			GenerateReportCommand = new DelegateCommand(LoadReport);
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

		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		public IEntityAutocompleteSelectorFactory AuthorSelectorFactory { get; }

		public virtual DiscountReason DiscountReason
		{
			get => _discountReason;
			set => SetField(ref _discountReason, value);
		}

		public virtual IEnumerable<DiscountReason> DiscountReasons
		{
			get => _discountReasons;
			set => SetField(ref _discountReasons, value);
		}

		public virtual bool HasPromoset
		{
			get => _hasPromoset;
			set => SetField(ref _hasPromoset, value);
		}

		public virtual bool ShowOnlyClientsWithOneOrder
		{
			get => _showOnlyClientsWithOneOrder;
			set => SetField(ref _showOnlyClientsWithOneOrder, value);
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
						{ "show_only_client_with_one_order", ShowOnlyClientsWithOneOrder },
						{ "author_employer_id", Author?.Id ?? 0 },
						{ "has_promo_set", HasPromoset }
					};

				return parameters;
			}
		}
	}
}
