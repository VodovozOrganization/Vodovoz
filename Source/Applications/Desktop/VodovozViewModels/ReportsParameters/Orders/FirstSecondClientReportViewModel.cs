using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Extensions;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ReportsParameters.Orders
{
	public class FirstSecondClientReportViewModel : ReportParametersUowViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly IDiscountReasonRepository _discountReasonRepository;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _author;
		private DiscountReason _discountReason;
		private IEnumerable<DiscountReason> _discountReasons;
		private bool _hasPromoset;
		private bool _showOnlyClientsWithOneOrder;
		private IList<OrderStatus> _firstOrderStatuses;
		private IList<OrderStatus> _secondOrderStatuses;

		public FirstSecondClientReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			IDiscountReasonRepository discountReasonRepository
		) : base(rdlViewerViewModel, uowFactory, reportInfoFactory)
		{
			var employeesFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));

			Title = "Отчёт по первичным и вторичным заказам";
			Identifier = "Bottles.FirstSecondClients";

			_startDate = DateTime.Today.AddDays(-7);
			_endDate = DateTime.Today;

			AuthorSelectorFactory = employeesFactory.CreateEmployeeAutocompleteSelectorFactory();

			DiscountReasons = _discountReasonRepository.GetDiscountReasons(UoW);

			GenerateReportCommand = new DelegateCommand(GenerateReport);

			_firstOrderStatuses = SelectedByDefaultOrderStatuses;
			_secondOrderStatuses = SelectedByDefaultOrderStatuses;
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

		public virtual IList<OrderStatus> FirstOrderStatuses
		{
			get => _firstOrderStatuses;
			set => SetField(ref _firstOrderStatuses, value);
		}

		public virtual IList<OrderStatus> SecondOrderStatuses
		{
			get => _secondOrderStatuses;
			set => SetField(ref _secondOrderStatuses, value);
		}

		private IList<OrderStatus> SelectedByDefaultOrderStatuses =>
			new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };

		private void GenerateReport()
		{
			if(StartDate is null || EndDate is null)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Необходимо указать период");
			}

			LoadReport();
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
				{
					{ "start_date", StartDate.Value.Date },
					{ "end_date", EndDate.Value.Date },
					{ "discount_id", DiscountReason?.Id ?? 0 },
					{ "has_promo_set", HasPromoset },
					{ "show_only_client_with_one_order", ShowOnlyClientsWithOneOrder },
					{ "author_employer_id", Author?.Id ?? 0 },
					{ "first_order_statuses", FirstOrderStatuses },
					{ "second_order_statuses", SecondOrderStatuses },
					{ "selected_filters_info", SelectedFiltersInfo }
					};

				return parameters;
			}
		}

		private string SelectedFiltersInfo =>
			$"Период даты доставки заказа: {StartDate.Value.Date : dd.MM.yyyy} - {EndDate.Value.Date: dd.MM.yyyy}\n" +
			$"Основание скидки первого заказа: {DiscountReasonInfo}\n" +
			$"Только с промонаборами: {HasPromosetInfo}\n" +
			$"Только клиенты с одним заказом: {ShowOnlyClientsWithOneOrderInfo}\n" +
			$"Автор первого заказа: {AuthorInfo}\n" +
			$"Выбранные статусы для первого заказа: {string.Join(", ", FirstOrderStatuses.Select(x => x.GetEnumDisplayName()))}\n" +
			$"Выбранные статусы для второго заказа: {string.Join(", ", SecondOrderStatuses.Select(x => x.GetEnumDisplayName()))}";

		private string DiscountReasonInfo =>
			DiscountReason is null
			? "Не выбрано"
			: DiscountReason.Name;

		private string HasPromosetInfo =>
			HasPromoset
			? "Да"
			: "Нет";

		private string ShowOnlyClientsWithOneOrderInfo =>
			ShowOnlyClientsWithOneOrder
			? "Да"
			: "Нет";

		private string AuthorInfo =>
			Author is null ? "Не выбрано" : $"{Author.LastName} {Author.Name} {Author.Patronymic}";
	}
}
