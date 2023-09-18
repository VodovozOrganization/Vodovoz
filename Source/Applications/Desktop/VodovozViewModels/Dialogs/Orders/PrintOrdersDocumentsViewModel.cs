using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.ViewModels.Infrastructure.Print;

namespace Vodovoz.ViewModels.Dialogs.Orders
{
	public class PrintOrdersDocumentsViewModel : DialogTabViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly IEntityDocumentsPrinterFactory _entityDocumentsPrinterFactory;
		private readonly List<Order> _orders;
		private readonly bool _isOrdersListValid;
		private const int _maxOrdersCount = 100;
		private bool _isPrintBill;
		private bool _isPrintUpd;
		private bool _isPrintInProcess;
		private string _printingDocumentInfo;
		private GenericObservableList<string> _warnings;

		public PrintOrdersDocumentsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICommonServices commonServices,
			IEntityDocumentsPrinterFactory entityDocumentsPrinterFactory,
			List<Order> orders
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_commonServices = commonServices ?? throw new System.ArgumentNullException(nameof(commonServices));
			_entityDocumentsPrinterFactory = entityDocumentsPrinterFactory ?? throw new System.ArgumentNullException(nameof(entityDocumentsPrinterFactory));
			_orders = orders ?? new List<Order>();
			_warnings = new GenericObservableList<string>();

			Title = "Печать документов заказа";

			_isOrdersListValid = IsOrdersListValidCheck();
		}

		#region Properties

		[PropertyChangedAlso(nameof(CanPrintDocuments))]
		public bool IsPrintBill
		{
			get => _isPrintBill;
			set => SetField(ref _isPrintBill, value);
		}

		[PropertyChangedAlso(nameof(CanPrintDocuments))]
		public bool IsPrintUpd
		{
			get => _isPrintUpd;
			set => SetField(ref _isPrintUpd, value);
		}

		[PropertyChangedAlso(nameof(CanPrintDocuments))]
		public bool IsPrintInProcess
		{
			get => _isPrintInProcess;
			set => SetField(ref _isPrintInProcess, value);
		}

		public string PrintingDocumentInfo
		{
			get => _printingDocumentInfo;
			set => SetField(ref _printingDocumentInfo, value);
		}

		public bool CanPrintDocuments =>
			_isOrdersListValid
			&& !IsPrintInProcess
			&& (IsPrintBill || IsPrintUpd);

		public int OrdersClientsCount =>
			Orders
			.Select(o => o.Client.Id)
			.Distinct()
			.Count();

		public List<Order> Orders => _orders;

		public GenericObservableList<string> Warnings => _warnings;

		#endregion Properties

		private bool IsOrdersListValidCheck()
		{
			if(Orders.Count < 1)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Заказы для печати не выбраны. Печать документов недоступна");

				return false;
			}

			if(OrdersClientsCount != 1)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Выбраны заказы нескольких контрагентов. Печать документов недоступна");

				return false;
			}

			if(Orders.Count > _maxOrdersCount)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					$"Выбрано более {_maxOrdersCount} заказов. Печать документов недоступна.");

				return false;
			}

			return true;
		}

		private List<OrderDocumentType> GetSelectedOrderDocumentTypes()
		{
			var selectedTypes = new List<OrderDocumentType>();

			if(IsPrintBill)
			{
				selectedTypes.Add(OrderDocumentType.Bill);
			}

			if(IsPrintUpd)
			{
				selectedTypes.Add(OrderDocumentType.UPD);
			}

			return selectedTypes;
		}

		#region Commands

		#region PrintCommand
		private DelegateCommand _printCommand;
		public DelegateCommand PrintCommand
		{
			get
			{
				if(_printCommand == null)
				{
					_printCommand = new DelegateCommand(Print, () => CanPrint);
					_printCommand.CanExecuteChangedWith(this, x => x.CanPrint);
				}
				return _printCommand;
			}
		}

		public bool CanPrint => CanPrintDocuments;

		private void Print()
		{
			if(!CanPrintDocuments)
			{
				return;
			}

			var selectedDocumentTypes = GetSelectedOrderDocumentTypes();

			IsPrintInProcess = true;

			foreach(var order in Orders)
			{
				bool cancelPrinting = false;

				PrintingDocumentInfo = $"Печать докуметов заказа №{order.Id}";

				var printer = _entityDocumentsPrinterFactory.CreateOrderDocumentsPrinter(
					order,
					false,
					selectedDocumentTypes);

				printer.PrintingCanceled += (s, ea) =>
				{
					cancelPrinting = true;
				};

				printer.Print();

				if(!string.IsNullOrEmpty(printer.ODTTemplateNotFoundMessages))
				{
					_warnings.Add($"Заказ {order.Id}");
					_warnings.Add(printer.ODTTemplateNotFoundMessages);
				}

				if(cancelPrinting)
				{
					PrintingDocumentInfo = "Печать отменена";
					break;
				}
			}

			PrintingDocumentInfo = $"Выполнено!";
			IsPrintInProcess = false;
		}
		#endregion PrintCommand

		#region CloseDialogCommand
		private DelegateCommand _closeDialogCommand;
		public DelegateCommand CloseDialogCommand
		{
			get
			{
				if(_closeDialogCommand == null)
				{
					_closeDialogCommand = new DelegateCommand(CloseDialog, () => CanCloseDialog);
					_closeDialogCommand.CanExecuteChangedWith(this, x => x.CanCloseDialog);
				}
				return _closeDialogCommand;
			}
		}

		public bool CanCloseDialog => true;

		private void CloseDialog()
		{
			Close(false, CloseSource.Cancel);
		}
		#endregion CloseDialogCommand

		#endregion Commands
	}
}
