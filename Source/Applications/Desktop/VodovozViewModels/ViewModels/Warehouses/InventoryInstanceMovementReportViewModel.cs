using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ClosedXML.Excel;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Core.Domain.Warehouses.Documents;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.WriteOffDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.ViewModels.ViewModels.Warehouses
{
	public class InventoryInstanceMovementReportViewModel : DialogTabViewModelBase
	{
		private static readonly string _xlsxExtension = ".xlsx";
		private static readonly string _xlsxFileFilter = $"XLSX File (*.{_xlsxExtension})";
		private static readonly string _defaultTabName = "Движение по инвентарному номеру";
		private readonly ILifetimeScope _scope;
		private readonly IFileDialogService _fileDialogService;
		private InventoryNomenclatureInstance _selectedInstance;
		private InventoryInstanceMovementHistoryNode _selectedNode;
		private string _inventoryNumber;
		private bool _isGenerating;
		private DelegateCommand _openWarehouseDocumentCommand;

		public InventoryInstanceMovementReportViewModel(
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ILifetimeScope scope,
			IFileDialogService fileDialogService) :base(uowFactory, interactiveService, navigation)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			TabName = _defaultTabName;
			SetEntryViewModel();
		}

		public bool IsGenerating
		{
			get => _isGenerating;
			set => SetField(ref _isGenerating, value);
		}

		public InventoryNomenclatureInstance SelectedInstance
		{
			get => _selectedInstance;
			set
			{
				SetField(ref _selectedInstance, value);
				InventoryNumber = _selectedInstance?.GetInventoryNumber;
			}
		}

		public string InventoryNumber
		{
			get => _inventoryNumber;
			set => SetField(ref _inventoryNumber, value);
		}

		public InventoryInstanceMovementHistoryNode SelectedNode
		{
			get => _selectedNode;
			set => SetField(ref _selectedNode, value);
		}

		public IList<InventoryInstanceMovementHistoryNode> MovementHistoryNodes { get; private set; }
		public IEntityEntryViewModel InventoryInstanceEntryViewModel { get; private set; }
		public CancellationTokenSource ReportGenerationCancellationTokenSource { get; set; }

		public DelegateCommand OpenWarehouseDocumentCommand =>
			_openWarehouseDocumentCommand ?? (_openWarehouseDocumentCommand = new DelegateCommand(
				() =>
				{
					if(SelectedNode is null)
					{
						return;
					}

					switch(SelectedNode.DocumentType)
					{
						case DocumentType.IncomingInvoice:
							NavigationManager.OpenViewModel<IncomingInvoiceViewModel, IEntityUoWBuilder>(
								this, EntityUoWBuilder.ForOpen(SelectedNode.DocumentId), OpenPageOptions.AsSlave);
							break;
						case DocumentType.MovementDocument:
							NavigationManager.OpenViewModel<MovementDocumentViewModel, IEntityUoWBuilder>(
								this, EntityUoWBuilder.ForOpen(SelectedNode.DocumentId), OpenPageOptions.AsSlave);
							break;
						case DocumentType.WriteoffDocument:
							NavigationManager.OpenViewModel<WriteOffDocumentViewModel, IEntityUoWBuilder>(
								this, EntityUoWBuilder.ForOpen(SelectedNode.DocumentId), OpenPageOptions.AsSlave);
							break;
					}
				}));
		
		public async Task ActionGenerateReportAsync(CancellationToken cancellationToken)
		{
			var uow = UnitOfWorkFactory.CreateWithoutRoot("Отчет движение по инвентарному номеру");
			try
			{
				MovementHistoryNodes =  await GenerateAsync(uow, cancellationToken);
			}
			finally
			{
				uow.Dispose();
			}
		}

		public void ExportReport()
		{
			if(!MovementHistoryNodes.Any())
			{
				ShowWarningMessage("Нечего выгружать, отчет пустой");
				return;
			}

			try
			{
				Export();
			}
			catch(OutOfMemoryException)
			{
				ShowWarningMessage("Слишком большой обьём данных.\n Пожалуйста, уменьшите выборку.");
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}
		
		public void ShowWarning(string warning, string title = null) => ShowWarningMessage(warning, title);

		private void SetEntryViewModel()
		{
			var builder = new CommonEEVMBuilderFactory<InventoryInstanceMovementReportViewModel>(
				this, this, UoW, NavigationManager, _scope);

			InventoryInstanceEntryViewModel = builder.ForProperty(x => x.SelectedInstance)
				.UseViewModelDialog<InventoryInstanceViewModel>()
				.UseViewModelJournalAndAutocompleter<InventoryInstancesJournalViewModel>()
				.Finish();
		}

		private async Task<IList<InventoryInstanceMovementHistoryNode>> GenerateAsync(
			IUnitOfWork localUow,
			CancellationToken cancellationToken)
		{
			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee editorAlias = null;
			Employee employeeStorageAlias = null;
			Car carStorageAlias = null;
			CarModel carModelAlias = null;
			IncomingInvoiceItem incomingInvoiceItemAlias = null;
			IncomingInvoice incomingInvoiceAlias = null;
			MovementDocumentItem movementDocumentItemAlias = null;
			MovementDocument movementDocumentAlias = null;
			WriteOffDocumentItem writeOffDocumentItemAlias = null;
			WriteOffDocument writeOffDocumentAlias = null;
			InstanceGoodsAccountingOperation instanceOperationAlias = null;
			InventoryInstanceMovementHistoryNode resultAlias = null;

			var warehouseProjection = CustomProjections.Concat(
				Projections.Constant("Склад: "),
				Projections.Property(() => warehouseAlias.Name));
			
			var authorProjection = CustomProjections.Concat(
				Projections.Property(() => authorAlias.LastName),
				Projections.Constant(" "),
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "LEFT(?1, 1)"),
					NHibernateUtil.String,
					Projections.Property(() => authorAlias.Name)),
				Projections.Constant("."),
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "LEFT(?1, 1)"),
					NHibernateUtil.String,
					Projections.Property(() => authorAlias.Patronymic)),
				Projections.Constant(".")
			);

			var editorProjection = CustomProjections.Concat(
				Projections.Property(() => editorAlias.LastName),
				Projections.Constant(" "),
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "LEFT(?1, 1)"),
					NHibernateUtil.String,
					Projections.Property(() => editorAlias.Name)),
				Projections.Constant("."),
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "LEFT(?1, 1)"),
					NHibernateUtil.String,
					Projections.Property(() => editorAlias.Patronymic)),
				Projections.Constant(".")
			);

			var employeeStorageProjection = CustomProjections.Concat(
				Projections.Constant("Сотрудник: "),
				Projections.Property(() => employeeStorageAlias.LastName),
				Projections.Constant(" "),
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "LEFT(?1, 1)"),
					NHibernateUtil.String,
					Projections.Property(() => employeeStorageAlias.Name)),
				Projections.Constant("."),
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "LEFT(?1, 1)"),
					NHibernateUtil.String,
					Projections.Property(() => employeeStorageAlias.Patronymic)),
				Projections.Constant(".")
			);

			var carStorageProjection = CustomProjections.Concat(
				() => "Автомобиль: ",
				() => carModelAlias.Name,
				() => " ",
				() => carStorageAlias.RegistrationNumber);
			
			var storageMovementProjection = Projections.Conditional(
				Restrictions.WhereNot(() => warehouseAlias.Name == null),
				warehouseProjection,
				Projections.Conditional(
					Restrictions.WhereNot(() => employeeStorageAlias.Name == null),
					employeeStorageProjection,
					Projections.Conditional(
						Restrictions.WhereNot(() => carModelAlias.Name == null),
						carStorageProjection,
						Projections.Constant(string.Empty))));

			var queryIncomingInvoice = localUow.Session.QueryOver(() => incomingInvoiceAlias)
				.JoinAlias(ii => ii.Warehouse, () => warehouseAlias)
				.JoinEntityAlias(() => authorAlias, () => incomingInvoiceAlias.AuthorId == authorAlias.Id, NHibernate.SqlCommand.JoinType.InnerJoin)
				.JoinEntityAlias(() => editorAlias, () => incomingInvoiceAlias.LastEditorId == editorAlias.Id, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(ii => ii.Items, () => incomingInvoiceItemAlias)
				.JoinAlias(() => incomingInvoiceItemAlias.GoodsAccountingOperation, () => instanceOperationAlias)
				.Where(() => instanceOperationAlias.InventoryNomenclatureInstance.Id == SelectedInstance.Id)
				.SelectList(list => list
					.SelectGroup(ii => ii.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => DocumentType.IncomingInvoice.GetEnumTitle()).WithAlias(() => resultAlias.Document)
					.Select(() => DocumentType.IncomingInvoice).WithAlias(() => resultAlias.DocumentType)
					.Select(ii => ii.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(warehouseProjection).WithAlias(() => resultAlias.Receiver)
					.Select(authorProjection).WithAlias(() => resultAlias.Author)
					.Select(editorProjection).WithAlias(() => resultAlias.Editor)
					.Select(ii => ii.Comment).WithAlias(() => resultAlias.Comment)
				)
				.TransformUsing(Transformers.AliasToBean<InventoryInstanceMovementHistoryNode>());

			var queryWriteOffMovement = localUow.Session.QueryOver(() => movementDocumentAlias)
				.JoinEntityAlias(() => authorAlias, () => movementDocumentAlias.AuthorId == authorAlias.Id, NHibernate.SqlCommand.JoinType.InnerJoin)
				.JoinEntityAlias(() => editorAlias, () => movementDocumentAlias.LastEditorId == editorAlias.Id, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Left.JoinAlias(md => md.FromWarehouse, () => warehouseAlias)
				.Left.JoinAlias(md => md.FromEmployee, () => employeeStorageAlias)
				.Left.JoinAlias(md => md.FromCar, () => carStorageAlias)
				.Left.JoinAlias(() => carStorageAlias.CarModel, () => carModelAlias)
				.JoinAlias(md => md.Items, () => movementDocumentItemAlias)
				.JoinAlias(() => movementDocumentItemAlias.WriteOffOperation, () => instanceOperationAlias)
				.Where(() => instanceOperationAlias.InventoryNomenclatureInstance.Id == SelectedInstance.Id)
				.SelectList(list => list
					.SelectGroup(md => md.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => DocumentType.MovementDocument.GetEnumTitle()).WithAlias(() => resultAlias.Document)
					.Select(() => DocumentType.MovementDocument).WithAlias(() => resultAlias.DocumentType)
					.Select(md => md.SendTime).WithAlias(() => resultAlias.Date)
					.Select(storageMovementProjection).WithAlias(() => resultAlias.Sender)
					.Select(authorProjection).WithAlias(() => resultAlias.Author)
					.Select(editorProjection).WithAlias(() => resultAlias.Editor)
					.Select(md => md.Comment).WithAlias(() => resultAlias.Comment)
				)
				.TransformUsing(Transformers.AliasToBean<InventoryInstanceMovementHistoryNode>());
			
			var queryIncomeMovement = localUow.Session.QueryOver(() => movementDocumentAlias)
				.JoinEntityAlias(() => authorAlias, () => movementDocumentAlias.AuthorId == authorAlias.Id, NHibernate.SqlCommand.JoinType.InnerJoin)
				.JoinEntityAlias(() => editorAlias, () => movementDocumentAlias.LastEditorId == editorAlias.Id, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Left.JoinAlias(md => md.ToWarehouse, () => warehouseAlias)
				.Left.JoinAlias(md => md.ToEmployee, () => employeeStorageAlias)
				.Left.JoinAlias(md => md.ToCar, () => carStorageAlias)
				.Left.JoinAlias(() => carStorageAlias.CarModel, () => carModelAlias)
				.JoinAlias(md => md.Items, () => movementDocumentItemAlias)
				.JoinAlias(() => movementDocumentItemAlias.IncomeOperation, () => instanceOperationAlias)
				.Where(() => instanceOperationAlias.InventoryNomenclatureInstance.Id == SelectedInstance.Id)
				.SelectList(list => list
					.SelectGroup(md => md.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => DocumentType.MovementDocument.GetEnumTitle()).WithAlias(() => resultAlias.Document)
					.Select(() => DocumentType.MovementDocument).WithAlias(() => resultAlias.DocumentType)
					.Select(md => md.ReceiveTime).WithAlias(() => resultAlias.Date)
					.Select(storageMovementProjection).WithAlias(() => resultAlias.Receiver)
					.Select(authorProjection).WithAlias(() => resultAlias.Author)
					.Select(editorProjection).WithAlias(() => resultAlias.Editor)
					.Select(md => md.Comment).WithAlias(() => resultAlias.Comment)
				)
				.TransformUsing(Transformers.AliasToBean<InventoryInstanceMovementHistoryNode>());
			
			var queryWriteOffDocument = localUow.Session.QueryOver(() => writeOffDocumentAlias)
				.JoinEntityAlias(() => authorAlias, () => writeOffDocumentAlias.AuthorId == authorAlias.Id, NHibernate.SqlCommand.JoinType.InnerJoin)
				.JoinEntityAlias(() => editorAlias, () => writeOffDocumentAlias.LastEditorId == editorAlias.Id, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Left.JoinAlias(wo => wo.WriteOffFromWarehouse, () => warehouseAlias)
				.Left.JoinAlias(wo => wo.WriteOffFromEmployee, () => employeeStorageAlias)
				.Left.JoinAlias(wo => wo.WriteOffFromCar, () => carStorageAlias)
				.Left.JoinAlias(() => carStorageAlias.CarModel, () => carModelAlias)
				.JoinAlias(wo => wo.Items, () => writeOffDocumentItemAlias)
				.JoinAlias(() => writeOffDocumentItemAlias.GoodsAccountingOperation, () => instanceOperationAlias)
				.Where(() => instanceOperationAlias.InventoryNomenclatureInstance.Id == SelectedInstance.Id)
				.SelectList(list => list
					.SelectGroup(wo => wo.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => DocumentType.WriteoffDocument.GetEnumTitle()).WithAlias(() => resultAlias.Document)
					.Select(() => DocumentType.WriteoffDocument).WithAlias(() => resultAlias.DocumentType)
					.Select(wo => wo.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(storageMovementProjection).WithAlias(() => resultAlias.Sender)
					.Select(authorProjection).WithAlias(() => resultAlias.Author)
					.Select(editorProjection).WithAlias(() => resultAlias.Editor)
					.Select(wo => wo.Comment).WithAlias(() => resultAlias.Comment)
				)
				.TransformUsing(Transformers.AliasToBean<InventoryInstanceMovementHistoryNode>());
			
			var result = new List<InventoryInstanceMovementHistoryNode>(queryIncomingInvoice.List<InventoryInstanceMovementHistoryNode>());
			result.AddRange(queryWriteOffMovement.List<InventoryInstanceMovementHistoryNode>());
			result.AddRange(queryIncomeMovement.List<InventoryInstanceMovementHistoryNode>());
			result.AddRange(queryWriteOffDocument.List<InventoryInstanceMovementHistoryNode>());

			cancellationToken.ThrowIfCancellationRequested();
			return result.OrderBy(x => x.Date).ToList();
		}

		private void Export()
		{
			using(var wb = new XLWorkbook())
			{
				var sheetName = $"{DateTime.Now:dd.MM.yyyy}";
				var ws = wb.Worksheets.Add(sheetName);

				InsertValues(ws);
				ws.Columns().AdjustToContents();

				if(TryGetSavePath(out var path))
				{
					wb.SaveAs(path);
				}
			}
		}

		private bool TryGetSavePath(out string path)
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				FileName = $"{_defaultTabName} {DateTime.Now:yyyy-MM-dd-HH-mm}{_xlsxExtension}"
			};

			dialogSettings.FileFilters.Add(new DialogFileFilter(_xlsxFileFilter, $"*{_xlsxExtension}"));
			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			path = result.Path;

			return result.Successful;
		}

		private void InsertValues(IXLWorksheet ws)
		{
			var colNames = new[] { "Дата", "№ док-та", "Документ", "Отправитель", "Получатель", "Автор", "Изменил", "Комментарий" };
			var rows = from row in MovementHistoryNodes
				select new
				{
					row.Date,
					row.DocumentId,
					row.Document,
					row.Sender,
					row.Receiver,
					row.Author,
					row.Editor,
					row.Comment
				};
			
			int index = 1;
			var title = ws.Range(1, index, 1, colNames.Length);
			title.Cell(1, 1).Value =
				$"Отчет по движению инвентарного номера({_selectedInstance.Name} инв. номер: {_selectedInstance.GetInventoryNumber})";
			title.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
			title.Merge();
			
			foreach(var name in colNames)
			{
				ws.Cell(3, index).Value = name;
				index++;
			}
			ws.Cell(4, 1).InsertData(rows);
		}
	}
}
