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
using System.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.ViewModels.Documents;
using Vodovoz.Services;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Orders;
using Vodovoz.Tools.CallTasks;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Cash
{
	public class PaymentByCardViewModel : EntityTabViewModelBase<Order>
	{
		private readonly Employee _currentEmployee;
		private readonly ICallTaskWorker _callTaskWorker;

		public PaymentByCardViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ICallTaskWorker callTaskWorker,
			IOrderPaymentSettings orderPaymentSettings,
			IOrderSettings orderSettings,
			IDeliveryRulesSettings deliveryRulesSettings,
			IEmployeeService employeeService) : base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
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

			if(employeeService is null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}

			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_currentEmployee = employeeService.GetEmployeeForCurrentUser();

			TabName = "Оплата по карте";

			Entity.PaymentType = PaymentType.Terminal;

			Entity.PropertyChanged += Entity_PropertyChanged;

			ValidationContext.ServiceContainer.AddService(orderSettings);
			ValidationContext.ServiceContainer.AddService(deliveryRulesSettings);

			SaveCommand = new DelegateCommand(SaveHandler, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanSave);

			CloseCommand = new DelegateCommand(CloseHandler);
		}

		public PaymentType PaymentType
		{
			get => Entity.PaymentType;
			set => Entity.PaymentType = value;
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseCommand { get; }

		public bool CanSave => Entity.OnlineOrder != null;

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
			if(e.PropertyName == nameof(Entity.PaymentType))
			{
				OnPropertyChanged(nameof(PaymentType));
			}

			if(e.PropertyName == nameof(Entity.OnlineOrder))
			{
				OnPropertyChanged(nameof(CanSave));
			}
		}

		protected override bool BeforeValidation()
		{
			Entity.ChangePaymentTypeToByCardTerminal(_callTaskWorker);

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
