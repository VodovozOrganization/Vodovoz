using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Utilities.Debug;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Settings.Reports;

namespace Vodovoz.ViewModels.Reports
{
	public class ProductionWarehouseMovementReportViewModel : DialogTabViewModelBase
	{
		private readonly IInteractiveService _interactiveService;

		private const string _help =
			"В отчёт попадают только ТМЦ из документов перемещений с типом <b>Вода</b>, со статусами перемещений <b>Принят</b> и <b>Расхождение</b>.\n" +
			"\nФильтры:\n" +
			"- <b>Период</b> - ограничивает даты документов-перемещений;\n" +
			"- <b>Детализированный</b> - выбор вида отчёта;\n" +
			"- <b>Производство</b> - фильтр по складам с типом использования Производство, ограничивает документы перемещения,\n" +
			" отбирая только те, где Склад-отправитель = Выбранный склад.\n";

		private DelegateCommand _runReportCommand = null;
		private DelegateCommand _exportCommand = null;
		private DelegateCommand _helpCommand = null;
		private readonly IFileDialogService _fileDialogService;
		private readonly IProductionWarehouseMovementReportSettings _productionWarehouseMovementReportProvider;
		private bool _isDetailedForExport;
		private DateTime? _filterStartDate;

		public ProductionWarehouseMovementReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IFileDialogService fileDialogService,
			IProductionWarehouseMovementReportSettings productionWarehouseMovementReportProvider)
			: base(unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory)), interactiveService, navigationManager)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_productionWarehouseMovementReportProvider = productionWarehouseMovementReportProvider ?? throw new ArgumentNullException(nameof(productionWarehouseMovementReportProvider));

			Title = "Отчет по перемещениям с производств";
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			FilterWarehouse = new Warehouse() { Id = _productionWarehouseMovementReportProvider.DefaultProductionWarehouseId };
			WarehouseList = UoW.Query<Warehouse>().Where(x => x.TypeOfUse == WarehouseUsing.Production).List();
		}

		public DelegateCommand RunReportCommand => _runReportCommand ?? (_runReportCommand = new DelegateCommand(
			() =>
			{
				try
				{
					Report = new ProductionWarehouseMovementReport(UoW, FilterStartDate, FilterEndDate, WarehouseList, FilterWarehouse);
					Report.Fill(IsDetailed);
					_isDetailedForExport = IsDetailed;
				}
				catch(Exception ex)
				{
					if(ex.FindExceptionTypeInInner<TimeoutException>() != null)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Error, "Превышен интервал ожидания выполнения запроса.\n Попробуйте уменьшить период");
					}
					else
					{
						throw;
					}
				}
			},
			() => true
		));

		public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(
			() =>
			{
				try
				{
					Report?.Export(_fileDialogService, TabName, _isDetailedForExport);
				}
				catch(Exception e)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, $"При выгрузке произошла ошибка.\n{ e.Message }");
				}
			},
			() => true
		));

		public DelegateCommand HelpCommand => _helpCommand ?? (_helpCommand = new DelegateCommand(
			() => { _interactiveService.ShowMessage(ImportanceLevel.Info, _help, "Справка"); },
			() => true)
			);

		#region Properties

		[PropertyChangedAlso(nameof(CanGenerate))]
		public virtual DateTime? FilterStartDate
		{
			get => _filterStartDate;
			set => SetField(ref _filterStartDate, value);
		}

		public DateTime? FilterEndDate { get; set; }
		public IList<Warehouse> WarehouseList { get; }
		public Warehouse FilterWarehouse { get; set; }
		public bool IsDetailed { get; set; }
		public ProductionWarehouseMovementReport Report { get; set; }
		public bool CanGenerate => FilterStartDate != null;

		#endregion
	}
}
