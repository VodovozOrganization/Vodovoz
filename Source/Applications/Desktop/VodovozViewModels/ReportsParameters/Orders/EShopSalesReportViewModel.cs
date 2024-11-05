using Gamma.Utilities;
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
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Orders
{
	public class EShopSalesReportViewModel : ValidatableUoWReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private List<OrderStatus> _orderStatuses = new List<OrderStatus>();
		private IEnumerable<OnlineStore> _onlineStores;
		private OnlineStore _onlineStore;

		public EShopSalesReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			Title = "Отчет по продажам ИМ";
			Identifier = "Orders.EShopSalesReport";

			UoW = _uowFactory.CreateWithoutRoot();

			var onlineStores = UoW.Session.QueryOver<OnlineStore>().List();
			onlineStores.Insert(0, new OnlineStore() { Id = -1, Name = "Все" });
			OnlineStores = onlineStores;
			OnlineStore = OnlineStores.FirstOrDefault();

			_startDate = DateTime.Today;
			_endDate = DateTime.Today;

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

		public Type OrderStatusType => typeof(OrderStatus);

		public virtual List<OrderStatus> OrderStatuses
		{
			get => _orderStatuses;
			set => SetField(ref _orderStatuses, value);
		}

		public virtual IEnumerable<OnlineStore> OnlineStores
		{
			get => _onlineStores;
			set => SetField(ref _onlineStores, value);
		}

		public virtual OnlineStore OnlineStore
		{
			get => _onlineStore;
			set => SetField(ref _onlineStore, value);
		}


		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "end_date", EndDate?.Date.AddDays(1).AddMilliseconds(-1) },
						{ "e_shop_id", OnlineStore?.Id ?? -1 },
						{ "creation_timestamp", DateTime.Now },
						{ "order_statuses", OrderStatuses },
						{ "order_statuses_rus", string.Join(", ", OrderStatuses.Select(x => x.GetEnumTitle())) }
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

			if(OrderStatuses.Count() == 0)
			{
				yield return new ValidationResult("Необходимо выбрать статусы заказа.", new[] { nameof(OrderStatuses) });
			}
		}
	}
}
