using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.ViewModels.Service
{
	public class ExportTo1CViewModel : DialogTabViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private Organization _selectedCashlessOrganization;
		private Organization _selectedRetailOrganization;
		private DateTime? _endDate;
		private DateTime? _startDate;
		
		[PropertyChangedAlso(nameof(CanExport))]
		private bool ExportInProgress { get; set; }

		public ExportTo1CViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			TabName = "Выгрузка в 1с 8.3";

			// ExportCodesCommand = new DelegateCommand(ExportCodes, () => CanCloseTenderEdoTask);
			// ExportCodesCommand.CanExecuteChangedWith(this, vm => vm.CanCloseTenderEdoTask);
			CashlessOrganizations = UoW.GetAll<Organization>().ToList();
			RetailOrganizations = UoW.GetAll<Organization>().ToList();

		}

		public List<Organization> RetailOrganizations { get; }
		public IList<Organization> CashlessOrganizations { get;}

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

		public ICommand ExportBookkeepingCommand { get; set; }
		public ICommand ExportComplexAutomationCommand { get; set; }
		public ICommand ExportIPTinkoffCommand { get; set; }
		public ICommand ExportBookkeepingNewCommand { get; set; }

		public bool CanExport => !ExportInProgress
		                         && EndDate != null
		                         && StartDate != null
		                         && StartDate <= EndDate;

		[PropertyChangedAlso (nameof(CanExport))]
		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[PropertyChangedAlso (nameof(CanExport))]
		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}


		/*
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IFileDialogService _fileDialogService;
		private readonly IOrderSettings _orderSettings;
		private readonly ICommonServices _commonServices;
		bool exportInProgress;
		public ExportTo1cDialog(
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IDialogSettingsFactory dialogSettingsFactory,
			IFileDialogService fileDialogService,
			IOrderSettings orderSettings,
			ICommonServices commonServices)
		{

			comboOrganization.ItemsList = unitOfWorkFactory.CreateWithoutRoot().GetAll<Organization>();

			buttonExportBookkeeping.Clicked += (sender, args) => Export(Export1cMode.BuhgalteriaOOO);
			ybuttonComplexAutomation1CExport.Clicked += (sender, args) => Export(Export1cMode.ComplexAutomation);
			buttonExportIPTinkoff.Clicked += (sender, args) => Export(Export1cMode.IPForTinkoff);

			ybuttonExportBookkeepingNew.Clicked += (sender, args) => {
				if(comboOrganization.SelectedItem is Organization)
				{
					Export(Export1cMode.BuhgalteriaOOONew);
				}
				else
				{
					MessageDialogHelper.RunWarningDialog("Для этой выгрузки необходимо выбрать организацию");
				}
			};

			ylistcomboboxOrganization.ItemsList = unitOfWorkFactory.CreateWithoutRoot().GetAll<Organization>();
			ylistcomboboxOrganization.SetRenderTextFunc((Organization x) => x.Name);

			ybuttonRetailReport.Clicked += (sender, args) => CreateRetailReport(comboOrganization.SelectedItem as Organization);

			UpdateExportSensitivity();
		}

		private void CreateRetailReport(Organization organization)
		{
			var dateStart = dateperiodpicker1.StartDate;
			var dateEnd = dateperiodpicker1.EndDate;

			if(organization is null)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Не выбрана организация");

				return;
			}

			if(dateStart != dateEnd)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Нельзя сформировать отчёт за диапазон дат. Выберите одну дату.");

				return;
			}

			var retailSalesReport = new RetailSalesReportFor1C
			{
				Organization = organization,
				OrganizationVersionStartDate = dateStart,
				SaleDate = dateStart,
				Suffix = organization.Suffix
			};

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Получение заказов розницы для экспорта в 1С"))
			{
				var orders = _orderRepository.GetOrdersToExport1c8(unitOfWork, _orderSettings, Export1cMode.RetailReport, dateStart, dateEnd, organization.Id);

				retailSalesReport.Generate(orders);
			}

			retailSalesReport.SaveReport(_dialogSettingsFactory, _fileDialogService);
		}

		private ExportData exportData;

		private void Export(Export1cMode mode)
		{
			var organizationSettings = ScopeProvider.Scope.Resolve<IOrganizationSettings>();
			var dateStart = dateperiodpicker1.StartDate;
			var dateEnd = dateperiodpicker1.EndDate;

			int? organizationId = null;
			if(mode == Export1cMode.BuhgalteriaOOONew)
			{
				organizationId = (comboOrganization.SelectedItem as Organization)?.Id;
			}
			else if(mode == Export1cMode.BuhgalteriaOOO || mode == Export1cMode.ComplexAutomation)
			{
				organizationId = organizationSettings.VodovozOrganizationId;
			}

			using(var exportOperation = new ExportOperation(
				mode,
				ScopeProvider.Scope.Resolve<IOrderSettings>(),
				dateStart,
				dateEnd,
				organizationId
				))
			{
				this.exportInProgress = true;
				UpdateExportSensitivity();
				LongOperationDlg.StartOperation(exportOperation.Run, "", 1, false);
				this.exportInProgress = false;
				UpdateExportSensitivity();
				exportData = exportOperation.Result;
			}

			labelTotalCounterparty.Text = exportData.Objects
				.OfType<CatalogObjectNode>()
				.Count(node => node.Type == Common1cTypes.ReferenceCounterparty)
				.ToString();

			labelTotalNomenclature.Text = exportData.Objects
				.OfType<CatalogObjectNode>()
				.Count(node => node.Type == Common1cTypes.ReferenceNomenclature)
				.ToString();

			labelTotalSales.Text = (exportData.Objects
				.OfType<SalesDocumentNode>()
				.Count()
				+ exportData.Objects.OfType<RetailDocumentNode>().Count())
				.ToString();

			labelTotalSum.Text = exportData.OrdersTotalSum.ToString("C", CultureInfo.GetCultureInfo("ru-RU"));

			labelExportedSum.Markup =
				$"<span foreground=\"{(exportData.ExportedTotalSum == exportData.OrdersTotalSum ? GdkColors.SuccessText.ToHtmlColor() : GdkColors.DangerText.ToHtmlColor())}\">" +
				$"{exportData.ExportedTotalSum.ToString("C", CultureInfo.GetCultureInfo("ru-RU"))}</span>";

			labelTotalInvoices.Text = exportData.Objects
				.OfType<InvoiceDocumentNode>()
				.Count()
				.ToString();

			GtkScrolledWindowErrors.Visible = exportData.Errors.Count > 0;
			if(exportData.Errors.Count > 0)
			{
				TextTagTable textTags = new TextTagTable();
				var tag = new TextTag("Red");
				tag.Foreground = "red";
				textTags.Add(tag);
				TextBuffer tempBuffer = new TextBuffer(textTags);
				TextIter iter = tempBuffer.EndIter;
				tempBuffer.InsertWithTags(ref iter, String.Join("\n", exportData.Errors), tag);
				textviewErrors.Buffer = tempBuffer;
			}

			buttonSave.Sensitive = exportData != null && exportData.Errors.Count == 0;
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			var settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true,
				Encoding = System.Text.Encoding.UTF8,
				NewLineChars = "\r\n"
			};
			var fileChooser = new Gtk.FileChooserDialog("Выберите файл для сохранения выгрузки",
				(Window)this.Toplevel,
				Gtk.FileChooserAction.Save,
				"Отмена", ResponseType.Cancel,
				"Сохранить", ResponseType.Accept
			);
			var dateText = exportData.EndPeriodDate.ToShortDateString().Replace(System.IO.Path.DirectorySeparatorChar, '-');

			fileChooser.CurrentName = $"Выгрузка 1с на {dateText}.xml";
			var filter = new FileFilter();
			filter.AddPattern("*.xml");
			fileChooser.Filter = filter;
			if(fileChooser.Run() == (int)ResponseType.Accept)
			{
				var filename = fileChooser.Filename.EndsWith(".xml") ? fileChooser.Filename : fileChooser.Filename + ".xml";
				using(XmlWriter writer = XmlWriter.Create(filename, settings))
				{
					exportData.ToXml().WriteTo(writer);
				}
			}

			fileChooser.Destroy();
		}

		private void UpdateExportSensitivity()
		{
			var canExport =
				!exportInProgress
				&& dateperiodpicker1.EndDateOrNull != null
				&& dateperiodpicker1.StartDateOrNull != null
				&& dateperiodpicker1.StartDate <= dateperiodpicker1.EndDate;

			comboOrganization.Sensitive = canExport;
			buttonExportBookkeeping.Sensitive = canExport;
			ybuttonComplexAutomation1CExport.Sensitive = canExport;
			buttonExportIPTinkoff.Sensitive = canExport;
			ybuttonExportBookkeepingNew.Sensitive = canExport;
		}

		protected void OnDateperiodpicker1PeriodChanged(object sender, EventArgs e)
		{
			UpdateExportSensitivity();
		}*/
	}
}
