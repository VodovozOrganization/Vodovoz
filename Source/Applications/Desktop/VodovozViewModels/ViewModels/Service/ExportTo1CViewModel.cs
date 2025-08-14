using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using ExportTo1c.Library;
using ExportTo1c.Library.ExportNodes;
using Microsoft.VisualBasic.FileIO;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using Vodovoz.ViewModels.ViewModels.Reports.Sales.RetailSalesReportFor1C;

namespace Vodovoz.ViewModels.ViewModels.Service
{
	public class ExportTo1CViewModel : DialogViewModelBase, IDisposable
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IGenericRepository<Organization> _organizationRepository;
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
		private readonly CancellationTokenSource _cancellationTokenSource;

		private readonly List<RetailSalesReportFor1cOrganizationSettings> _retailSalesReportFor1cOrganizationSettings =
			new List<RetailSalesReportFor1cOrganizationSettings>();

		public ExportTo1CViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IDialogSettingsFactory dialogSettingsFactory,
			IGenericRepository<Organization> organizationRepository,
			IOrganizationSettings organizationSettings)
			: base(navigation)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_dialogSettingsFactory = dialogSettingsFactory ?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
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
					_retailSalesReportFor1cOrganizationSettings.Add(new RetailSalesReportFor1cOrganizationSettings(organization));
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

		private IEnumerable<RetailSalesReportFor1C> CreateRetailReports(CancellationToken cancellationToken)
		{
			var reports = new List<RetailSalesReportFor1C>();

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

		private RetailSalesReportFor1C CreateRetailReportForOrganization(Organization organization, CancellationToken cancellationToken)
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

				return new RetailSalesReportFor1C();
			}

			var phone = export1CRetailOrganization.Phone;

			var retailSalesReport = new RetailSalesReportFor1C
			{
				OrganizationVersionForTitle = organizationVersion,
				OrganizationForTitle = organization,
				Phone = phone,
				SaleDate = StartDate.Value
			};

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Получение заказов розницы для отчёта"))
			{
				var orders = GetRetailOrders(unitOfWork, organization);

				retailSalesReport.Generate(orders, ProgressBarDisplayable, cancellationToken);
			}

			return retailSalesReport;
		}

		private void ExportRetail()
		{
			ExportInProgress = true;

			if(SelectedRetailOrganization is null)
			{
				foreach(var organization in RetailOrganizations)
				{
					ExportRetailForOrganization(organization);
				}
			}
			else
			{
				ExportRetailForOrganization(SelectedRetailOrganization);
			}

			_interactiveService.ShowMessage(ImportanceLevel.Info, "Экспорт розницы завершен");

			ExportInProgress = false;
		}

		private void ExportRetailForOrganization(Organization organization)
		{
			XElement xml;

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Получение заказов розницы для экспорта в 1С"))
			{
				var orders = GetRetailOrders(unitOfWork, organization);

				xml = Retail1cDataExporter.CreateRetailXml(
					orders,
					StartDate.Value,
					EndDate.Value,
					organization.INN,
					_cancellationTokenSource.Token,
					ProgressBarDisplayable);
			}

			var dateText = EndDate.Value.ToString("yyyy-MM-dd");
			var fileName = $"Выгрузка 1с на {dateText} ИНН {organization.INN} (розница).xml";

			SaveXmlToFile(xml, fileName);
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

			ExportInProgress = true;

			var reports = CreateRetailReports(_cancellationTokenSource.Token);

			foreach(var report in reports)
			{
				report.SaveReport(_dialogSettingsFactory, _fileDialogService, report.OrganizationForTitle.INN);
			}

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

		public void Dispose()
		{
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource?.Dispose();
		}
	}
}
