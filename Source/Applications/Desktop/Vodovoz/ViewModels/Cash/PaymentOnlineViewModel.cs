using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Services;
using QS.Utilities.Extensions;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Presentation.ViewModels.Documents;
using Vodovoz.Services;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Orders;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Services.Orders;
using static OneOf.Types.TrueFalseOrNull;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Cash
{
	public class PaymentOnlineViewModel : EntityTabViewModelBase<Order>
	{
		private readonly Employee _currentEmployee;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IOrderContractUpdater _contractUpdater;
		private readonly IInteractiveService _interactiveService;

		public PaymentOnlineViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ICallTaskWorker callTaskWorker,
			IOrderPaymentSettings orderPaymentSettings,
			IOrderSettings orderSettings,
			IDeliveryRulesSettings deliveryRulesSettings,
			IOrderContractUpdater contractUpdater,
			IEmployeeService employeeService,
			IInteractiveService interactiveService) : base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(orderPaymentSettings == null)
			{
				throw new ArgumentNullException(nameof(orderPaymentSettings));
			}

			if(orderSettings == null)
			{
				throw new ArgumentNullException(nameof(orderSettings));
			}
			if(deliveryRulesSettings == null)
			{
				throw new ArgumentNullException(nameof(deliveryRulesSettings));
			}

			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));
			_currentEmployee =
				(employeeService ?? throw new ArgumentNullException(nameof(employeeService)))
				.GetEmployeeForCurrentUser(UoW);
			_interactiveService = interactiveService;

			TabName = "Онлайн оплата";

			ItemsList = UoW.GetAll<PaymentFrom>().Where(p => !p.IsArchive).ToList();

			if(PaymentOnlineFrom == null)
			{
				PaymentOnlineFrom = ItemsList.FirstOrDefault(p => p.Id == orderPaymentSettings.DefaultSelfDeliveryPaymentFromId);
			}

			Entity.PropertyChanged += Entity_PropertyChanged;

			ValidationContext.ServiceContainer.AddService(orderSettings);
			ValidationContext.ServiceContainer.AddService(deliveryRulesSettings);

			SaveCommand = new DelegateCommand(SaveHandler, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanSave);

			CloseCommand = new DelegateCommand(CloseHandler);
		}

		public PaymentFrom PaymentOnlineFrom
		{
			get => Entity.PaymentByCardFrom;
			set => Entity.UpdatePaymentByCardFrom(value, _contractUpdater);
		}

		public List<PaymentFrom> ItemsList { get; private set; }

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseCommand { get; }

		public bool CanSave => Entity.OnlinePaymentNumber != null;

		protected override bool BeforeSave()
		{
			bool isUnshippedSelfDeliveryWithPayAfterShipment = Entity.PayAfterShipment
				&& Entity.SelfDelivery
				&& Entity.OrderStatus != OrderStatus.OnLoading;

			if(!isUnshippedSelfDeliveryWithPayAfterShipment)
			{
				return true;
			}

			if(!_interactiveService.Question(
				"Данный заказ ещё не отгружен.\nПри принятии оплаты заказ будет закрыт и его невозможно будет отгрузить.\nПродолжить?",
				"Внимание"))
			{
				return false;
			}

			return true;
		}

		private void SaveHandler()
		{
			if(Entity.SelfDelivery)
			{
				if(!Save(false))
				{
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Не удалось сохранить документ");
					return;
				}

				var document = Entity.OrderDocuments
					.FirstOrDefault(x =>
						x.Type == OrderDocumentType.Invoice
						|| x.Type == OrderDocumentType.InvoiceBarter
						|| x.Type == OrderDocumentType.InvoiceContractDoc)
					as IPrintableRDLDocument;

				var page = NavigationManager
					.OpenViewModel<PrintableRdlDocumentViewModel<IPrintableRDLDocument>, IPrintableRDLDocument>(this, document, OpenPageOptions.AsSlave);

				page.PageClosed += OnInvoiceDocumentPrintViewClosed;
			}
			else
			{
				SaveAndClose();
			}
		}

		private void OnInvoiceDocumentPrintViewClosed(object sender, EventArgs eventArgs)
		{
			if(sender is IPage page)
			{
				page.PageClosed -= OnInvoiceDocumentPrintViewClosed;
			}

			Close(false, CloseSource.Save);
		}

		private void CloseHandler()
		{
			Close(true, CloseSource.Cancel);
		}

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.PaymentByCardFrom))
			{
				OnPropertyChanged(nameof(PaymentOnlineFrom));
			}

			if(e.PropertyName == nameof(Entity.OnlinePaymentNumber))
			{
				OnPropertyChanged(nameof(CanSave));
			}
		}

		protected override bool BeforeValidation()
		{
			Entity.ChangePaymentTypeToOnline(_callTaskWorker);

			if(!Entity.PayAfterShipment)
			{
				Entity.SelfDeliveryToLoading(_currentEmployee, CommonServices.CurrentPermissionService, _callTaskWorker);
			}

			if(Entity.SelfDelivery)
			{
				Entity.IsSelfDeliveryPaid = true;
			}

			return true;
		}
	}
}

