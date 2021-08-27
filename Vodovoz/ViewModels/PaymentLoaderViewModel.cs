using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.ViewModels;
using QS.Services;
using QS.Navigation;
using Vodovoz.Services;
using Vodovoz.Domain.Payments;
using NLog;
using QS.Commands;
using Vodovoz.Domain.Orders;
using System.Threading.Tasks;
using QS.Dialog;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.ViewModels
{
	public class PaymentLoaderViewModel : DialogTabViewModelBase
	{
		private readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IProfitCategoryProvider _profitCategoryProvider;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly int _vodovozId;
		private readonly int _vodovozSouthId;
		private IReadOnlyList<Domain.Organizations.Organization> _organisations;		
		private IReadOnlyList<Domain.Organizations.Organization> _allVodOrganisations;
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
			IOrganizationParametersProvider organizationParametersProvider,
			IProfitCategoryProvider profitCategoryProvider,
			IPaymentsRepository paymentsRepository,
			ICounterpartyRepository counterpartyRepository) 
			: base(unitOfWorkFactory, commonServices?.InteractiveService, navigationManager)
		{
			if(commonServices == null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			_profitCategoryProvider = profitCategoryProvider ?? throw new ArgumentNullException(nameof(profitCategoryProvider));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));

			if(organizationParametersProvider == null)
			{
				throw new ArgumentNullException(nameof(organizationParametersProvider));
			}

			InteractiveService = commonServices.InteractiveService;
			_vodovozId = organizationParametersProvider.VodovozOrganizationId;
			_vodovozSouthId = organizationParametersProvider.VodovozSouthOrganizationId;

			UoW = unitOfWorkFactory.CreateWithoutRoot();
			
			TabName = "Выгрузка выписки из банк-клиента";

			GetOrganisations();
			CreateCommands();
			GetProfitCategories();
		}

		public TransferDocumentsFromBankParser Parser { get; private set; }
		public GenericObservableList<Payment> ObservablePayments { get; } =	new GenericObservableList<Payment>();
		public IList<CategoryProfit> ProfitCategories { get; private set; }
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
			var orgs = new List<Domain.Organizations.Organization>();

			var vodovozOrg = UoW.GetById<Domain.Organizations.Organization>(_vodovozId);
			var vodovozSouthOrg = UoW.GetById<Domain.Organizations.Organization>(_vodovozSouthId);
			_allVodOrganisations = UoW.GetAll<Domain.Organizations.Organization>().ToList();

			orgs.Add(vodovozOrg);
			orgs.Add(vodovozSouthOrg);

			_organisations = orgs;
		}

		private void CreateCommands() 
		{
			CreateParseCommand();
		}

		private void GetProfitCategories() => ProfitCategories = UoW.GetAll<CategoryProfit>().ToList();

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
			CategoryProfit defaultProfitCategory,
			IList<TransferDocument> parsedPayments,
			Domain.Organizations.Organization org)
		{
			foreach(var doc in parsedPayments)
			{
				var curDoc = ObservablePayments.SingleOrDefault(
					x => x.Date == doc.Date
					&& x.PaymentNum == int.Parse(doc.DocNum)
					&& x.Organization.INN == doc.RecipientInn
					&& x.CounterpartyInn == doc.PayerInn
					&& x.CounterpartyCurrentAcc == doc.PayerCurrentAccount);

				if(_paymentsRepository.PaymentFromBankClientExists(
					UoW,
					doc.Date,
					int.Parse(doc.DocNum),
					doc.RecipientInn,
					doc.PayerInn,
					doc.PayerCurrentAccount) || curDoc != null)
				{
					count++;
					countDuplicates++;
					UpdateProgress?.Invoke($"Обработан платеж {count} из {totalCount}", _progress);
					continue;
				}

				var counterparty = _counterpartyRepository.GetCounterpartyByINN(UoW, doc.PayerInn);
				var curPayment = new Payment(doc, org, counterparty);

				if(!autoPaymentMatching.IncomePaymentMatch(curPayment))
				{
					curPayment.Status = PaymentState.undistributed;
				}
				else
				{
					curPayment.Status = PaymentState.distributed;
				}

				count++;
				curPayment.ProfitCategory = defaultProfitCategory;

				ObservablePayments.Add(curPayment);

				UpdateProgress?.Invoke($"Обработан платеж {count} из {totalCount}", _progress);
			}
		}

		public async Task<bool> SaveAllocatedOrderAsync(PaymentItem paymentItem)
		{
			try
			{
				var order = UoW.GetById<Order>(paymentItem.Order.Id);
				order.OrderPaymentStatus = OrderPaymentStatus.Paid;

				UoW.Save(order);
				UoW.Commit();

				return true;
			}
			catch(Exception ex)
			{
				UoW.Session.Clear();
				_logger.Error(ex);
				_saveAttempts++;

				if(_saveAttempts >= 3)
				{
					return false;
				}

				await Task.Delay(1000);
				return await SaveAllocatedOrderAsync(paymentItem);
			}
		}

		public void Init(string docPath)
		{
			IsNotAutoMatchingMode = false;
			_progress = 0;
			UpdateProgress?.Invoke("Начинаем работу...", _progress);
			Parser = new TransferDocumentsFromBankParser(docPath);
			Parser.Parse();

			UpdateProgress?.Invoke("Сопоставляем полученные платежи...", _progress);
			MatchPayments();
		}

		public void MatchPayments()
		{
			var count = 0;
			var countDuplicates = 0;
			
			AutoPaymentMatching autoPaymentMatching = new AutoPaymentMatching(UoW);
			var defaultProfitCategory = UoW.GetById<CategoryProfit>(_profitCategoryProvider.GetDefaultProfitCategory());
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

			var paymentsSum = ObservablePayments.Sum(x => x.Total);
			UpdateProgress?.Invoke($"Загрузка завершена. Обработано платежей {count} на сумму: {paymentsSum}р. из них не загружено дублей: {countDuplicates}", _progress = 1);

			IsNotAutoMatchingMode = true;
		}

		public void CreateOperations(Payment payment)
		{
			payment.CreateIncomeOperation();
			UoW.Save(payment.CashlessMovementOperation);

			foreach(PaymentItem item in payment.ObservableItems)
			{
				item.CreateExpenseOperation();
				UoW.Save(item.CashlessMovementOperation);
			}
		}
	}
}
