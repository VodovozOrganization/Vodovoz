using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NLog;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Services;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class PaymentLoaderViewModel : DialogTabViewModelBase
	{
		private readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IPaymentSettings _paymentSettings;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly int _vodovozId;
		private readonly int _vodovozSouthId;
		private IReadOnlyList<Organization> _organisations;
		private IReadOnlyList<Organization> _allVodOrganisations;
		//убираем из выписки Юмани и банк СИАБ (платежи от физ. лиц)
		private readonly string[] _excludeInnForVodovozSouth = new []{ "2465037737", "7750005725" };

		private double _progress;
		private int _saveAttempts;
		private bool _isNotAutoMatchingMode = true;
		private bool _isSavingState;

		public PaymentLoaderViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices, 
			INavigationManager navigationManager,
			IOrganizationSettings organizationSettings,
			IPaymentSettings paymentSettings,
			IPaymentsRepository paymentsRepository,
			ICounterpartyRepository counterpartyRepository,
			IOrderRepository orderRepository) 
			: base(unitOfWorkFactory, commonServices?.InteractiveService, navigationManager)
		{
			if(commonServices == null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			_paymentSettings = paymentSettings ?? throw new ArgumentNullException(nameof(paymentSettings));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));

			if(organizationSettings == null)
			{
				throw new ArgumentNullException(nameof(organizationSettings));
			}

			InteractiveService = commonServices.InteractiveService;
			_vodovozId = organizationSettings.VodovozOrganizationId;
			_vodovozSouthId = organizationSettings.VodovozSouthOrganizationId;

			UnitOfWorkFactory = unitOfWorkFactory;
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			
			TabName = "Выгрузка выписки из банк-клиента";

			GetOrganisations();
			CreateCommands();
			GetProfitCategories();
		}

		public IUnitOfWorkFactory UnitOfWorkFactory { get; }
		public TransferDocumentsFromBankParser Parser { get; private set; }
		public GenericObservableList<AutoMatchingNode> ObservableIncomes { get; }
			= new GenericObservableList<AutoMatchingNode>();
		public IList<ProfitCategory> ProfitCategories { get; private set; }
		public event Action<string, double> UpdateProgress;
		public IInteractiveService InteractiveService { get; }
		
		public bool IsNotAutoMatchingMode
		{
			get => _isNotAutoMatchingMode;
			set
			{
				if(_isNotAutoMatchingMode == value)
				{
					return;
				}

				_isNotAutoMatchingMode = value;
				OnPropertyChanged(nameof(CanSave));
				OnPropertyChanged(nameof(CanCancel));
				OnPropertyChanged(nameof(CanReadFile));
			}
		}

		public bool IsSavingState
		{
			get => _isSavingState;
			set
			{
				if(_isSavingState == value)
				{
					return;
				}

				_isSavingState = value;
				OnPropertyChanged(nameof(CanSave));
				OnPropertyChanged(nameof(CanCancel));
				OnPropertyChanged(nameof(CanReadFile));
			}
		}
		public bool CanSave => IsNotAutoMatchingMode && !IsSavingState;
		public bool CanCancel => IsNotAutoMatchingMode && !IsSavingState;
		public bool CanReadFile => IsNotAutoMatchingMode && !IsSavingState;

		private void GetOrganisations()
		{
			var orgs = new List<Organization>();

			var vodovozOrg = UoW.GetById<Organization>(_vodovozId);
			var vodovozSouthOrg = UoW.GetById<Organization>(_vodovozSouthId);
			_allVodOrganisations = UoW.GetAll<Organization>().ToList();

			orgs.Add(vodovozOrg);
			orgs.Add(vodovozSouthOrg);

			_organisations = orgs;
		}

		private void CreateCommands() 
		{
			CreateParseCommand();
		}

		private void GetProfitCategories() => ProfitCategories = UoW.GetAll<ProfitCategory>().ToList();

		#region Команды

		public DelegateCommand<string> ParseCommand { get; private set; }

		private void CreateParseCommand()
		{
			ParseCommand = new DelegateCommand<string>(
				Init,
				docPath => !string.IsNullOrEmpty(docPath)
			);
		}

		#endregion Команды

		private void Match(
			ref int count,
			ref int countDuplicates,
			int totalCount,
			AutoPaymentMatching autoPaymentMatching,
			ProfitCategory defaultProfitCategory,
			IList<TransferDocument> parsedPayments,
			Organization org)
		{
			//собрать все нужные параметры для сравнения
			
			//провести проверку по ним
			
			//получить все приходы за эту дату, на которую выписка
			foreach(var doc in parsedPayments)
			{
				var curDoc = ObservableIncomes.SingleOrDefault(
					x => x.Date == doc.Date
					&& x.Number == doc.DocNum
					&& x.OrganizationInn == doc.RecipientInn
					&& x.CounterpartyInn == doc.PayerInn
					&& x.CounterpartyCurrentAcc == doc.PayerCurrentAccount
					&& x.IncomeSum == doc.Total);

				if(curDoc != null)
				{
					count++;
					countDuplicates++;
					UpdateProgress?.Invoke($"Обработан платеж {count} из {totalCount}", _progress);
					continue;
				}

				if(_paymentsRepository.NotManuallyPaymentFromBankClientExists(
					UoW,
					doc.Date,
					int.Parse(doc.DocNum),
					doc.RecipientInn,
					doc.PayerInn,
					doc.PayerCurrentAccount,
					doc.Total))
				{
					count++;
					countDuplicates++;
					UpdateProgress?.Invoke($"Обработан платеж {count} из {totalCount}", _progress);
					continue;
				}

				var counterparty = _counterpartyRepository.GetCounterpartyByINN(UoW, doc.PayerInn);
				var curIncome = CashlessIncome.Create(doc, org, counterparty);

				var payment = curIncome.Payments.First();
				payment.Status =
					!autoPaymentMatching.IncomePaymentMatch(payment)
						? PaymentState.undistributed
						: PaymentState.distributed;

				count++;
				payment.ProfitCategory = defaultProfitCategory;

				AddNewNode(curIncome);

				UpdateProgress?.Invoke($"Обработан платеж {count} из {totalCount}", _progress);
			}
		}

		private void AddNewNode(CashlessIncome curIncome)
		{
			var node = new AutoMatchingNode();
			node.SetCashlessIncome(curIncome);
			
			ObservableIncomes.Add(node);
		}

		private void Init(string docPath)
		{
			IsNotAutoMatchingMode = false;
			_progress = 0;
			UpdateProgress?.Invoke("Начинаем работу...", _progress);
			Parser = new TransferDocumentsFromBankParser(docPath);

			try
			{
				Parser.Parse();
			}
			catch(Exception ex)
			{
				if(ex is NotSupportedException)
				{
					InteractiveService.ShowMessage(ImportanceLevel.Error, ex.Message);
					UpdateProgress?.Invoke("Произошла ошибка во время загрузки", 0);
					return;
				}
			}

			UpdateProgress?.Invoke("Сопоставляем полученные платежи...", _progress);
			MatchPayments();
		}

		private void MatchPayments()
		{
			var count = 0;
			var countDuplicates = 0;
			
			AutoPaymentMatching autoPaymentMatching = new AutoPaymentMatching(UoW, _orderRepository);
			var defaultProfitCategory = UoW.GetById<ProfitCategory>(_paymentSettings.DefaultProfitCategory);
			var paymentsToVodovoz = 
				Parser.TransferDocuments.Where(x => 
					x.RecipientInn == _organisations[0].INN
					&& !_allVodOrganisations.Select(o => o.INN).Contains(x.PayerInn))
					.ToList();
			var paymentsToVodovozSouth = 
				Parser.TransferDocuments.Where(x => 
					x.RecipientInn == _organisations[1].INN
					&& !_excludeInnForVodovozSouth.Contains(x.PayerInn)
					&& !_allVodOrganisations.Select(o => o.INN).Contains(x.PayerInn))
					.ToList();
			var totalCount = paymentsToVodovoz.Count + paymentsToVodovozSouth.Count;

			_progress = 1d / totalCount;

			Match(ref count, ref countDuplicates, totalCount, autoPaymentMatching, defaultProfitCategory, paymentsToVodovoz, _organisations[0]);
			Match(ref count, ref countDuplicates, totalCount, autoPaymentMatching, defaultProfitCategory, paymentsToVodovozSouth, _organisations[1]);

			var paymentsSum = ObservableIncomes.Sum(x => x.IncomeSum);
			UpdateProgress?.Invoke($"Загрузка завершена. Обработано платежей {count} на сумму: {paymentsSum}р. из них не загружено дублей: {countDuplicates}", _progress = 1);

			IsNotAutoMatchingMode = true;
		}

		public void CreateOperations(IUnitOfWork uow, Payment payment)
		{
			payment.CreateIncomeOperation();
			uow.Save(payment.CashlessMovementOperation);

			foreach(var item in payment.ObservableItems)
			{
				item.CreateOrUpdateExpenseOperation();
				uow.Save(item.CashlessMovementOperation);
			}
		}
	}

	public class AutoMatchingNode
	{
		public string Number { get; private set; }
		public DateTime Date { get; private set; }
		public decimal IncomeSum { get; private set; }
		public string Orders { get; private set; }
		public string Payer { get; private set; }
		public string Organization { get; private set; }
		public string PaymentPurpose { get; private set; }
		public ProfitCategory ProfitCategory { get; private set; }
		public PaymentState Status { get; private set; }
		public string OrganizationInn { get; private set; }
		public string CounterpartyInn { get; private set; }
		public string CounterpartyCurrentAcc { get; private set; }
		
		public CashlessIncome CashlessIncome { get; private set; }
		
		public void SetCashlessIncome(CashlessIncome cashlessIncome)
		{
			CashlessIncome = cashlessIncome;
			
			var payment = CashlessIncome.Payments.First();
			
			Number = CashlessIncome.Number.ToString();
			Date = CashlessIncome.Date;
			IncomeSum = CashlessIncome.Total;
			Orders = payment.NumOrders;
			Payer = CashlessIncome.PayerName;
			Organization = CashlessIncome.Organization.Name;
			OrganizationInn = CashlessIncome.Organization.INN;
			CounterpartyInn = CashlessIncome.PayerInn;
			PaymentPurpose = CashlessIncome.PaymentPurpose;
			ProfitCategory = payment.ProfitCategory;
			Status = payment.Status;
			CounterpartyCurrentAcc = CashlessIncome.PayerCurrentAcc;
		}
	}
}
