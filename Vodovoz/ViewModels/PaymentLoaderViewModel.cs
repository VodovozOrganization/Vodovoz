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
using Vodovoz.Repositories.Payments;
using QS.Commands;
using Vodovoz.Repositories;

namespace Vodovoz.ViewModels
{
	public class PaymentLoaderViewModel : DialogTabViewModelBase
	{
		private readonly IProfitCategoryProvider profitCategoryProvider;
		private readonly int vodovozId;
		private readonly int vodovozSouthId;
		private IReadOnlyList<Domain.Organizations.Organization> organisations;		
		private IReadOnlyList<Domain.Organizations.Organization> allVodOrganisations;
		//убираем из выписки Юмани и банк СИАБ (платежи от физ. лиц)
		private readonly string[] excludeInnForVodovozSouth = new []{ "2465037737", "7750005725" };

		private Logger logger = LogManager.GetCurrentClassLogger();
		double progress;

		private bool isNotAutoMatchingMode = true;
		public bool IsNotAutoMatchingMode {
			get => isNotAutoMatchingMode;
			set => SetField(ref isNotAutoMatchingMode, value);
		}
		public TransferDocumentsFromBankParser Parser { get; private set; }
		public GenericObservableList<Payment> ObservablePayments { get; } = 
			new GenericObservableList<Payment>();
		public IList<CategoryProfit> ProfitCategories { get; private set; }
		public event Action<string, double> UpdateProgress;

		public PaymentLoaderViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices, 
			INavigationManager navigationManager,
			IOrganizationParametersProvider organizationParametersProvider,
			IProfitCategoryProvider profitCategoryProvider) 
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager)
		{
			this.profitCategoryProvider = profitCategoryProvider ?? throw new ArgumentNullException(nameof(profitCategoryProvider));

			if(organizationParametersProvider == null)
            {
				throw new ArgumentNullException(nameof(organizationParametersProvider));
            }

			vodovozId = organizationParametersProvider.VodovozOrganizationId;
			vodovozSouthId = organizationParametersProvider.VodovozSouthOrganizationId;

			UoW = unitOfWorkFactory.CreateWithoutRoot();
			
			TabName = "Выгрузка выписки из банк-клиента";

			GetOrganisations();
			CreateCommands();
			GetProfitCategories();
		}

		private void GetOrganisations()
        {
			var orgs = new List<Domain.Organizations.Organization>();

			var vodovozOrg = UoW.GetById<Domain.Organizations.Organization>(vodovozId);
			var vodovozSouthOrg = UoW.GetById<Domain.Organizations.Organization>(vodovozSouthId);
			allVodOrganisations = UoW.GetAll<Domain.Organizations.Organization>().ToList();

			orgs.Add(vodovozOrg);
			orgs.Add(vodovozSouthOrg);

			organisations = orgs;
        }

		private void CreateCommands() {
			CreateSaveCommand();
			CreateParseCommand();
		}
		
		private void GetProfitCategories() {
			ProfitCategories = UoW.GetAll<CategoryProfit>().ToList();
		}

        #region Команды

        public DelegateCommand<GenericObservableList<Payment>> SaveCommand { get; private set; }

		private void CreateSaveCommand()
		{
			SaveCommand = new DelegateCommand<GenericObservableList<Payment>>(
				payments => {

					UpdateProgress?.Invoke("Начинаем сохранение...", progress = 0);

					foreach(Payment payment in payments) {

						if(payment.Status == PaymentState.distributed) {
							CreateOperations(payment);
						}
						UoW.Save(payment);
					}

					UoW.Commit();
					UpdateProgress?.Invoke("Сохранение закончено...", progress = 0);
					Close(false, CloseSource.Self);
				},
				payments => payments.Count > 0
			);
		}

		public DelegateCommand CloseViewModelCommand { get; private set; }

		public DelegateCommand<string> ParseCommand { get; private set; }

		private void CreateParseCommand()
		{
			ParseCommand = new DelegateCommand<string>(
				Init,
				docPath => !string.IsNullOrEmpty(docPath)
			);
		}

        #endregion Команды

        public void Init(string docPath)
		{
			IsNotAutoMatchingMode = false;
			progress = 0;
			UpdateProgress?.Invoke("Начинаем работу...", progress);
			Parser = new TransferDocumentsFromBankParser(docPath);
			Parser.Parse();

			UpdateProgress?.Invoke("Сопоставляем полученные платежи...", progress);
			MatchPayments();
		}

		public void MatchPayments()
        {
            var count = 0;
            var countDuplicates = 0;
			
            AutoPaymentMatching autoPaymentMatching = new AutoPaymentMatching(UoW);
            var defaultProfitCategory = UoW.GetById<CategoryProfit>(profitCategoryProvider.GetDefaultProfitCategory());
            var paymentsToVodovoz = 
	            Parser.TransferDocuments.Where(x => 
		            x.RecipientInn == organisations[0].INN
		            && !allVodOrganisations.Select(o => o.INN).Contains(x.PayerInn))
		            .ToList();
            var paymentsToVodovozSouth = 
				Parser.TransferDocuments.Where(x => 
					x.RecipientInn == organisations[1].INN
					&& !excludeInnForVodovozSouth.Contains(x.PayerInn)
					&& !allVodOrganisations.Select(o => o.INN).Contains(x.PayerInn))
					.ToList();
			var totalCount = paymentsToVodovoz.Count + paymentsToVodovozSouth.Count;

			progress = 1d / totalCount;

            Match(ref count, ref countDuplicates, totalCount, autoPaymentMatching, defaultProfitCategory, paymentsToVodovoz, organisations[0]);
			Match(ref count, ref countDuplicates, totalCount, autoPaymentMatching, defaultProfitCategory, paymentsToVodovozSouth, organisations[1]);

            var paymentsSum = ObservablePayments.Sum(x => x.Total);
            UpdateProgress?.Invoke($"Загрузка завершена. Обработано платежей {count} на сумму: {paymentsSum}р. из них не загружено дублей: {countDuplicates}", progress);

            IsNotAutoMatchingMode = true;
        }

        private void Match(
			ref int count,
			ref int countDuplicates,
			int totalCount,
			AutoPaymentMatching autoPaymentMatching,
			CategoryProfit defaultProfitCategory,
			IList<TransferDocument> parsedPayments,
			Domain.Organizations.Organization org)
        {
            foreach (var doc in parsedPayments)
            {
                var curDoc = ObservablePayments.SingleOrDefault(x => x.Date == doc.Date
                                                                && x.PaymentNum == int.Parse(doc.DocNum)
                                                                && x.Organization.INN == doc.RecipientInn
                                                                && x.CounterpartyInn == doc.PayerInn
                                                                && x.CounterpartyCurrentAcc == doc.PayerCurrentAccount);

                if (PaymentsRepository.PaymentFromBankClientExists(UoW,
                                                                   doc.Date,
                                                                   int.Parse(doc.DocNum),
																   doc.RecipientInn,
                                                                   doc.PayerInn,
                                                                   doc.PayerCurrentAccount) || curDoc != null)
                {
                    count++;
                    countDuplicates++;
                    UpdateProgress?.Invoke($"Обработан платеж {count} из {totalCount}", progress);
                    continue;
                }

                var counterparty = CounterpartyRepository.GetCounterpartyByINN(UoW, doc.PayerInn);
				var curPayment = new Payment(doc, org, counterparty);

				if (!autoPaymentMatching.IncomePaymentMatch(curPayment)){
					curPayment.Status = PaymentState.undistributed;
				}
				else{
					curPayment.Status = PaymentState.distributed;
				}

                count++;
                curPayment.ProfitCategory = defaultProfitCategory;

                ObservablePayments.Add(curPayment);

                UpdateProgress?.Invoke($"Обработан платеж {count} из {totalCount}", progress);
            }
        }

        private void CreateOperations(Payment payment)
		{
			payment.CreateIncomeOperation();
			UoW.Save(payment.CashlessMovementOperation);

			foreach(PaymentItem item in payment.ObservableItems) {
				item.CreateExpenseOperation();
				UoW.Save(item.CashlessMovementOperation);
			}
		}
	}
}
