using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Print;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services.FileDialog;
using QS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.StoredResources;
using Vodovoz.Infrastructure.Print;
using Vodovoz.ViewModels.Journals.FilterViewModels.Receipts;
using Vodovoz.ViewModels.Journals.JournalNodes.Receipts;
using Vodovoz.ViewModels.Print.Receipts;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Receipts
{
	public class ReceiptCorrectionExplanatoryNoteJournalViewModel : JournalViewModelBase
	{
		private readonly ReceiptCorrectionExplanatoryNoteJournalFilterViewModel _filterViewModel;
		private readonly IReceiptCorrectionRepository _receiptCorrectionRepository;
		private readonly IFileDialogService _fileDialogService;
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IDocumentPrinter _documentPrinter;

		public ReceiptCorrectionExplanatoryNoteJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ReceiptCorrectionExplanatoryNoteJournalFilterViewModel filterViewModel,
			IReceiptCorrectionRepository receiptCorrectionRepository,
			IFileDialogService fileDialogService,
			IReportInfoFactory reportInfoFactory,
			IDocumentPrinter documentPrinter,
			QS.Dialog.IInteractiveService interactiveService,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_receiptCorrectionRepository = receiptCorrectionRepository ?? throw new ArgumentNullException(nameof(receiptCorrectionRepository));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_documentPrinter = documentPrinter ?? throw new ArgumentNullException(nameof(documentPrinter));

			Title = "Реестр пояснительных записок";
			SelectionMode = JournalSelectionMode.Multiple;
			DataLoader = new AnyDataLoader<ReceiptCorrectionExplanatoryNoteJournalNode>(GetNodes);
			SearchEnabled = false;
			_filterViewModel.IsShow = true;
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			CreateNodeActions();
		}

		public override IJournalFilterViewModel JournalFilter
		{
			get => _filterViewModel;
			protected set => throw new NotSupportedException("Установка фильтра выполняется через конструктор");
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			base.CreateNodeActions();

			var printDialogAction = new JournalAction(
				"Диалог печати...",
				selected => GetNodesForPrint(selected).Any(),
				_ => true,
				selected => OpenPrintDialog(selected));
			RowActivatedAction = printDialogAction;
			NodeActionsList.Add(printDialogAction);

			NodeActionsList.Add(new JournalAction(
				"Печатать выбранные",
				selected => GetNodesForPrint(selected).Any(),
				_ => true,
				selected => PrintSelected(selected)));

			NodeActionsList.Add(new JournalAction(
				"Выгрузить в Excel",
				_ => true,
				_ => true,
				_ => ExportRegistryExcel()));

			NodeActionsList.Add(new JournalAction(
				"Выгрузить в PDF",
				_ => true,
				_ => true,
				_ => ExportRegistryPdf()));
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e) => Refresh();

		private IList<ReceiptCorrectionExplanatoryNoteJournalNode> GetNodes(CancellationToken token)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var notes = _receiptCorrectionRepository
					.GetExplanatoryNotes(
						uow,
						_filterViewModel.OrderId,
						_filterViewModel.Organization?.Id)
					.Where(note => note.CreatedDate.Date >= _filterViewModel.DateFrom.Date
						&& note.CreatedDate.Date <= _filterViewModel.DateTo.Date)
					.OrderBy(note => note.CreatedDate)
					.ThenBy(note => note.Id)
					.ToList();

				var organizationIds = notes
					.Where(x => x.OrganizationId.HasValue)
					.Select(x => x.OrganizationId.Value)
					.Distinct()
					.ToList();

				var organizations = organizationIds.Count == 0
					? new Dictionary<int, string>()
					: uow.Session.Query<OrganizationEntity>()
						.Where(x => organizationIds.Contains(x.Id))
						.Select(x => new { x.Id, x.Name, x.FullName })
						.ToList()
						.GroupBy(x => x.Id)
						.ToDictionary(
							g => g.Key,
							g =>
							{
								var org = g.First();
								return string.IsNullOrWhiteSpace(org.Name) ? org.FullName : org.Name;
							});

				var rowNumber = 1;
				return notes
					.Select(note => new ReceiptCorrectionExplanatoryNoteJournalNode
					{
						RowNumber = rowNumber++,
						Id = note.Id,
						OrderId = note.Process.OrderId,
						ProcessId = note.Process.Id,
						ScenarioType = note.Process.ScenarioType,
						TemplateType = note.TemplateType,
						SignerName = note.SignerName,
						SignerSignatureId = note.SignerSignatureId,
						OrganizationName = note.OrganizationId.HasValue
							&& organizations.TryGetValue(note.OrganizationId.Value, out var orgName)
							? orgName
							: null,
						FiscalDocumentNumber = note.Process.BaselineFiscalDocumentNumber,
						Content = note.Content,
						CreatedDate = note.CreatedDate
					})
					.ToList();
			}
		}

		private IList<ReceiptCorrectionExplanatoryNoteJournalNode> GetNodesForPrint(object[] selected)
		{
			var selectedNodes = selected?
				.OfType<ReceiptCorrectionExplanatoryNoteJournalNode>()
				.ToList();

			if(selectedNodes != null && selectedNodes.Count > 0)
			{
				return selectedNodes;
			}

			return new List<ReceiptCorrectionExplanatoryNoteJournalNode>();
		}

		private IList<ExplanatoryNotePrintableDocument> CreatePrintableDocuments(
			IList<ReceiptCorrectionExplanatoryNoteJournalNode> nodes)
		{
			var signatures = LoadSignatures(nodes);

			return nodes
				.Select(node =>
				{
					byte[] signaturePng = null;
					if(node.SignerSignatureId.HasValue
						&& signatures.TryGetValue(node.SignerSignatureId.Value, out var bytes))
					{
						signaturePng = ReceiptCorrectionExplanatoryNoteExportHelper.TryPrepareSignaturePng(bytes);
					}

					return new ExplanatoryNotePrintableDocument(
						_reportInfoFactory,
						node.Id,
						node.OrderId,
						node.Content,
						node.CreatedDate,
						signaturePng);
				})
				.ToList();
		}

		private IDictionary<int, byte[]> LoadSignatures(IList<ReceiptCorrectionExplanatoryNoteJournalNode> nodes)
		{
			var ids = nodes
				.Where(x => x.SignerSignatureId.HasValue)
				.Select(x => x.SignerSignatureId.Value)
				.Distinct()
				.ToList();

			if(ids.Count == 0)
			{
				return new Dictionary<int, byte[]>();
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				return uow.Session.Query<StoredResource>()
					.Where(x => ids.Contains(x.Id))
					.ToList()
					.ToDictionary(x => x.Id, x => x.BinaryFile);
			}
		}

		private void OpenPrintDialog(object[] selected)
		{
			var nodes = GetNodesForPrint(selected);
			if(nodes.Count == 0)
			{
				ShowWarningMessage("Выберите одну или несколько записок для печати.");
				return;
			}

			if(NavigationManager == null)
			{
				ShowErrorMessage("Навигатор недоступен для открытия диалога печати.");
				return;
			}

			var documents = CreatePrintableDocuments(nodes);
			var page = NavigationManager.OpenViewModel<ExplanatoryNotesPrinterViewModel>(this, OpenPageOptions.AsSlave);
			page.ViewModel.Configure(documents);
		}

		private void PrintSelected(object[] selected)
		{
			var nodes = GetNodesForPrint(selected);
			if(nodes.Count == 0)
			{
				ShowWarningMessage("Выберите одну или несколько записок для печати.");
				return;
			}

			var documents = CreatePrintableDocuments(nodes)
				.Cast<IPrintableDocument>()
				.ToList();
			_documentPrinter.PrintAllDocuments(documents);
		}

		private IList<ReceiptCorrectionExplanatoryNoteJournalNode> GetExportNodes(object[] selected)
		{
			var selectedNodes = selected?
				.OfType<ReceiptCorrectionExplanatoryNoteJournalNode>()
				.ToList();

			if(selectedNodes != null && selectedNodes.Count > 0)
			{
				return selectedNodes;
			}

			return Items?.OfType<ReceiptCorrectionExplanatoryNoteJournalNode>().ToList()
				?? new List<ReceiptCorrectionExplanatoryNoteJournalNode>();
		}

		private void ExportRegistryExcel()
		{
			var nodes = GetExportNodes(null);
			if(!EnsureHasNodes(nodes))
			{
				return;
			}

			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить реестр Excel",
				FileName = $"Реестр_пояснительных_{DateTime.Now:yyyy-MM-dd}.xlsx"
			};
			dialogSettings.FileFilters.Add(new DialogFileFilter("Excel (*.xlsx)", "*.xlsx"));
			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!result.Successful)
			{
				return;
			}

			ReceiptCorrectionExplanatoryNoteExportHelper.ExportRegistryToExcel(nodes, result.Path);
			ShowInfoMessage($"Сохранено:\n{result.Path}");
		}

		private void ExportRegistryPdf()
		{
			var nodes = GetExportNodes(null);
			if(!EnsureHasNodes(nodes))
			{
				return;
			}

			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить реестр PDF",
				FileName = $"Реестр_пояснительных_{DateTime.Now:yyyy-MM-dd}.pdf"
			};
			dialogSettings.FileFilters.Add(new DialogFileFilter("PDF (*.pdf)", "*.pdf"));
			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!result.Successful)
			{
				return;
			}

			ReceiptCorrectionExplanatoryNoteExportHelper.ExportRegistryToPdf(nodes, result.Path);
			ShowInfoMessage($"Сохранено:\n{result.Path}");
		}

		private bool EnsureHasNodes(IList<ReceiptCorrectionExplanatoryNoteJournalNode> nodes)
		{
			if(nodes != null && nodes.Count > 0)
			{
				return true;
			}

			ShowWarningMessage("Нет записей для выгрузки по текущему фильтру.");
			return false;
		}
	}
}
