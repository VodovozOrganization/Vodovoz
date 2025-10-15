using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using ResourceLocker.Library;
using ResourceLocker.Library.Factories;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Services;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class PaymentLoaderViewModel : DialogTabViewModelBase
	{
		private readonly ILogger<PaymentLoaderViewModel> _logger;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IPaymentSettings _paymentSettings;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IGenericRepository<Organization> _organizationRepository;
		private readonly IResourceLocker _resourceLocker;
		private IReadOnlyDictionary<string, Organization> _allVodOrganisations;
		//убираем из выписки Юмани и банк СИАБ (платежи от физ. лиц)
		private readonly string[] _excludeInnPayers = new []{ "2465037737", "7750005725" };

		private double _progress;
		private int _saveAttempts;
		private bool _isNotAutoMatchingMode = true;
		private bool _isSavingState;

		public PaymentLoaderViewModel(
			ILogger<PaymentLoaderViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices, 
			INavigationManager navigationManager,
			IOrganizationSettings organizationSettings,
			IPaymentSettings paymentSettings,
			IPaymentsRepository paymentsRepository,
			ICounterpartyRepository counterpartyRepository,
			IOrderRepository orderRepository,
			IGenericRepository<Organization> organizationRepository,
			IResourceLockerFactory resourceLockerFactory,
			IUserRepository userRepository) 
			: base(unitOfWorkFactory, commonServices?.InteractiveService, navigationManager)
		{
			if(commonServices == null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_paymentSettings = paymentSettings ?? throw new ArgumentNullException(nameof(paymentSettings));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));

			InteractiveService = commonServices.InteractiveService;
			
			_resourceLocker = resourceLockerFactory.Create($"{nameof(PaymentLoaderViewModel)}");
			
			var lockResult = _resourceLocker.TryLockResourceAsync().GetAwaiter().GetResult();

			if(!lockResult.IsSuccess)
			{
				var ownerUser = userRepository.GetUserByLogin(UoW, lockResult.OwnerLockValue?.Split(':')[0]);

				throw new AbortCreatingPageException(
					$"Диалог уже открыт пользователем {ownerUser?.Name}",
					"Не удалось открыть диалог",
					ImportanceLevel.Warning);
			}
			
			UnitOfWorkFactory = unitOfWorkFactory;
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			
			TabName = "Выгрузка выписки из банк-клиента";

			GetOrganisations();
			CreateCommands();
			GetProfitCategories();
		}

		public IUnitOfWorkFactory UnitOfWorkFactory { get; }
		public TransferDocumentsFromBankParser Parser { get; private set; }
		public GenericObservableList<Payment> ObservablePayments { get; } =	new GenericObservableList<Payment>();
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
			_allVodOrganisations = _organizationRepository.Get(UoW)
				.ToList()
				.ToDictionary(x => x.INN);
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
				Parse,
				docPath => !string.IsNullOrEmpty(docPath)
			);
		}

		#endregion Команды
		
		public void CreateOperations(IUnitOfWork uow, Payment payment)
		{
			payment.CreateIncomeOperation();
			uow.Save(payment.CashlessMovementOperation);

			foreach(var item in payment.Items)
			{
				item.CreateOrUpdateExpenseOperation();
				uow.Save(item.CashlessMovementOperation);
			}
		}

		private void Match(
			ref int count,
			ref int countDuplicates,
			ref int countUnknownInn,
			int totalCount,
			AutoPaymentMatching autoPaymentMatching,
			IReadOnlyCollection<TransferDocument> parsedDocuments)
		{
			var defaultProfitCategory = UoW.GetById<ProfitCategory>(_paymentSettings.DefaultProfitCategoryId);
			var otherProfitCategory = UoW.GetById<ProfitCategory>(_paymentSettings.OtherProfitCategoryId);
			
			var vodovozOrganization = _organizationRepository
				.Get(UoW, x => x.Id == _organizationSettings.VodovozOrganizationId)
				.FirstOrDefault();

			foreach(var doc in parsedDocuments)
			{
				if(!_allVodOrganisations.ContainsKey(doc.RecipientInn))
				{
					count++;
					countUnknownInn++;
					UpdateProgress?.Invoke($"Обработан платеж {count} из {totalCount}", _progress);
					continue;
				}

				var docDate = doc.ReceivedDate ?? doc.Date;

				var curDoc = ObservablePayments.SingleOrDefault(
					x => x.Date == docDate
						&& x.PaymentNum == int.Parse(doc.DocNum)
						&& x.Organization.INN == doc.RecipientInn
						&& x.CounterpartyInn == doc.PayerInn
						&& x.CounterpartyCurrentAcc == doc.PayerCurrentAccount
						&& x.Total == doc.Total);

				if(_paymentsRepository.NotManuallyPaymentFromBankClientExists(
					UoW,
					docDate,
					int.Parse(doc.DocNum),
					doc.RecipientInn,
					doc.PayerInn,
					doc.PayerCurrentAccount,
					doc.Total) || curDoc != null)
				{
					count++;
					countDuplicates++;
					UpdateProgress?.Invoke($"Обработан платеж {count} из {totalCount}", _progress);
					continue;
				}

				var counterparty = _counterpartyRepository.GetCounterpartyByINN(UoW, doc.PayerInn);
				var curPayment = new Payment(doc, _allVodOrganisations[doc.RecipientInn], counterparty);

				if((_allVodOrganisations.ContainsKey(doc.RecipientInn) && _allVodOrganisations.ContainsKey(doc.PayerInn))
					|| (doc.RecipientInn == vodovozOrganization.INN && _excludeInnPayers.Contains(doc.PayerInn)))
				{
					curPayment.OtherIncome(otherProfitCategory);
				}
				else
				{
					curPayment.ProfitCategory = defaultProfitCategory;
					curPayment.Status =
						!autoPaymentMatching.IncomePaymentMatch(curPayment)
							? PaymentState.undistributed
							: PaymentState.distributed;
				}

				count++;
				ObservablePayments.Add(curPayment);
				UpdateProgress?.Invoke($"Обработан платеж {count} из {totalCount}", _progress);
			}
		}

		private void Parse(string docPath)
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
			var countUnknownInn = 0;
			
			var autoPaymentMatching = new AutoPaymentMatching(UoW, _orderRepository);
			var totalCount = Parser.TransferDocuments.Count;

			_progress = 1d / totalCount;
			Match(ref count, ref countDuplicates, ref countUnknownInn, totalCount, autoPaymentMatching, Parser.TransferDocuments);

			var paymentsSum = ObservablePayments.Sum(x => x.Total);
			UpdateProgress?.Invoke(
				$"Загрузка завершена. Обработано платежей {count} на сумму: {paymentsSum}р." +
				$" из них не загружено дублей: {countDuplicates} и неизвестных ИНН: {countUnknownInn}", _progress = 1);

			IsNotAutoMatchingMode = true;
		}
		
		public override void Dispose()
		{
			_resourceLocker.DisposeAsync().AsTask().GetAwaiter().GetResult();
			base.Dispose();
		}
	}
}
