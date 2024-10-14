using QS.Commands;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Bottles
{
	public class ClientsByDeliveryPointCategoryAndActivityKindsReportViewModel : ValidatableUoWReportViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private IEnumerable<CounterpartyActivityKind> _activityKinds;
		private CounterpartyActivityKind _activityKind;
		private IEnumerable<DeliveryPointCategory> _deliveryPointCategories;
		private DeliveryPointCategory _deliveryPointCategory;
		private PaymentType? _paymentType;
		private IEnumerable<SubstringToSearch> _substringsToSearch;
		private string _substringToSearch;

		public ClientsByDeliveryPointCategoryAndActivityKindsReportViewModel(
			IUnitOfWorkFactory uowFactory,
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			Title = "Клиенты по типам объектов и видам деятельности";
			Identifier = "Bottles.ClientsByDeliveryPointCategoryAndActivityKindsReport";

			UoW = _uowFactory.CreateWithoutRoot();

			GenerateReportCommand = new DelegateCommand(GenerateReport);

			_startDate = DateTime.Today;
			_endDate = DateTime.Today;
			_deliveryPointCategories = UoW.Session.QueryOver<DeliveryPointCategory>().List();
			_activityKinds = UoW.Session.QueryOver<CounterpartyActivityKind>().List();
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

		public virtual IEnumerable<CounterpartyActivityKind> ActivityKinds
		{
			get => _activityKinds;
			private set => SetField(ref _activityKinds, value);
		}

		public virtual CounterpartyActivityKind ActivityKind
		{
			get => _activityKind;
			set
			{
				if(SetField(ref _activityKind, value))
				{
					UpdateSubstringsToSearch();
					OnPropertyChanged(nameof(SubstringsVisible));
				}
			}
		}

		public virtual IEnumerable<DeliveryPointCategory> DeliveryPointCategories
		{
			get => _deliveryPointCategories;
			private set => SetField(ref _deliveryPointCategories, value);
		}

		public virtual DeliveryPointCategory DeliveryPointCategory
		{
			get => _deliveryPointCategory;
			set => SetField(ref _deliveryPointCategory, value);
		}

		public Type PaymentTypeType => typeof(PaymentType);

		public virtual PaymentType? PaymentType
		{
			get => _paymentType;
			set => SetField(ref _paymentType, value);
		}

		public bool SubstringsVisible => _activityKind == null;

		public virtual IEnumerable<SubstringToSearch> SubstringsToSearch
		{
			get => _substringsToSearch;
			private set => SetField(ref _substringsToSearch, value);
		}

		public virtual string SubstringToSearch
		{
			get => _substringToSearch;
			set => SetField(ref _substringToSearch, value);
		}


		protected override Dictionary<string, object> Parameters
		{
			get
			{
				string[] substrings = { "ALL" };
				if(_activityKind == null && !string.IsNullOrEmpty(SubstringToSearch))
				{
					substrings = new[] { SubstringToSearch };
				}

				if(_activityKind != null)
				{
					substrings = GetSubstrings(SubstringsToSearch.Where(s => s.Selected).Select(s => s.Substring));
				}

				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "end_date", EndDate },
						{ "category_id", _deliveryPointCategory?.Id ?? 0},
						{ "payment_type", _paymentType?.ToString() ?? "ALL" },
						{ "substrings", substrings}
					};

				return parameters;
			}
		}

		private string[] GetSubstrings(IEnumerable<string> strings)
		{
			return strings.Any() ? strings.ToArray() : new[] { "ALL" };
		}

		private void UpdateSubstringsToSearch()
		{
			if(_activityKind != null)
			{
				SubstringsToSearch = _activityKind.GetListOfSubstrings();
			}
			else
			{
				SubstringsToSearch = null;
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
