using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Repository;
using QS.ViewModels;
using QS.Services;
using QS.Navigation;
using Vodovoz.Core.DataService;
using Vodovoz.Services;
using Vodovoz.Domain.Payments;
using NLog;
using System.Collections.Generic;
using Vodovoz.Repositories.Payments;
using Vodovoz.Domain.Operations;
using QS.Commands;
using System.Threading.Tasks;

namespace Vodovoz.ViewModels
{
	public class PaymentLoaderVM : DialogTabViewModelBase
	{
		public TransferDocumentsFromBankParser Parser { get; private set; }

		GenericObservableList<Payment> observablePayments;
		public GenericObservableList<Payment> ObservablePayments { 
			get => observablePayments;
			set => SetField(ref observablePayments, value);
		}

		readonly ICommonServices commonServices;
		private Logger logger = LogManager.GetCurrentClassLogger();
		double progress;

		private bool isNotAutoMatchingMode;
		public bool IsNotAutoMatchingMode { 
			get => isNotAutoMatchingMode; 
			set => SetField(ref isNotAutoMatchingMode, value); 
		}

		public event Action<string, double> UpdateProgress;

		public PaymentLoaderVM(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigationManager) 
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			UoW = unitOfWorkFactory.CreateWithoutRoot();

			ObservablePayments = new GenericObservableList<Payment>();

			CreateCommands();
		}

		private void CreateCommands()
		{
			CreateSaveCommand();
		}

		public DelegateCommand<GenericObservableList<Payment>> SaveCommand { get; private set; }

		private void CreateSaveCommand()
		{
			SaveCommand = new DelegateCommand<GenericObservableList<Payment>>(
				payments => {
					if(IsNotAutoMatchingMode) {
						UoW.Commit();
					}
					UpdateProgress?.Invoke("Сохранение закончено...", progress = 0);
				},
				payments => payments.Count > 0
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
			var list = new List<Payment>();

			AutoPaymentMatching autoPaymentMatching = new AutoPaymentMatching(Parser.TransferDocuments);
			var org = OrganizationRepository.GetMainOrganization(UoW, orgProvider.GetMainOrganization());
			var defaultProfitCategory = UoW.GetById<CategoryProfit>(profitCategoryProvider.GetDefaultProfitCategory());
			var payments = Parser.TransferDocuments.Where(x => x.RecipientInn == org.INN).ToList();

			var duplicates = UoW.GetAll<Payment>().ToList();

			progress = 1d / payments.Count;

			foreach(var payment in payments){

				if(PaymentsRepository.PaymentFromBankClientExists(UoW,
															   	payment.Date.Year,
															   	int.Parse(payment.DocNum),
															   	payment.PayerInn,
															   	payment.PayerCurrentAccount)) {

					UpdateProgress?.Invoke(string.Empty, progress);
					countDuplicates++;
					continue;
				}

				var counterparty = CounterpartyRepository.GetCounterpartyByINN(UoW, payment.PayerInn);

				var curPayment = new Payment(payment.DocNum, payment.Date, payment.Total, payment.PayerName, payment.PayerInn,
					payment.PayerKpp, payment.PayerBank, payment.PayerAccount, payment.PayerCurrentAccount,
					payment.PayerCorrespondentAccount, payment.PayerBik, payment.PaymentPurpose, payment.RecipientCurrentAccount,
					org, counterparty);

				if(!autoPaymentMatching.IncomePaymentMatch(curPayment))
					curPayment.Status = PaymentState.undistributed;
				else {
					curPayment.Status = PaymentState.distributed;
				}

				count++;
				curPayment.ProfitCategory = defaultProfitCategory;

				list.Add(curPayment);
				UoW.Save(curPayment);

				if(curPayment.Status == PaymentState.distributed)
					CreateOperations(curPayment);

				logger.Debug($"Добавлен платеж {curPayment.PaymentNum}");
				UpdateProgress?.Invoke($"Обработан платеж {count} из {payments.Count}", progress);
			}

			UpdateProgress?.Invoke($"Обработка файла выгрузки завершена. Не загружено дублей: {countDuplicates}", progress);

			ObservablePayments = new GenericObservableList<Payment>(list.OrderBy(p => p.Status)
																	.ThenBy(p => p.CounterpartyName)
																	.ThenBy(p => p.Total)
																	.ToList());
			
			IsNotAutoMatchingMode = true;
		}

		private void CreateOperations(Payment payment)
		{
			CreateIncomeOperation(payment);
			CreateExpenseOperation(payment);
		}

		private void CreateIncomeOperation(Payment payment)
		{
			var operationIncome = new CashlessIncomeOperation {
				Payment = payment,
				OperationTime = DateTime.Now,
				Sum = payment.Total,
				Counterparty = payment.Counterparty
			};

			UoW.Save(operationIncome);
		}

		private void CreateExpenseOperation(Payment payment)
		{
			foreach(var order in payment.Orders) 
			{
				var operationExpense = new CashlessExpenseOperation {
					Order = order,
					Payment = payment,
					OperationTime = DateTime.Now,
					Sum = order.ActualTotalSum,
					Counterparty = payment.Counterparty
				};

				UoW.Save(operationExpense);
			}
		}
	}
}
