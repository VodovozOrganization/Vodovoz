using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
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
		private readonly IList<Order> _orders;
		private readonly bool _isOrdersListValid;
		private const int _maxOrdersCount = 100;
		private bool _isPrintBill;
		private bool _isPrintUpd;
		private bool _isPrintSpecialBill;
		private bool _isPrintSpecialUpd;
		private bool _isPrintInProcess;
		private bool _isShowWarnings;
		private string _printingDocumentInfo;
		private int _printCopiesCount;
		private int _ordersPrintedCount;
		private GenericObservableList<string> _warnings;

		public PrintOrdersDocumentsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICommonServices commonServices,
			IEntityDocumentsPrinterFactory entityDocumentsPrinterFactory,
			IList<Order> orders
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_commonServices = commonServices ?? throw new System.ArgumentNullException(nameof(commonServices));
			_entityDocumentsPrinterFactory = entityDocumentsPrinterFactory ?? throw new System.ArgumentNullException(nameof(entityDocumentsPrinterFactory));
			_orders = orders ?? new List<Order>();
			_warnings = new GenericObservableList<string>();

			_isOrdersListValid = IsOrdersListValidCheck();

			Title = _isOrdersListValid
				? $"Печать документов заказов клиента \"{GetCounterpartyName()}\""
				: "Печать документов заказов";

			_printCopiesCount = 1;
			_isPrintBill = true;
			_isPrintSpecialBill = true;
			_isPrintUpd = true;
			_isPrintSpecialUpd = true;
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

		[PropertyChangedAlso(nameof(CanPrintDocuments))]
		public bool IsPrintSpecialBill
		{
			get => _isPrintSpecialBill;
			set => SetField(ref _isPrintSpecialBill, value);
		}

		[PropertyChangedAlso(nameof(CanPrintDocuments))]
		public bool IsPrintSpecialUpd
		{
			get => _isPrintSpecialUpd;
			set => SetField(ref _isPrintSpecialUpd, value);
		}

		public bool IsShowWarnings
		{
			get => _isShowWarnings;
			set => SetField(ref _isShowWarnings, value);
		}

		public string PrintingDocumentInfo
		{
			get => _printingDocumentInfo;
			set => SetField(ref _printingDocumentInfo, value);
		}

		public int PrintCopiesCount
		{
			get => _printCopiesCount;
			set => SetField(ref _printCopiesCount, value);
		}

		public int OrdersPrintedCount
		{
			get => _ordersPrintedCount;
			set => SetField(ref _ordersPrintedCount, value);
		}

		public bool CanPrintDocuments =>
			_isOrdersListValid
			&& !IsPrintInProcess
			&& PrintCopiesCount > 0
			&& (IsPrintBill || IsPrintUpd || IsPrintSpecialBill || IsPrintSpecialUpd);

		public int OrdersClientsCount =>
			Orders
			.Select(o => o.Client.Id)
			.Distinct()
			.Count();

		public IList<Order> Orders => _orders;

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

			if(IsPrintSpecialBill)
			{
				selectedTypes.Add(OrderDocumentType.SpecialBill);
			}

			if(IsPrintSpecialUpd)
			{
				selectedTypes.Add(OrderDocumentType.SpecialUPD);
			}

			return selectedTypes;
		}

		private int GetPrintingDocsCount()
		{
			var selectedTypes = GetSelectedOrderDocumentTypes();

			var docsIds =
				UoW.GetAll<BillDocument>()
				.Where(d => IsPrintBill && Orders.Contains(d.AttachedToOrder))
				.Select(d => d.Id)
				.ToList()
				.Union(
					UoW.GetAll<SpecialBillDocument>()
					.Where(d => IsPrintSpecialBill && Orders.Contains(d.AttachedToOrder))
					.Select(d => d.Id)
					.ToList())
				.Union(
					UoW.GetAll<UPDDocument>()
					.Where(d => IsPrintUpd && Orders.Contains(d.AttachedToOrder))
					.Select(d => d.Id)
					.ToList())
				.Union(
					UoW.GetAll<SpecialUPDDocument>()
					.Where(d => IsPrintSpecialUpd && Orders.Contains(d.AttachedToOrder))
					.Select(d => d.Id)
					.ToList());

			var docsCount = docsIds.Count();

			return docsCount;
		}

		private string GetCounterpartyName()
		{
			var counterpartyName = Orders
				.FirstOrDefault()?
				.Client?
				.FullName ?? string.Empty;

			return counterpartyName;
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

			OrdersPrintedCount = 0;
			PrintingDocumentInfo = "";
			_warnings.Clear();
			IsShowWarnings = false;

			var selectedDocumentTypes = GetSelectedOrderDocumentTypes();
			var printingDocumentsCount = GetPrintingDocsCount();

			if(!_commonServices.InteractiveService.Question(
				$"Будет распечатано:\n" +
				$"{printingDocumentsCount} документов\n" +
				$"по {PrintCopiesCount} копий\n" +
				$"для {Orders.Count} заказов.\n\n" +
				$"Продолжить?"))
			{
				return;
			}

			IsPrintInProcess = true;

			try
			{
				foreach(var order in Orders)
				{
					OrdersPrintedCount++;

					bool cancelPrinting = false;

					PrintingDocumentInfo = $"Печать документов заказа №{order.Id}";

					var printer = _entityDocumentsPrinterFactory.CreateOrderDocumentsPrinter(
						order,
						false,
						selectedDocumentTypes);

					printer.DocumentsToPrint.ForEach(d => d.Copies = PrintCopiesCount);

					printer.PrintingCanceled += (s, ea) =>
					{
						cancelPrinting = true;
					};

					printer.Print();

					if(!string.IsNullOrEmpty(printer.ODTTemplateNotFoundMessages))
					{
						_warnings.Add($"Заказ {order.Id}");
						_warnings.Add(printer.ODTTemplateNotFoundMessages);
						IsShowWarnings = true;
					}

					PrintingDocumentInfo = $"Выполнено!";

					if(cancelPrinting)
					{
						PrintingDocumentInfo = "Печать отменена";
						break;
					}
				}
			}
			catch(Exception ex)
			{
				PrintingDocumentInfo = "Ошибка печати";
				throw ex;
			}
			finally
			{
				OrdersPrintedCount = Orders.Count;
				IsPrintInProcess = false;
			}	
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
