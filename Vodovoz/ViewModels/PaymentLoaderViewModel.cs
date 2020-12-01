using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.ViewModels;
using QS.Services;
using QS.Navigation;
using Vodovoz.Core.DataService;
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
		private Logger logger = LogManager.GetCurrentClassLogger();
		double progress;

		private bool isNotAutoMatchingMode = true;
		public bool IsNotAutoMatchingMode {
			get => isNotAutoMatchingMode;
			set => SetField(ref isNotAutoMatchingMode, value);
		}
		public TransferDocumentsFromBankParser Parser { get; private set; }
		public GenericObservableList<Payment> ObservablePayments { get; set; }
		public IList<CategoryProfit> ProfitCategories { get; private set; }
		public event Action<string, double> UpdateProgress;

		public PaymentLoaderViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigationManager) 
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager)
		{
			UoW = unitOfWorkFactory.CreateWithoutRoot();

			ObservablePayments = new GenericObservableList<Payment>();

			CreateCommands();
			GetProfitCategories();
		}

		private void CreateCommands() {
			CreateSaveCommand();
			CreateCloseViewModelCommand();
			CreateParseCommand();
		}
		
		private void GetProfitCategories() {
			ProfitCategories = UoW.GetAll<CategoryProfit>().ToList();
		}

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

		private void CreateCloseViewModelCommand()
		{
			CloseViewModelCommand = new DelegateCommand(
				() => Close(false, CloseSource.Cancel),
				() => true
			);
		}

		public DelegateCommand<string> ParseCommand { get; private set; }

		private void CreateParseCommand()
		{
			ParseCommand = new DelegateCommand<string>(
				Init,
				docPath => !string.IsNullOrEmpty(docPath)
			);
		}

		public void Init(string docPath)
		{
			IsNotAutoMatchingMode = false;
			progress = 0;
			UpdateProgress?.Invoke("Начинаем работу...", progress);
			Parser = new TransferDocumentsFromBankParser(docPath);
			Parser.Parse();

			UpdateProgress?.Invoke("Сопоставляем полученные платежи...", progress);
			MatchPayments(new BaseParametersProvider(), new BaseParametersProvider());
		}

		public void MatchPayments(IOrganizationProvider orgProvider, IProfitCategoryProvider profitCategoryProvider)
		{
			var count = 0;
			var countDuplicates = 0;

			AutoPaymentMatching autoPaymentMatching = new AutoPaymentMatching(UoW);
			var org = OrganizationRepository.GetMainOrganization(UoW, orgProvider.GetMainOrganization());
			var defaultProfitCategory = UoW.GetById<CategoryProfit>(profitCategoryProvider.GetDefaultProfitCategory());
			var parserDocs = Parser.TransferDocuments.Where(x => x.RecipientInn == org.INN).ToList();

			progress = 1d / parserDocs.Count;

			foreach(var doc in parserDocs){

				var curDoc = ObservablePayments.SingleOrDefault(x => x.Date == doc.Date
																&& x.PaymentNum == int.Parse(doc.DocNum)
																&& x.CounterpartyInn == doc.PayerInn
																&& x.CounterpartyCurrentAcc == doc.PayerCurrentAccount);

				if(PaymentsRepository.PaymentFromBankClientExists(UoW,
															   	doc.Date,
															   	int.Parse(doc.DocNum),
															   	doc.PayerInn,
															   	doc.PayerCurrentAccount) || curDoc != null) {

					count++;
					countDuplicates++;
					UpdateProgress?.Invoke($"Обработан платеж {count} из {parserDocs.Count}", progress);
					continue;
				}

				var counterparty = CounterpartyRepository.GetCounterpartyByINN(UoW, doc.PayerInn);

				var curPayment = new Payment(doc, org, counterparty);

				if(!autoPaymentMatching.IncomePaymentMatch(curPayment))
					curPayment.Status = PaymentState.undistributed;
				else {
					curPayment.Status = PaymentState.distributed;
				}

				count++;
				curPayment.ProfitCategory = defaultProfitCategory;

				ObservablePayments.Add(curPayment);

				UpdateProgress?.Invoke($"Обработан платеж {count} из {parserDocs.Count}", progress);
			}

			var paymentsSum = ObservablePayments.Sum(x => x.Total);
			UpdateProgress?.Invoke($"Загрузка завершена. Обработано платежей {count} на сумму: {paymentsSum}р. из них не загружено дублей: {countDuplicates}", progress);

			IsNotAutoMatchingMode = true;
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
