using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
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
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Service
{
	public class ExportTo1CViewModel : DialogTabViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IGtkTabsOpener _gtkTabsOpener;
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
		
		public ExportTo1CViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IGtkTabsOpener gtkTabsOpener)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));

			TabName = "Выгрузка в 1с 8.3";

			CashlessOrganizations = UoW.GetAll<Organization>().ToList();
			RetailOrganizations = UoW.GetAll<Organization>().ToList();

			ExportComplexAutomationCommand = new DelegateCommand(ExportComplexAutomation, () => CanExport);
			ExportComplexAutomationCommand.CanExecuteChangedWith(this, x => CanExport);

			ExportBookkeepingCommand = new DelegateCommand(ExportBookkeeping, () => CanExport);
			ExportBookkeepingCommand.CanExecuteChangedWith(this, x => CanExport);

			ExportBookkeepingNewCommand = new DelegateCommand(ExportBookkeepingNew, () => CanExport);
			ExportBookkeepingNewCommand.CanExecuteChangedWith(this, x => CanExport);

			ExportIPTinkoffCommand = new DelegateCommand(ExportIPTinkoff, () => CanExport);
			ExportIPTinkoffCommand.CanExecuteChangedWith(this, x => CanExport);

			SaveFileCommand = new DelegateCommand(SaveFile, () => CanSave);
			SaveFileCommand.CanExecuteChangedWith(this, x => CanSave);
		}

		private void ExportIPTinkoff()
		{
			Export(Export1cMode.IPForTinkoff);
		}

		private void ExportBookkeepingNew()
		{
			if(SelectedCashlessOrganization == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Для этой выгрузки необходимо выбрать организацию");

				return;
			}

			Export(Export1cMode.BuhgalteriaOOONew);
		}

		private void ExportBookkeeping()
		{
			Export(Export1cMode.BuhgalteriaOOO);
		}

		private void ExportComplexAutomation()
		{
			Export(Export1cMode.ComplexAutomation);
		}

		private void Export(Export1cMode mode)
		{
			var organizationSettings = ScopeProvider.Scope.Resolve<IOrganizationSettings>();
			int? organizationId = null;
			
			switch (mode)
			{
				case Export1cMode.BuhgalteriaOOONew:
					organizationId = (SelectedCashlessOrganization)?.Id;
					break;
				case Export1cMode.BuhgalteriaOOO:
				case Export1cMode.ComplexAutomation:
					organizationId = organizationSettings.VodovozOrganizationId;
					break;
			}

			using(var exportOperation = new ExportTo1cOperation(
				      mode,
				      _orderSettings,
				      _orderRepository,
				      UnitOfWorkFactory,
				      StartDate.Value,
				      EndDate.Value,
				      organizationId
			      ))
			{
				ExportInProgress = true;

				_gtkTabsOpener.RunLongOperation(exportOperation.Run, "", 1, false);
				
				ExportData = exportOperation.Result;

				ExportInProgress = false;

				OnPropertyChanged(nameof(CanSave));
			}

			TotalCounterparty = ExportData.Objects
				.OfType<CatalogObjectNode>()
				.Count(node => node.Type == Common1cTypes.ReferenceCounterparty)
				.ToString();

			TotalNomenclature = ExportData.Objects
				.OfType<CatalogObjectNode>()
				.Count(node => node.Type == Common1cTypes.ReferenceNomenclature)
				.ToString();

			TotalSales = (ExportData.Objects
				              .OfType<SalesDocumentNode>()
				              .Count()
			              + ExportData.Objects.OfType<RetailDocumentNode>().Count())
				.ToString();

			TotalSum = ExportData.OrdersTotalSum.ToString("C", CultureInfo.GetCultureInfo("ru-RU"));

			TotalInvoices = ExportData.Objects
				.OfType<InvoiceDocumentNode>()
				.Count()
				.ToString();

			ExportCompleteAction.Invoke();
		}

		public List<Organization> RetailOrganizations { get; }
		public IList<Organization> CashlessOrganizations { get; }

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

		public DelegateCommand ExportBookkeepingCommand { get; set; }
		public DelegateCommand ExportComplexAutomationCommand { get; set; }
		public DelegateCommand ExportIPTinkoffCommand { get; set; }
		public DelegateCommand ExportBookkeepingNewCommand { get; set; }
		public DelegateCommand SaveFileCommand { get; set; }

		public bool CanExport =>
			!ExportInProgress
			&& EndDate != null
			&& StartDate != null
			&& StartDate <= EndDate;

		public bool CanSave => ExportData?.Errors?.Count == 0;

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

		public Action ExportCompleteAction { get; set; }

		public ExportData ExportData { get; set; }

		private void SaveFile()
		{
			var dialogSettings = CreateDialogSettings();

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
				ExportData.ToXml().WriteTo(writer);
			}

			_interactiveService.ShowMessage(ImportanceLevel.Info, "Выгрузка завершена");
		}

		private DialogSettings CreateDialogSettings()
		{
			var dateText = ExportData.EndPeriodDate.ToShortDateString().Replace(Path.DirectorySeparatorChar, '-');

			var settings = new DialogSettings
			{
				Title = "Выберите файл для сохранения выгрузки",
				DefaultFileExtention = "*.xml",
				InitialDirectory = SpecialDirectories.Desktop,
				FileName = $"Выгрузка 1с на {dateText}.xml"
			};

			settings.FileFilters.Clear();
			settings.FileFilters.Add(new DialogFileFilter($"XML файлы", "*.xml"));

			return settings;
		}
	}
}
