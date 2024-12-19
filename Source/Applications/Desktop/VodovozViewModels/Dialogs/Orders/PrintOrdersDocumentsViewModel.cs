using MoreLinq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.ViewModels.Infrastructure.Print;
using Vodovoz.ViewModels.TempAdapters;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Dialogs.Orders
{
	public partial class PrintOrdersDocumentsViewModel : DialogTabViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly IEntityDocumentsPrinterFactory _entityDocumentsPrinterFactory;
		private readonly IReportExporter _reportExporter;
		private readonly IFileChooserProvider _fileChooserProvider;
		private readonly IList<Order> _orders;

		private readonly bool _isOrdersListValid;

		private const int _maxOrdersCount = 100;

		private bool _isPrintBill;
		private bool _isPrintUpd;
		private bool _isPrintSpecialBill;
		private bool _isPrintSpecialUpd;

		private bool _isPrintBillWithSignatureAndStamp;
		private bool _isPrintUpdWithSignatureAndStamp;
		private bool _isPrintSpecialBillWithSignatureAndStamp;
		private bool _isPrintSpecialUpdWithSignatureAndStamp;

		private bool _isPrintInProcess;
		private bool _isShowWarnings;
		private string _printingDocumentInfo;
		private int _printCopiesCount;
		private int _ordersPrintedCount;

		private int _selectedToPrintCount;

		public PrintOrdersDocumentsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICommonServices commonServices,
			IEntityDocumentsPrinterFactory entityDocumentsPrinterFactory,
			IReportExporter reportExporter,
			IFileChooserProvider fileChooserProvider,
			IList<Order> orders)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_entityDocumentsPrinterFactory = entityDocumentsPrinterFactory ?? throw new ArgumentNullException(nameof(entityDocumentsPrinterFactory));
			_reportExporter = reportExporter;
			_fileChooserProvider = fileChooserProvider ?? throw new ArgumentNullException(nameof(fileChooserProvider));
			_orders = orders ?? new List<Order>();

			OrdersClientsCount = _orders
				.Select(o => o.Client.Id)
				.Distinct()
				.Count();

			foreach(var order in _orders)
			{
				OrdersToPrint.Add(new OrdersToPrintNode
				{
					Id = order.Id,
					DeliveryDate = order.DeliveryDate,
					Selected = true
				});
			}

			OrdersToPrint.ElementChanged += (s, e) => OnPropertyChanged(nameof(CanPrintOrSaveDocuments));

			_isOrdersListValid = IsOrdersListValidCheck();

			Title = _isOrdersListValid
				? $"Печать документов заказов клиента \"{GetCounterpartyName()}\""
				: "Печать документов заказов";

			_printCopiesCount = 1;

			_isPrintBill = true;
			_isPrintSpecialBill = true;
			_isPrintUpd = true;
			_isPrintSpecialUpd = true;

			PrintCommand = CreatePrintCommand();
			SaveCommand = CreateSaveCommand();
			CloseDialogCommand = CreateCloseDialogCommand();
			SelectAllOrdersCommand = CreateSelectAllOrdersCommand();
			DeselectAllOrdersCommand = CreateDeselectAllOrdersCommand();
		}

		#region Properties

		[PropertyChangedAlso(nameof(CanPrintOrSaveDocuments))]
		public bool IsPrintBill
		{
			get => _isPrintBill;
			set => SetField(ref _isPrintBill, value);
		}

		[PropertyChangedAlso(nameof(CanPrintOrSaveDocuments))]
		public bool IsPrintUpd
		{
			get => _isPrintUpd;
			set => SetField(ref _isPrintUpd, value);
		}

		[PropertyChangedAlso(nameof(CanPrintOrSaveDocuments))]
		public bool IsPrintSpecialBill
		{
			get => _isPrintSpecialBill;
			set => SetField(ref _isPrintSpecialBill, value);
		}

		[PropertyChangedAlso(nameof(CanPrintOrSaveDocuments))]
		public bool IsPrintSpecialUpd
		{
			get => _isPrintSpecialUpd;
			set => SetField(ref _isPrintSpecialUpd, value);
		}

		public bool IsPrintBillWithSignatureAndStamp
		{
			get => _isPrintBillWithSignatureAndStamp;
			set => SetField(ref _isPrintBillWithSignatureAndStamp, value);
		}

		public bool IsPrintUpdWithSignatureAndStamp
		{
			get => _isPrintUpdWithSignatureAndStamp;
			set => SetField(ref _isPrintUpdWithSignatureAndStamp, value);
		}

		public bool IsPrintSpecialBillWithSignatureAndStamp
		{
			get => _isPrintSpecialBillWithSignatureAndStamp;
			set => SetField(ref _isPrintSpecialBillWithSignatureAndStamp, value);
		}

		public bool IsPrintSpecialUpdWithSignatureAndStamp
		{
			get => _isPrintSpecialUpdWithSignatureAndStamp;
			set => SetField(ref _isPrintSpecialUpdWithSignatureAndStamp, value);
		}

		[PropertyChangedAlso(nameof(CanPrintOrSaveDocuments))]
		public bool IsPrintOrSaveInProcess
		{
			get => _isPrintInProcess;
			set => SetField(ref _isPrintInProcess, value);
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

		[PropertyChangedAlso(nameof(CanPrintOrSaveDocuments))]
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

		[PropertyChangedAlso(
			nameof(CanSave),
			nameof(CanPrint))]
		public bool CanPrintOrSaveDocuments =>
			_isOrdersListValid
			&& !IsPrintOrSaveInProcess
			&& PrintCopiesCount > 0
			&& (IsPrintBill || IsPrintUpd || IsPrintSpecialBill || IsPrintSpecialUpd)
			&& OrdersToPrint.Any(x => x.Selected);

		public int OrdersClientsCount { get; }

		public GenericObservableList<string> Warnings { get; } = new GenericObservableList<string>();

		public int SelectedToPrintCount
		{
			get => _selectedToPrintCount;
			set => SetField(ref _selectedToPrintCount, value);
		}

		public GenericObservableList<OrdersToPrintNode> OrdersToPrint { get; } = new GenericObservableList<OrdersToPrintNode>();

		#endregion Properties

		#region Commands

		#region PrintCommand

		public DelegateCommand PrintCommand { get; }

		public bool CanPrint => CanPrintOrSaveDocuments;

		private void Print()
		{
			if(!CanPrintOrSaveDocuments)
			{
				return;
			}

			OrdersPrintedCount = 0;
			PrintingDocumentInfo = "";
			Warnings.Clear();
			IsShowWarnings = false;

			var signaturesAndStampsOfDocument = GetSignaturesAndStampsOfDocument();

			var printingDocumentsCount = GetPrintingDocsCount();

			var ordersToPrintIds = OrdersToPrint
				.Where(x => x.Selected)
				.Select(x => x.Id)
				.ToList();

			int ordersToPrintCount = ordersToPrintIds.Count;

			SelectedToPrintCount = ordersToPrintCount;

			if(!_commonServices.InteractiveService.Question(
				$"Будет распечатано:\n" +
				$"{printingDocumentsCount} документов\n" +
				$"по {PrintCopiesCount} копий\n" +
				$"для {ordersToPrintCount} заказов.\n\n" +
				$"Продолжить?"))
			{
				return;
			}

			IsPrintOrSaveInProcess = true;

			var ordersToPrint = _orders
				.Where(order => ordersToPrintIds.Contains(order.Id))
				.ToList();

			try
			{
				foreach(var order in ordersToPrint)
				{
					bool cancelPrinting = false;

					PrintingDocumentInfo = $"Печать документов заказа №{order.Id}";

					var printer = _entityDocumentsPrinterFactory.CreateOrderDocumentsPrinter(
						order,
						signaturesAndStampsOfDocument);

					printer.MultiDocPrinterPrintableDocuments.ForEach(d => d.Copies = PrintCopiesCount);

					printer.PrintingCanceled += (s, ea) =>
					{
						cancelPrinting = true;
					};

					printer.Print();

					if(!string.IsNullOrEmpty(printer.ODTTemplateNotFoundMessages))
					{
						Warnings.Add($"Заказ {order.Id}");
						Warnings.Add(printer.ODTTemplateNotFoundMessages);
						IsShowWarnings = true;
					}

					PrintingDocumentInfo = $"Выполнено!";

					if(cancelPrinting)
					{
						PrintingDocumentInfo = "Печать отменена";
						break;
					}

					OrdersPrintedCount++;
				}
			}
			catch(Exception)
			{
				PrintingDocumentInfo = "Ошибка печати";
				throw;
			}
			finally
			{
				OrdersPrintedCount = ordersToPrintCount;
				IsPrintOrSaveInProcess = false;

				ordersToPrint.Clear();
			}
		}

		#endregion PrintCommand

		#region SaveCommand

		public DelegateCommand SaveCommand { get; }

		public bool CanSave => CanPrintOrSaveDocuments;

		private void SaveHandler()
		{
			if(!CanPrintOrSaveDocuments)
			{
				return;
			}

			OrdersPrintedCount = 0;
			PrintingDocumentInfo = "";
			Warnings.Clear();
			IsShowWarnings = false;

			var signaturesAndStampsOfDocument = GetSignaturesAndStampsOfDocument();

			var printingDocumentsCount = GetPrintingDocsCount();

			var ordersToPrintIds = OrdersToPrint
				.Where(x => x.Selected)
				.Select(x => x.Id)
				.ToList();

			int ordersToPrintCount = ordersToPrintIds.Count;

			SelectedToPrintCount = ordersToPrintCount;

			if(!_commonServices.InteractiveService.Question(
				$"Будет экспортировано:\n" +
				$"{printingDocumentsCount} документов\n" +
				$"для {ordersToPrintCount} заказов.\n\n" +
				$"Продолжить?"))
			{
				return;
			}

			var path = _fileChooserProvider.GetExportFolderPath();

			IsPrintOrSaveInProcess = true;

			var ordersToPrint = _orders
				.Where(order => ordersToPrintIds.Contains(order.Id))
				.ToList();

			try
			{
				foreach(var order in ordersToPrint)
				{
					bool cancelPrinting = false;

					PrintingDocumentInfo = $"Экспорт документов заказа №{order.Id}";

					var documentsToSave = order.OrderDocuments
						.Where(x => signaturesAndStampsOfDocument.Keys.Contains(x.Type))
						.ToList();

					foreach(var document in order.OrderDocuments)
					{
						if(signaturesAndStampsOfDocument.TryGetValue(document.Type, out bool showSignature)
							&& document is IPrintableRDLDocument printableRDLDocument)
						{
							_reportExporter.ExportReport(
								printableRDLDocument,
								$"{path}{Path.DirectorySeparatorChar}{document.Name}.pdf",
								!showSignature);
						}
					}

					PrintingDocumentInfo = $"Выполнено!";

					if(cancelPrinting)
					{
						PrintingDocumentInfo = "Экспорт отменен";
						break;
					}

					OrdersPrintedCount++;
				}
			}
			catch(Exception)
			{
				PrintingDocumentInfo = "Ошибка экспорта";
				throw;
			}
			finally
			{
				OrdersPrintedCount = ordersToPrintCount;
				IsPrintOrSaveInProcess = false;

				ordersToPrint.Clear();
			}
		}

		#endregion SaveCommand

		#region CloseDialogCommand

		public DelegateCommand CloseDialogCommand { get; }

		public bool CanCloseDialog => true;


		private void CloseDialog()
		{
			Close(false, CloseSource.Cancel);
		}

		#endregion CloseDialogCommand

		#region SelectAllOrdersCommand

		public DelegateCommand SelectAllOrdersCommand { get; }

		public bool CanSelectAllOrders => true;

		private void SelectAllOrders()
		{
			OrdersToPrint.ForEach(o => o.Selected = true);
		}

		#endregion SelectAllOrdersCommand

		#region DeselectAllOrdersCommand

		public DelegateCommand DeselectAllOrdersCommand { get; }

		public bool CanDeselectAllOrders => true;

		private void DeselectAllOrders()
		{
			OrdersToPrint.ForEach(o => o.Selected = false);
		}

		#endregion DeselectAllOrdersCommand

		private DelegateCommand CreateCloseDialogCommand()
		{
			var closeDialogCommand = new DelegateCommand(CloseDialog, () => CanCloseDialog);
			closeDialogCommand.CanExecuteChangedWith(this, x => x.CanCloseDialog);

			return closeDialogCommand;
		}

		private DelegateCommand CreatePrintCommand()
		{
			var printCommand = new DelegateCommand(Print, () => CanPrint);
			printCommand.CanExecuteChangedWith(this, x => x.CanPrint);

			return printCommand;
		}

		private DelegateCommand CreateSaveCommand()
		{
			var saveCommand = new DelegateCommand(SaveHandler, () => CanSave);
			saveCommand.CanExecuteChangedWith(this, x => x.CanSave);

			return saveCommand;
		}

		private DelegateCommand CreateSelectAllOrdersCommand()
		{
			var selectAllOrdersCommand = new DelegateCommand(SelectAllOrders, () => CanSelectAllOrders);
			selectAllOrdersCommand.CanExecuteChangedWith(this, x => x.CanSelectAllOrders);

			return selectAllOrdersCommand;
		}

		private DelegateCommand CreateDeselectAllOrdersCommand()
		{
			var deselectAllOrdersCommand = new DelegateCommand(DeselectAllOrders, () => CanDeselectAllOrders);
			deselectAllOrdersCommand.CanExecuteChangedWith(this, x => x.CanDeselectAllOrders);

			return deselectAllOrdersCommand;
		}

		#endregion Commands

		private bool IsOrdersListValidCheck()
		{
			if(!OrdersToPrint.Any(x => x.Selected))
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

			if(SelectedToPrintCount > _maxOrdersCount)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					$"Выбрано более {_maxOrdersCount} заказов. Печать документов недоступна.");

				return false;
			}

			return true;
		}

		private IDictionary<OrderDocumentType, bool> GetSignaturesAndStampsOfDocument()
		{
			var signaturesAndStampRequired = new Dictionary<OrderDocumentType, bool>();

			if(IsPrintBill)
			{
				signaturesAndStampRequired.Add(
					OrderDocumentType.Bill,
					IsPrintBillWithSignatureAndStamp);
			}

			if(IsPrintUpd)
			{
				signaturesAndStampRequired.Add(
					OrderDocumentType.UPD,
					IsPrintUpdWithSignatureAndStamp);
			}

			if(IsPrintSpecialBill)
			{
				signaturesAndStampRequired.Add(
					OrderDocumentType.SpecialBill,
					IsPrintSpecialBillWithSignatureAndStamp);
			}

			if(IsPrintSpecialUpd)
			{
				signaturesAndStampRequired.Add(
					OrderDocumentType.SpecialUPD,
					IsPrintSpecialUpdWithSignatureAndStamp);
			}

			return signaturesAndStampRequired;
		}

		private int GetPrintingDocsCount()
		{
			var ordersIds = OrdersToPrint.Where(x => x.Selected).Select(x => x.Id).ToList();

			var docsIds =
				UoW.Session.Query<BillDocument>()
				.Where(document => IsPrintBill && ordersIds.Contains(document.Order.Id))
				.Select(document => document.Id)
				.ToList()
				.Union(
					UoW.Session.Query<SpecialBillDocument>()
					.Where(document => IsPrintSpecialBill && ordersIds.Contains(document.Order.Id))
					.Select(document => document.Id)
					.ToList())
				.Union(
					UoW.Session.Query<UPDDocument>()
					.Where(document => IsPrintUpd && ordersIds.Contains(document.Order.Id))
					.Select(document => document.Id)
					.ToList())
				.Union(
					UoW.Session.Query<SpecialUPDDocument>()
					.Where(document => IsPrintSpecialUpd && ordersIds.Contains(document.Order.Id))
					.Select(document => document.Id)
					.ToList());

			var docsCount = docsIds.Count();

			return docsCount;
		}

		private string GetCounterpartyName() => _orders
			.FirstOrDefault()?
			.Client?.FullName ?? string.Empty;
	}
}
