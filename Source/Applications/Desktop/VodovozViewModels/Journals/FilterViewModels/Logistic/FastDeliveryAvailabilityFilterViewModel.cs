using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using System;
using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class FastDeliveryAvailabilityFilterViewModel : FilterViewModelBase<FastDeliveryAvailabilityFilterViewModel>
	{
		private ILifetimeScope _lifetimeScope;
		private DateTime? _verificationDateFrom;
		private DateTime? _verificationDateEndTo;
		private Counterparty _counterparty;
		private Employee _logistician;
		private District _district;
		private bool? _isValid;
		private bool? _isVerificationFromSite;
		private bool? _isNomenclatureNotInStock;
		private int _logisticianReactionTimeMinutes;
		private DelegateCommand _infoCommand;
		private string _failsReportName;
		private DelegateCommand _failsReportCommand;

		public FastDeliveryAvailabilityFilterViewModel(
			ILifetimeScope lifetimeScope,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IDistrictJournalFactory districtJournalFactory)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			EmployeeSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();

			var districtFilter = new DistrictJournalFilterViewModel()
			{
				Status = DistrictsSetStatus.Active
			};
			DistrictSelectorFactory =
				(districtJournalFactory ?? throw new ArgumentNullException(nameof(districtJournalFactory)))
				.CreateDistrictAutocompleteSelectorFactory(districtFilter);

			CounterpartySelectorFactory =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope);

			IsValid = false;
			IsVerificationFromSite = false;
			IsNomenclatureNotInStock = false;
		}

		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DistrictSelectorFactory { get; }

		public DateTime? VerificationDateFrom
		{
			get => _verificationDateFrom;
			set => UpdateFilterField(ref _verificationDateFrom, value);
		}

		public DateTime? VerificationDateTo
		{
			get => _verificationDateEndTo;
			set => UpdateFilterField(ref _verificationDateEndTo, value);
		}

		public Counterparty Counterparty
		{
			get => _counterparty;
			set => UpdateFilterField(ref _counterparty, value);
		}

		public District District
		{
			get => _district;
			set => UpdateFilterField(ref _district, value);
		}

		public bool? IsValid
		{
			get => _isValid;
			set => UpdateFilterField(ref _isValid, value);
		}

		public bool? IsVerificationFromSite
		{
			get => _isVerificationFromSite;
			set => UpdateFilterField(ref _isVerificationFromSite, value);
		}

		public bool? IsNomenclatureNotInStock
		{
			get => _isNomenclatureNotInStock; 
			set => UpdateFilterField(ref _isNomenclatureNotInStock, value);
		}

		public Employee Logistician
		{
			get => _logistician;
			set => UpdateFilterField(ref _logistician, value);
		}

		public int LogisticianReactionTimeMinutes
		{
			get => _logisticianReactionTimeMinutes;
			set => UpdateFilterField(ref _logisticianReactionTimeMinutes, value);
		}

		public string FailsReportName
		{
			get => _failsReportName;
			set => SetField(ref _failsReportName, value);
		}

		public Action FailsReportAction;

		public DelegateCommand InfoCommand => _infoCommand ?? (_infoCommand = new DelegateCommand(
			() => ServicesConfig.InteractiveService.ShowMessage(
				ImportanceLevel.Info,
				"Отчёт по заказам с ассортиментом из запаса доступен при установленном крестике на фильтре \"Ассортимент не в запасе\".\n"+
				"В нём отображаются заказы, которые в итоге не стали заказами с доставкой за час.\n" +
				"В столбце \"Не доставлено заказов\" считаются уникальные заказы.\nВ остальных столбцах кол-во проблем (по одному заказу может быть несколько проверок).\n" +
				"Если в проверке не нашлось подходящих по расстоянию автомобилей, то в колонку \"Большое расстояние\" попадает 1, а в остальные колонки 0.\n" +
				"В противном случае суммируются показатели проверки для автомобилей подходящих по расстоянию с группировкой по адресам , а в колонку с расстоянием попадает 0.\n\n" +
				"Отчёт по всем заказам - всё также, как и в отчёте по заказам с ассортиментом из запаса, только с выключеннымм фильтром \"Ассортимент не в запасе\"\n\n" +
				"Отчёт \"Анализ ассортимента не в запасе\" доступен при установленной галочке в фильтре \"Ассортимент не в запасе\"\n" +
				"В нём отображаются заказы, которые в итоге не стали заказами с доставкой за час. \n" +
				"Отчёт показывает какие номенклатуры чаще других находились в заказе и становились причиной, по которой заказ не смогли доставить за час. \n" +
				"Ассортимент из запаса исключаем \n\n" +
				"Примечание: ассортимент не в запасе - истина, если есть хотя бы одна номенклатура, которая была в заказе с запросом на ДЗЧ, при этом отсутствовала в дополнительной погрузке (запасе)")
		));

		public DelegateCommand FailsReportCommand => _failsReportCommand ?? (_failsReportCommand = new DelegateCommand(
			() => FailsReportAction?.Invoke()
		));

		public void InitFailsReport() => OnPropertyChanged(nameof(IsNomenclatureNotInStock));

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
