using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.Banks.Domain;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using ResourceLocker.Library;
using ResourceLocker.Library.Factories;
using Vodovoz.Application.Payments;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
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
		private readonly IGenericRepository<ProfitCategory> _profitCategoryRepository;
		private readonly IGenericRepository<Account> _accountRepository;
		private readonly IGenericRepository<BankAccountMovement> _accountMovementRepository;
		private readonly IGenericRepository<NotAllocatedCounterparty> _notAllocatedCounterpartiesRepository;
		private readonly IResourceLocker _resourceLocker;
		private IReadOnlyDictionary<string, Organization> _allVodOrganisations;
		private IReadOnlyDictionary<string, NotAllocatedCounterparty> _allNotAllocatedCounterparties;

		private double _progress;
		private int _saveAttempts;
		private bool _isNotProcessingMode = true;
		private bool _isSavingState;
		private int _processedPayments;
		private int _paymentDuplicates;
		private int _paymentsWithNotOurOrganizationReceiver;

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
			IGenericRepository<ProfitCategory> profitCategoryRepository,
			IGenericRepository<Account> accountRepository,
			IGenericRepository<BankAccountMovement> accountMovementRepository,
			IGenericRepository<NotAllocatedCounterparty> notAllocatedCounterpartiesRepository,
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
			_profitCategoryRepository = profitCategoryRepository ?? throw new ArgumentNullException(nameof(profitCategoryRepository));
			_accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
			_accountMovementRepository = accountMovementRepository ?? throw new ArgumentNullException(nameof(accountMovementRepository));
			_notAllocatedCounterpartiesRepository =
				notAllocatedCounterpartiesRepository ?? throw new ArgumentNullException(nameof(notAllocatedCounterpartiesRepository));

			InteractiveService = commonServices.InteractiveService;
			UnitOfWorkFactory = unitOfWorkFactory;
			UoW = UnitOfWorkFactory.CreateWithoutRoot("Выгрузка выписки из банк-клиента");
			
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

			TabName = "Выгрузка выписки из банк-клиента";

			GetOrganisations();
			GetNotAllocatedCounterparties();
			CreateCommands();
			GetProfitCategories();
		}

		public IUnitOfWorkFactory UnitOfWorkFactory { get; }
		public TransferDocumentsFromBankParser Parser { get; private set; }
		public GenericObservableList<Payment> ObservablePayments { get; } =	new GenericObservableList<Payment>();
		public IList<ProfitCategory> ProfitCategories { get; private set; }
		public IList<BankAccountMovement> BankAccountMovements = new List<BankAccountMovement>();
		public event Action<string, double> UpdateProgress;
		public IInteractiveService InteractiveService { get; }
		
		public bool IsNotProcessingMode
		{
			get => _isNotProcessingMode;
			set
			{
				if(_isNotProcessingMode == value)
				{
					return;
				}

				_isNotProcessingMode = value;
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
		public bool CanSave => IsNotProcessingMode && !IsSavingState;
		public bool CanCancel => IsNotProcessingMode && !IsSavingState;
		public bool CanReadFile => IsNotProcessingMode && !IsSavingState;

		private void GetOrganisations()
		{
			_allVodOrganisations = _organizationRepository.Get(UoW)
				.ToList()
				.ToDictionary(x => x.INN);
		}

		private void GetNotAllocatedCounterparties()
		{
			_allNotAllocatedCounterparties = _notAllocatedCounterpartiesRepository.Get(UoW)
				.ToList()
				.ToDictionary(x => x.Inn);
		}

		private void CreateCommands() 
		{
			ProcessBankDocumentCommand = new DelegateCommand<string>(
				ProcessBankDocument,
				docPath => !string.IsNullOrEmpty(docPath)
			);
		}

		private void GetProfitCategories() => ProfitCategories = _profitCategoryRepository
			.Get(UoW, x => x.IsArchive == false)
			.ToList();

		#region Команды

		public DelegateCommand<string> ProcessBankDocumentCommand { get; private set; }

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

			foreach(var doc in parsedDocuments)
			{
				if(string.IsNullOrWhiteSpace(doc.RecipientInn) || !_allVodOrganisations.ContainsKey(doc.RecipientInn))
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
						&& x.CounterpartyAcc == doc.PayerAccount
						&& x.Total == doc.Total);

				if(_paymentsRepository.NotManuallyPaymentFromBankClientExists(
					UoW,
					docDate,
					int.Parse(doc.DocNum),
					doc.RecipientInn,
					doc.PayerInn,
					doc.PayerAccount,
					doc.Total) || curDoc != null)
				{
					count++;
					countDuplicates++;
					UpdateProgress?.Invoke($"Обработан платеж {count} из {totalCount}", _progress);
					continue;
				}

				var counterparty = _counterpartyRepository.GetCounterpartyByINN(UoW, doc.PayerInn);
				var curPayment = new Payment(doc, _allVodOrganisations[doc.RecipientInn], counterparty);

				if(_allVodOrganisations.ContainsKey(doc.RecipientInn) && _allVodOrganisations.ContainsKey(doc.PayerInn))
				{
					curPayment.OtherIncome(otherProfitCategory);
				}
				else if(_allVodOrganisations.ContainsKey(doc.RecipientInn)
					&& _allNotAllocatedCounterparties.TryGetValue(doc.PayerInn, out var notAllocatedCounterparty))
				{
					curPayment.OtherIncome(notAllocatedCounterparty.ProfitCategory);
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

		private void ProcessBankDocument(string docPath)
		{
			IsNotProcessingMode = false;
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

			if(!HandleBankAccountMovements())
			{
				return;
			}

			UpdateProgress?.Invoke("Сопоставляем полученные платежи...", _progress);
			MatchPayments();
			
			var paymentsSum = ObservablePayments.Sum(x => x.Total);
			UpdateProgress?.Invoke(
				$"Загрузка завершена. Обработано платежей {_processedPayments} на сумму: {paymentsSum}р." +
				$" из них платежей не к нашим организациям или исходящих: {_paymentsWithNotOurOrganizationReceiver} и не загружено дублей: {_paymentDuplicates}",
				_progress = 1);
			
			IsNotProcessingMode = true;
		}

		private void MatchPayments()
		{
			_processedPayments = 0;
			_paymentDuplicates = 0;
			_paymentsWithNotOurOrganizationReceiver = 0;
			
			var autoPaymentMatching = new AutoPaymentMatching(UoW, _orderRepository);
			var totalCount = Parser.TransferDocuments.Count;

			_progress = 1d / totalCount;
			
			Match(
				ref _processedPayments,
				ref _paymentDuplicates,
				ref _paymentsWithNotOurOrganizationReceiver,
				totalCount,
				autoPaymentMatching,
				Parser.TransferDocuments);
		}
		
		private bool HandleBankAccountMovements()
		{
			UpdateProgress?.Invoke("Обрабатываем данные по расчетным счетам...", _progress);
			var resultProcessBankAccountMovement = ProcessBankAccountMovement();
			
			if(resultProcessBankAccountMovement.IsFailure)
			{
				IsNotProcessingMode = true;
				UpdateProgress?.Invoke(resultProcessBankAccountMovement.Errors.First().Message, 1);
				return false;
			}
			
			UpdateProgress?.Invoke("Сопоставляем данные по расчетным счетам...", _progress);
			var matchBankAccountMovementResult = MatchBankAccountMovement();
			
			if(matchBankAccountMovementResult.IsFailure)
			{
				IsNotProcessingMode = true;
				UpdateProgress?.Invoke(matchBankAccountMovementResult.Errors.First().Message, 1);
				return false;
			}

			return true;
		}
		
		private Result ProcessBankAccountMovement()
		{
			BankAccountMovements.Clear();
			var builder = BankAccountMovementBuilder.Create();
			
			foreach(var accountData in Parser.Accounts)
			{
				foreach(var data in accountData)
				{
					builder.AddData(data);
				}
				
				var accountMovement = builder.Build();
				var account = _accountRepository
					.Get(UoW, x => x.Number == accountMovement.AccountNumber)
					.FirstOrDefault();

				if(account is null)
				{
					BankAccountMovements.Clear();
					return Result.Failure(new Error(
						"AccountNotFound",
						$"Не найден расчетный счет {accountMovement.AccountNumber} у наших организаций. " +
						"Добавьте этот р/сч к нужной организации через справочник организаций и повторно загрузите выписку"));
				}
				
				accountMovement.Account = account;
				accountMovement.Bank = account.InBank;
				BankAccountMovements.Add(accountMovement);
			}
			
			return Result.Success();
		}
		
		private Result MatchBankAccountMovement()
		{
			var i = 0;
			while(i < BankAccountMovements.Count)
			{
				var bankAccountMovement = BankAccountMovements[i];
				
				var accountMovementFromBase = _accountMovementRepository.Get(
						UoW,
						x => x.StartDate == bankAccountMovement.StartDate
							&& x.EndDate == bankAccountMovement.EndDate
							&& x.Account.Number == bankAccountMovement.Account.Number)
					.FirstOrDefault();

				if(accountMovementFromBase != null)
				{
					accountMovementFromBase.UpdateData(bankAccountMovement.BankAccountMovements);
					BankAccountMovements[i] = accountMovementFromBase;
					i++;
					continue;
				}
				
				accountMovementFromBase = _accountMovementRepository.Get(
						UoW,
						x => x.StartDate <= bankAccountMovement.StartDate
							&& x.EndDate >= bankAccountMovement.StartDate
							&& x.Account.Number == bankAccountMovement.Account.Number)
					.FirstOrDefault();

				if(accountMovementFromBase is null)
				{
					accountMovementFromBase = _accountMovementRepository.Get(
							UoW,
							x => x.StartDate <= bankAccountMovement.EndDate
								&& x.EndDate >= bankAccountMovement.EndDate
								&& x.Account.Number == bankAccountMovement.Account.Number)
						.FirstOrDefault();

					if(accountMovementFromBase is null)
					{
						i++;
						continue;
					}
				}
				
				return Result.Failure(new Error(
					"DuplicateAccountMovementFound",
					$"В базе уже есть загруженная выписка по расчетному счету {accountMovementFromBase.AccountNumber}" +
					$" с {accountMovementFromBase.StartDate} по {accountMovementFromBase.EndDate}" +
					"Нельзя загружать выписки за день, если есть интервальная выписка, включающая этот день и наоборот, " +
					"нельзя загружать интервальные выписки, если уже есть выписка за один день, входящий в интервал"));
			}

			return Result.Success();
		}
		
		public override void Dispose()
		{
			_resourceLocker.DisposeAsync().AsTask().GetAwaiter().GetResult();
			base.Dispose();
		}
	}
}
