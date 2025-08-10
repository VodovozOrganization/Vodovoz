using Autofac;
using ExportTo1c.Library;
using ExportTo1c.Library.ExportNodes;
using Microsoft.VisualBasic.FileIO;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Reports.Sales.RetailSalesReportFor1C;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.ViewModels.ViewModels.Service
{
	public class ExportTo1CViewModel : DialogTabViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IGenericRepository<Organization> _organizationRepository;
		private readonly IGenericRepository<CashPaymentTypeOrganizationSettings> _cashPaymentTypeOrganizationSettingsRepository;
		private readonly IOrganizationSettings _organizationSettings;
		private Organization _selectedCashlessOrganization;
		private Organization _selectedRetailOrganization;
		private DateTime? _endDate;
		private DateTime? _startDate;
		private string _totalCounterparty;
		private string _totalNomenclature;
		private string _totalSales;
		private string _totalSum;
		private string _totalInvoices;
		private bool _exportInProgress;
		private IList<OrganizationVersion> _cashPaymentTypeOrganizationVersions;
		private Organization _cashPaymentTypeOrganization;
		private string _cashPaymentTypeOrganizationPhone;
		private CancellationTokenSource _cancellationTokenSource;

		public ExportTo1CViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IGtkTabsOpener gtkTabsOpener,
			IDialogSettingsFactory dialogSettingsFactory,
			IGenericRepository<Organization> organizationRepository,
			IGenericRepository<CashPaymentTypeOrganizationSettings> cashPaymentTypeOrganizationSettingsRepository,
			IOrganizationSettings organizationSettings)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_dialogSettingsFactory = dialogSettingsFactory ?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_cashPaymentTypeOrganizationSettingsRepository = cashPaymentTypeOrganizationSettingsRepository ??
															 throw new ArgumentNullException(nameof(cashPaymentTypeOrganizationSettingsRepository));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			TabName = "Выгрузка в 1с 8.3";

			CreateCommands();

			LoadEntities();

			_cancellationTokenSource = new CancellationTokenSource();

		}

		private void LoadEntities()
		{
			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Инициализация формы эскпорта в 1С"))
			{
				CashlessOrganizations = _organizationRepository.Get(unitOfWork).ToList();
				RetailOrganizations = _organizationRepository.Get(unitOfWork).ToList();

				_cashPaymentTypeOrganizationVersions =
					_cashPaymentTypeOrganizationSettingsRepository.Get(unitOfWork).FirstOrDefault()
						?.Organizations
						?.FirstOrDefault()
						?.OrganizationVersions;

				_cashPaymentTypeOrganization = _cashPaymentTypeOrganizationVersions
					.Select(o => o.Organization)
					.FirstOrDefault();

				_cashPaymentTypeOrganizationPhone = _cashPaymentTypeOrganization.Phones.FirstOrDefault().ToString();
			}
		}

		public Organization SelectedCashlessOrganization
		{
			get => _selectedCashlessOrganization;
			set => SetField(ref _selectedCashlessOrganization, value);
		}

		public Organization SelectedRetailOrganization
		{
			get => _selectedRetailOrganization;
			set => SetField(ref _selectedRetailOrganization, value);
		}

		[PropertyChangedAlso(nameof(CanExport))]
		public bool ExportInProgress
		{
			get => _exportInProgress;
			set => SetField(ref _exportInProgress, value);
		}


		[PropertyChangedAlso(nameof(CanExport))]
		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[PropertyChangedAlso(nameof(CanExport))]
		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public string TotalCounterparty
		{
			get => _totalCounterparty;
			set => SetField(ref _totalCounterparty, value);
		}

		public string TotalNomenclature
		{
			get => _totalNomenclature;
			set => SetField(ref _totalNomenclature, value);
		}

		public string TotalSales
		{
			get => _totalSales;
			set => SetField(ref _totalSales, value);
		}

		public string TotalSum
		{
			get => _totalSum;
			set => SetField(ref _totalSum, value);
		}

		public string TotalInvoices
		{
			get => _totalInvoices;
			set => SetField(ref _totalInvoices, value);
		}

		public bool CanSaveCashless => ExportCashlessData?.Errors?.Count == 0;

		public bool CanExport =>
			!ExportInProgress
			&& EndDate != null
			&& StartDate != null
			&& StartDate <= EndDate;

		public List<Organization> RetailOrganizations { get; set; }

		public IList<Organization> CashlessOrganizations { get; set; }

		public Action ExportCompleteAction { get; set; }

		public Action StartProgressAction { get; set; }

		public Action EndProgressAction { get; set; }

		public ExportData ExportCashlessData { get; set; }

		public IProgressBarDisplayable ProgressBarDisplayable { get; set; }

		#region Commands

		public DelegateCommand ExportCashlessBookkeepingCommand { get; private set; }
		public DelegateCommand ExportCashlessComplexAutomationCommand { get; private set; }
		public DelegateCommand ExportCashlessIPTinkoffCommand { get; private set; }
		public DelegateCommand ExportCashlessBookkeepingNewCommand { get; private set; }
		public DelegateCommand SaveExportCashlessDataCommand { get; private set; }
		public DelegateCommand RetailReportCommand { get; private set; }
		public DelegateCommand ExportRetailCommand { get; set; }

		private void CreateCommands()
		{
			ExportCashlessComplexAutomationCommand = new DelegateCommand(ExportCashlessComplexAutomation, () => CanExport);
			ExportCashlessComplexAutomationCommand.CanExecuteChangedWith(this, x => CanExport);

			ExportCashlessBookkeepingCommand = new DelegateCommand(ExportCashlessBookkeeping, () => CanExport);
			ExportCashlessBookkeepingCommand.CanExecuteChangedWith(this, x => CanExport);

			ExportCashlessBookkeepingNewCommand = new DelegateCommand(ExportCashlessBookkeepingNew, () => CanExport);
			ExportCashlessBookkeepingNewCommand.CanExecuteChangedWith(this, x => CanExport);

			ExportCashlessIPTinkoffCommand = new DelegateCommand(ExportCashlessIPTinkoff, () => CanExport);
			ExportCashlessIPTinkoffCommand.CanExecuteChangedWith(this, x => CanExport);

			SaveExportCashlessDataCommand = new DelegateCommand(SaveExportCashlessData, () => CanSaveCashless);
			SaveExportCashlessDataCommand.CanExecuteChangedWith(this, x => CanSaveCashless);

			RetailReportCommand = new DelegateCommand(CreateAndSaveRetailReport, () => CanExport);
			RetailReportCommand.CanExecuteChangedWith(this, x => CanExport);

			ExportRetailCommand = new DelegateCommand(ExportRetail, () => CanExport);
			ExportRetailCommand.CanExecuteChangedWith(this, x => CanExport);
		}

		#endregion Commands

		#region Cashless

		private void ExportCashlessIPTinkoff()
		{
			ExportCashless(Export1cMode.IPForTinkoff);
		}

		private void SaveExportCashlessData()
		{
			var dateText = EndDate.Value.ToString("yyyy-MM-dd");
			var fileName = $"Выгрузка 1с на {dateText} (безнал).xml";

			SaveXmlToFile(ExportCashlessData.ToXml(), fileName);
		}

		private void ExportCashlessBookkeepingNew()
		{
			if(SelectedCashlessOrganization == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Для этой выгрузки необходимо выбрать организацию");

				return;
			}

			ExportCashless(Export1cMode.BuhgalteriaOOONew);
		}

		private void ExportCashlessBookkeeping()
		{
			ExportCashless(Export1cMode.BuhgalteriaOOO);
		}

		private void ExportCashlessComplexAutomation()
		{
			ExportCashless(Export1cMode.ComplexAutomation);
		}

		private void ExportCashless(Export1cMode mode)
		{
			StartProgressAction?.Invoke();

			int? organizationId = null;

			switch(mode)
			{
				case Export1cMode.BuhgalteriaOOONew:
					organizationId = (SelectedCashlessOrganization)?.Id;
					break;
				case Export1cMode.BuhgalteriaOOO:
				case Export1cMode.ComplexAutomation:
					organizationId = _organizationSettings.VodovozOrganizationId;
					break;
			}

			var exportOperation = new ExportCashlessTo1cOperation(
					  mode,
					  _orderSettings,
					  _orderRepository,
					  UnitOfWorkFactory,
					  StartDate.Value,
					  EndDate.Value,
					  organizationId
				  );

			ExportInProgress = true;

			exportOperation.Run(ProgressBarDisplayable, _cancellationTokenSource.Token);

			ExportCashlessData = exportOperation.Result;

			ExportInProgress = false;

			OnPropertyChanged(nameof(CanSaveCashless));

			TotalCounterparty = ExportCashlessData.Objects
				.OfType<CatalogObjectNode>()
				.Count(node => node.Type == Common1cTypes.ReferenceCounterparty)
				.ToString();

			TotalNomenclature = ExportCashlessData.Objects
				.OfType<CatalogObjectNode>()
				.Count(node => node.Type == Common1cTypes.ReferenceNomenclature)
				.ToString();

			TotalSales = (ExportCashlessData.Objects
							  .OfType<SalesDocumentNode>()
							  .Count()
						  + ExportCashlessData.Objects.OfType<RetailDocumentNode>().Count())
				.ToString();

			TotalSum = ExportCashlessData.OrdersTotalSum.ToString("C", CultureInfo.GetCultureInfo("ru-RU"));

			TotalInvoices = ExportCashlessData.Objects
				.OfType<InvoiceDocumentNode>()
				.Count()
				.ToString();

			_interactiveService.ShowMessage(ImportanceLevel.Info, "Выгрузка безнала завершена");

			ExportCompleteAction?.Invoke();
			EndProgressAction?.Invoke();
		}

		#endregion Cashless

		#region Retail

		private RetailSalesReportFor1C CreateRetailReport(
			Organization filterOrganization,
			OrganizationVersion supplierOrganizationVersionOnDate,
			Organization supplierOrganizationOnDate,
			CancellationToken token)
		{
			var retailSalesReport = new RetailSalesReportFor1C
			{
				OrganizationVersionForTitle = supplierOrganizationVersionOnDate,
				OrganizationForTitle = supplierOrganizationOnDate,
				Phone = _cashPaymentTypeOrganizationPhone,
				SaleDate = StartDate.Value
			};

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Получение заказов розницы для отчёта"))
			{
				var orders = GetRetailOrders(unitOfWork, filterOrganization);
				retailSalesReport.Generate(orders, ProgressBarDisplayable, token);
			}

			return retailSalesReport;
		}

		private void ExportRetail()
		{
			if(SelectedRetailOrganization == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Необходимо выбрать организацию для розницы");

				return;
			}

			ExportInProgress = true;

			XElement xml;

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Получение заказов розницы для экспорта в 1С"))
			{
				var orders = GetRetailOrders(unitOfWork, SelectedRetailOrganization);

				xml = Retail1cDataExporter.CreateRetailXmlAsync(orders, StartDate.Value, EndDate.Value, _cancellationTokenSource.Token, ProgressBarDisplayable);
			}

			var dateText = EndDate.Value.ToString("yyyy-MM-dd");
			var fileName = $"Выгрузка 1с на {dateText} (розница).xml";

			SaveXmlToFile(xml, fileName);

			_interactiveService.ShowMessage(ImportanceLevel.Info, "Экспорт розницы завершен");

			ExportInProgress = false;
		}

		private IList<Order> GetRetailOrders(IUnitOfWork unitOfWork, Organization organization)
		{
			return _orderRepository.GetOrdersToExport1c8(
				unitOfWork,
				_orderSettings,
				Export1cMode.RetailReport,
				StartDate.Value,
				EndDate.Value,
				organization.Id);
		}

		private void CreateAndSaveRetailReport()
		{
			StartProgressAction?.Invoke();

			if(SelectedRetailOrganization is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Необходимо выбрать организацию для розницы");

				return;
			}

			var supplierOrganizationVersionOnDate = _cashPaymentTypeOrganizationVersions
				.Where(x => x.StartDate <= StartDate)
				.OrderByDescending(x => x.StartDate)
				.FirstOrDefault();

			if(supplierOrganizationVersionOnDate == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, $"Отсутствует версия организации на дату {StartDate}, настроенная на нал");

				return;
			}

			ExportInProgress = true;

			var report = CreateRetailReport(SelectedRetailOrganization, supplierOrganizationVersionOnDate, _cashPaymentTypeOrganization, _cancellationTokenSource.Token);
			report.SaveReport(_dialogSettingsFactory, _fileDialogService);

			ExportInProgress = false;

			_interactiveService.ShowMessage(ImportanceLevel.Info, "Отчёт сохранён");
			EndProgressAction?.Invoke();
		}

		#endregion Retail

		#region Save xml to file

		private void SaveXmlToFile(XElement xml, string fileName)
		{
			var dialogSettings = CreateDialogSettings(fileName);

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!result.Successful)
			{
				return;
			}

			var filename = result.Path;

			var settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true,
				Encoding = Encoding.UTF8,
				NewLineChars = "\r\n"
			};

			using(var writer = XmlWriter.Create(filename, settings))
			{
				xml.WriteTo(writer);
			}
		}

		private DialogSettings CreateDialogSettings(string fileName)
		{
			var settings = new DialogSettings
			{
				Title = "Выберите файл для сохранения выгрузки",
				DefaultFileExtention = "*.xml",
				InitialDirectory = SpecialDirectories.Desktop,
				FileName = fileName
			};

			settings.FileFilters.Clear();
			settings.FileFilters.Add(new DialogFileFilter($"XML файлы", "*.xml"));

			return settings;
		}

		#endregion Save xml to file

		public override void Dispose()
		{
			_cancellationTokenSource.Cancel();

			base.Dispose();
		}
	}
}
