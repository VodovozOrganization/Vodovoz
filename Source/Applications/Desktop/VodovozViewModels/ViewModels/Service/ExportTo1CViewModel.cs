using ExportTo1c.Library;
using ExportTo1c.Library.ExportNodes;
using ExportTo1c.Library.Factories;
using Microsoft.VisualBasic.FileIO;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Extensions;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using Vodovoz.ViewModels.ViewModels.Reports.Sales.RetailSalesReportFor1c;

namespace Vodovoz.ViewModels.ViewModels.Service
{
	public class ExportTo1CViewModel : DialogViewModelBase, IDisposable
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IGenericRepository<Organization> _organizationRepository;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IDataExporterFor1cFactory _dataExporterFor1CFactory;
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
		private readonly CancellationTokenSource _cancellationTokenSource;

		private readonly List<RetailSalesReportFor1c.OrganizationSettings> _retailSalesReportFor1cOrganizationSettings =
			new List<RetailSalesReportFor1c.OrganizationSettings>();

		public ExportTo1CViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IGenericRepository<Organization> organizationRepository,
			IOrganizationSettings organizationSettings,
			IDataExporterFor1cFactory dataExporterFor1CFactory)
			: base(navigation)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_dataExporterFor1CFactory = dataExporterFor1CFactory ?? throw new ArgumentNullException(nameof(dataExporterFor1CFactory));
			Title = "Выгрузка в 1с 8.3";

			CreateCommands();

			LoadEntities();

			_cancellationTokenSource = new CancellationTokenSource();
		}

		private void LoadEntities()
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Инициализация формы эскпорта в 1С"))
			{
				CashlessOrganizations = _organizationRepository.Get(unitOfWork).ToList();
				RetailOrganizations = _organizationRepository.Get(unitOfWork).ToList();

				foreach(var organization in RetailOrganizations)
				{
					_retailSalesReportFor1cOrganizationSettings.Add(new RetailSalesReportFor1c.OrganizationSettings(organization));
				}
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
					organizationId = _organizationSettings.VodovozOrganizationId;
					break;
			}

			var exportOperation = new ExportCashlessTo1cOperation(
				mode,
				_orderSettings,
				_orderRepository,
				_unitOfWorkFactory,
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

		private IEnumerable<RetailSalesReportFor1c> CreateRetailReports(CancellationToken cancellationToken)
		{
			var reports = new List<RetailSalesReportFor1c>();

			if(SelectedRetailOrganization is null)
			{
				reports.AddRange(RetailOrganizations.Select(organization => CreateRetailReportForOrganization(organization, cancellationToken)));
			}
			else
			{
				reports.Add(CreateRetailReportForOrganization(SelectedRetailOrganization, cancellationToken));
			}

			return reports;
		}

		private RetailSalesReportFor1c CreateRetailReportForOrganization(Organization organization, CancellationToken cancellationToken)
		{
			var export1CRetailOrganization = _retailSalesReportFor1cOrganizationSettings
				.FirstOrDefault(x => x.Organization.Id == organization.Id);

			var organizationVersion =
				export1CRetailOrganization
					?.OrganizationVersions
					?.OrderByDescending(x => x.StartDate)
					.FirstOrDefault(x => x.StartDate <= StartDate);

			if(organizationVersion is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не найдена версия организации на выбранные даты");

				return new RetailSalesReportFor1c();
			}

			var phone = export1CRetailOrganization.Phone;

			var retailSalesReport = new RetailSalesReportFor1c
			{
				OrganizationVersionForTitle = organizationVersion,
				OrganizationForTitle = organization,
				Phone = phone,
				SaleDate = StartDate.Value
			};

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Получение заказов розницы для отчёта"))
			{
				var orders = GetOrdersByModeFor1cExport(unitOfWork, organization.Id, Export1cMode.Retail);

				retailSalesReport.Generate(orders, ProgressBarDisplayable, cancellationToken);
			}

			return retailSalesReport;
		}

		private void CreateAndSaveRetailReport()
		{
			StartProgressAction?.Invoke();

			ExportInProgress = true;

			var reports = CreateRetailReports(_cancellationTokenSource.Token);

			foreach(var report in reports)
			{
				report.SaveReport(_fileDialogService, report.OrganizationForTitle.INN);
			}

			ExportInProgress = false;

			_interactiveService.ShowMessage(ImportanceLevel.Info, "Отчёт сохранён");

			EndProgressAction?.Invoke();
		}

		private void ExportRetail()
		{
			ExportInProgress = true;

			if(SelectedRetailOrganization is null)
			{
				foreach(var organization in RetailOrganizations)
				{
					ExportData(Export1cMode.Retail, organization);
				}
			}
			else
			{
				ExportData(Export1cMode.Retail, SelectedRetailOrganization);
			}

			_interactiveService.ShowMessage(ImportanceLevel.Info, "Экспорт розницы завершен");

			ExportInProgress = false;
		}

		#endregion Retail


		private void ExportCashlessComplexAutomation()
		{
			ExportInProgress = true;

			if(SelectedCashlessOrganization is null)
			{
				foreach(var organization in CashlessOrganizations)
				{
					ExportData(Export1cMode.ComplexAutomation, organization);
				}
			}
			else
			{
				ExportData(Export1cMode.ComplexAutomation, SelectedCashlessOrganization);
			}

			_interactiveService.ShowMessage(ImportanceLevel.Info, "Экспорт безнала завершен");

			ExportInProgress = false;
		}

		private IList<Order> GetOrdersByModeFor1cExport(IUnitOfWork unitOfWork, int organizationId, Export1cMode export1CMode)
		{
			return _orderRepository.GetOrdersToExport1c8(
				unitOfWork,
				_orderSettings,
				export1CMode,
				StartDate.Value,
				EndDate.Value,
				organizationId);
		}

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

		private XElement CreateXml(Export1cMode export1CMode, IList<Order> orders, Organization organization)
		{
			var exporter = _dataExporterFor1CFactory.Create1cDataExporter(export1CMode);

			var xml = exporter.CreateXml
			(
				orders,
				StartDate.Value,
				EndDate.Value,
				organization,
				_cancellationTokenSource.Token,
				ProgressBarDisplayable
			);

			return xml;
		}

		private void ExportData(Export1cMode exportMode, Organization organization)
		{
			IList<Order> orders;
			XElement xml;

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot($"Получение заказов для экспорта в 1С ({exportMode.GetEnumDisplayName()})"))
			{
				orders = GetOrdersByModeFor1cExport(unitOfWork, organization.Id, exportMode);

				if(!orders.Any())
				{
					return;
				}

				xml = CreateXml(exportMode, orders, organization);
			}

			SaveExportFile(xml, organization.INN, exportMode);

			ProgressBarDisplayable.Close();
		}

		private void SaveExportFile(XElement xml, string organizationInn, Export1cMode export1cMode)
		{
			var startDate = StartDate.Value.ToString("yyyy-MM-dd");
			var endDate = EndDate.Value.ToString("yyyy-MM-dd");

			var fileName = $"Выгрузка 1с c {startDate} по {endDate} ИНН {organizationInn} ({export1cMode.GetEnumDisplayName()}).xml";

			SaveXmlToFile(xml, fileName);
		}

		#endregion Save xml to file

		public void Dispose()
		{
			if(_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource.Cancel();
			}

			_cancellationTokenSource?.Dispose();
		}
	}
}
