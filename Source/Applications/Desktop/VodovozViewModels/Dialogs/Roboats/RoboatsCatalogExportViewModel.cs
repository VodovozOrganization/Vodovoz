using ClosedXML.Excel;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Factories;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Commands;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Dialogs.Logistic;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Dialogs.Roboats
{
	public class RoboatsCatalogExportViewModel : TabViewModelBase
	{
		private DelegateCommand _startExport;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IFileDialogService _fileDialogService;
		private readonly IRoboatsFileStorageFactory _roboatsFileStorageFactory;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IDeliveryScheduleJournalFactory _deliveryScheduleJournalFactory;
		private readonly RoboatsJournalsFactory _roboatsJournalsFactory;
		private readonly ICommonServices _commonServices;
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository;
		private readonly IRoboatsViewModelFactory _roboatsViewModelFactory;
		private RoboatsEntityType? _selectedExportType;
		private OpenViewModelCommand _openDialogCommand;
		private bool exportStarting;

		public RoboatsEntityType? SelectedExportType
		{
			get => _selectedExportType;
			set
			{
				if(SetField(ref _selectedExportType, value))
				{
					UpdateJournal();
					OnPropertyChanged(nameof(CanStartExport));
				}
			}
		}

		public bool CanStartExport => !exportStarting && SelectedExportType != null;

		public RoboatsCatalogExportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IFileDialogService fileDialogService,
			IRoboatsFileStorageFactory roboatsFileStorageFactory,
			IRoboatsRepository roboatsRepository,
			IDeliveryScheduleJournalFactory deliverySchedulejournalFactory,
			RoboatsJournalsFactory roboatsJournalsFactory,
			ICommonServices commonServices,
			IDeliveryScheduleRepository deliveryScheduleRepository,
			IRoboatsViewModelFactory roboatsViewModelFactory,
			INavigationManager navigation) : base(commonServices.InteractiveService, navigation)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_roboatsFileStorageFactory = roboatsFileStorageFactory ?? throw new ArgumentNullException(nameof(roboatsFileStorageFactory));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_deliveryScheduleJournalFactory = deliverySchedulejournalFactory ?? throw new ArgumentNullException(nameof(deliverySchedulejournalFactory));
			_roboatsJournalsFactory = roboatsJournalsFactory ?? throw new ArgumentNullException(nameof(roboatsJournalsFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_deliveryScheduleRepository = deliveryScheduleRepository ?? throw new ArgumentNullException(nameof(deliveryScheduleRepository));
			_roboatsViewModelFactory = roboatsViewModelFactory ?? throw new ArgumentNullException(nameof(roboatsViewModelFactory));
			_openDialogCommand = new OpenViewModelCommand(OpenDialog);

			var canEdit = _commonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_roboats");
			if(!canEdit)
			{
				commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Нет права работать со справочниками для Roboats", "Доступ запрещен");
				FailInitialize = true;
				return;
			}

			Title = "Справочники Roboats";

			SetAvailableJournals();
			UpdateJournal();
		}

		public IEnumerable<RoboatsEntityType> DeniedEntityTypes { get; private set; }

		private void SetAvailableJournals()
		{
			var validationService = _commonServices.CurrentPermissionService;
			IList<RoboatsEntityType> deniedEntityTypes = new List<RoboatsEntityType>();

			if(!validationService.ValidateEntityPermission(typeof(DeliverySchedule)).CanRead)
			{
				deniedEntityTypes.Add(RoboatsEntityType.DeliverySchedules);
			}

			if(!validationService.ValidateEntityPermission(typeof(RoboatsStreet)).CanRead)
			{
				deniedEntityTypes.Add(RoboatsEntityType.Streets);
			}

			if(!validationService.ValidateEntityPermission(typeof(RoboatsWaterType)).CanRead)
			{
				deniedEntityTypes.Add(RoboatsEntityType.WaterTypes);
			}

			if(!validationService.ValidateEntityPermission(typeof(RoboAtsCounterpartyName)).CanRead)
			{
				deniedEntityTypes.Add(RoboatsEntityType.CounterpartyName);
			}

			if(!validationService.ValidateEntityPermission(typeof(RoboAtsCounterpartyPatronymic)).CanRead)
			{
				deniedEntityTypes.Add(RoboatsEntityType.CounterpartyPatronymic);
			}

			DeniedEntityTypes = deniedEntityTypes;
		}

		private JournalViewModelBase _journal;
		public JournalViewModelBase Journal
		{
			get => _journal;
			set => SetField(ref _journal, value);
		}

		private ViewModelBase _dialog;

		public ViewModelBase Dialog
		{
			get => _dialog;
			set => SetField(ref _dialog, value);
		}


		private void UpdateJournal()
		{
			Journal?.Dispose();
			CloseDialog();
			switch(SelectedExportType)
			{
				case RoboatsEntityType.DeliverySchedules:
					var deliveryScheduleJournal = _deliveryScheduleJournalFactory.CreateJournal(JournalSelectionMode.Single);
					deliveryScheduleJournal.SetForRoboatsCatalogExport(_openDialogCommand);
					deliveryScheduleJournal.OnSelectResult += OnDeliveryScheduleJournalSelected;
					Journal = deliveryScheduleJournal;
					break;
				case RoboatsEntityType.Streets:
					var streetJournal = _roboatsJournalsFactory.CreateStreetJournal();
					streetJournal.SetForRoboatsCatalogExport(_openDialogCommand);
					streetJournal.OnSelectResult += OnStreetJournalSelected;
					Journal = streetJournal;
					break;
				case RoboatsEntityType.WaterTypes:
					var waterTypeJournal = _roboatsJournalsFactory.CreateWaterJournal();
					waterTypeJournal.OnSelectResult += OnWaterJournalSelected;
					Journal = waterTypeJournal;
					break;
				case RoboatsEntityType.CounterpartyName:
					var nameJournal = _roboatsJournalsFactory.CreateCounterpartyNameJournal();
					nameJournal.SetForRoboatsCatalogExport(_openDialogCommand);
					nameJournal.OnSelectResult += OnCounterpartyNameJournalSelected;
					Journal = nameJournal;
					break;
				case RoboatsEntityType.CounterpartyPatronymic:
					var patronymicJournal = _roboatsJournalsFactory.CreateCounterpartyPatronymicJournal();
					patronymicJournal.SetForRoboatsCatalogExport(_openDialogCommand);
					patronymicJournal.OnSelectResult += OnCounterpartyPatronymicJournalSelected;
					Journal = patronymicJournal;
					break;
				default:
					Journal = null;
					break;
			}
		}

		private void OnStreetJournalSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.GetSelectedObjects<RoboatsStreetJournalNode>().SingleOrDefault();
			if(selectedNode == null)
			{
				return;
			}

			var viewModel = new RoboatsStreetViewModel(EntityUoWBuilder.ForOpen(selectedNode.Id), _roboatsViewModelFactory, _unitOfWorkFactory, _commonServices);
			OpenDialog(viewModel);
		}

		private void OnWaterJournalSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.GetSelectedObjects<RoboatsWaterTypeJournalNode>().SingleOrDefault();

			if(selectedNode == null)
			{
				return;
			}

			var viewModel = new RoboatsWaterTypeViewModel(EntityUoWBuilder.ForOpen(selectedNode.Id), _unitOfWorkFactory, _roboatsViewModelFactory, _commonServices, NavigationManager);
			OpenDialog(viewModel);
		}

		private void OnCounterpartyNameJournalSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.GetSelectedObjects<RoboAtsCounterpartyNameJournalNode>().SingleOrDefault();
			if(selectedNode == null)
			{
				return;
			}
			var viewModel = new RoboAtsCounterpartyNameViewModel(EntityUoWBuilder.ForOpen(selectedNode.Id), _unitOfWorkFactory, _roboatsViewModelFactory, _commonServices);
			OpenDialog(viewModel);
		}

		private void OnCounterpartyPatronymicJournalSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.GetSelectedObjects<RoboAtsCounterpartyPatronymicJournalNode>().SingleOrDefault();
			if(selectedNode == null)
			{
				return;
			}
			var viewModel = new RoboAtsCounterpartyPatronymicViewModel(EntityUoWBuilder.ForOpen(selectedNode.Id), _unitOfWorkFactory, _roboatsViewModelFactory, _commonServices);
			OpenDialog(viewModel);
		}

		private void OnDeliveryScheduleJournalSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.GetSelectedObjects<DeliveryScheduleJournalNode>().SingleOrDefault();
			if(selectedNode == null)
			{
				return;
			}

			var viewModel = new DeliveryScheduleViewModel(EntityUoWBuilder.ForOpen(selectedNode.Id), _unitOfWorkFactory, _commonServices, _deliveryScheduleRepository, _roboatsViewModelFactory);
			OpenDialog(viewModel);
		}

		private void ViewModel_TabClosed(object sender, EventArgs e)
		{
			CloseDialog();
		}

		private void CloseDialog()
		{
			if(Dialog is IDisposable dlg)
			{
				dlg?.Dispose();
			}
			Dialog = null;
		}

		private void OpenDialog(TabViewModelBase viewModel)
		{
			CloseDialog();
			viewModel.TabClosed += ViewModel_TabClosed;
			Dialog = viewModel;
		}

		public DelegateCommand StartExport
		{
			get
			{
				if(_startExport != null)
				{
					return _startExport;
				}

				_startExport = new DelegateCommand(() =>
				{
					try
					{
						exportStarting = true;
						OnPropertyChanged(nameof(CanStartExport));
						Export();
					}
					finally
					{
						exportStarting = false;
						OnPropertyChanged(nameof(CanStartExport));
					}
				}, () => CanStartExport);
				_startExport.CanExecuteChangedWith(this, x => x.CanStartExport);

				return _startExport;
			}
		}

		private void Export()
		{
			var result = _fileDialogService.RunOpenDirectoryDialog();
			var exportDirectory = result.Path;
			if(!result.Successful)
			{
				return;
			}

			bool isExported = ExportCatalog(exportDirectory);
			if(isExported)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Выгрузка успешно завершена", "Выгрузка Roboats");
			}
		}

		public bool ExportCatalog(string exportDirectory)
		{
			if(!SelectedExportType.HasValue)
			{
				return false;
			}

			var storage = _roboatsFileStorageFactory.CreateStorage(SelectedExportType.Value);
			storage.Refresh();

			var exportedItems = _roboatsRepository.GetExportedEntities(SelectedExportType.Value);

			var incorrectFiles = new List<IRoboatsEntity>();
			var incorrectRoboatsId = new List<IRoboatsEntity>();

			foreach(var exportedItem in exportedItems)
			{
				if(!storage.FileExist(exportedItem.FileId.ToString()))
				{
					incorrectFiles.Add(exportedItem);
				}
			}

			string message = null;
			if(incorrectFiles.Any())
			{
				if(incorrectFiles.Count > 20)
				{
					message = $"Невозможно выгрузить справочник, так как отсутствуют файлы для выгрузки у более чем 20 элементов\n";
				}
				else
				{
					message = $"Невозможно выгрузить справочник, так как отсутствуют файлы для выгрузки у некоторых элементов:\n";
					message += string.Join(", ", incorrectFiles.Select(x => GetEntityTitle(x)));
				}
			}

			if(incorrectRoboatsId.Any())
			{
				if(incorrectFiles.Count > 20)
				{
					message = $"Невозможно выгрузить справочник, так как не установлен идентификатор для Roboats у более чем 20 элементов\n";
				}
				else
				{
					message += $"\nНевозможно выгрузить справочник, так как не установлен идентификатор для Roboats у некоторых элементов:\n";
					message += string.Join(", ", incorrectRoboatsId.Select(x => GetEntityTitle(x)));
				}
			}

			if(!string.IsNullOrWhiteSpace(message))
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, message, "Ошибка выгрузки");
				return false;
			}

			var audiofilesDirectory = Path.Combine(exportDirectory, "audiofiles");
			Directory.CreateDirectory(audiofilesDirectory);

			foreach(var exportedItem in exportedItems)
			{
				var filePath = Path.Combine(audiofilesDirectory, exportedItem.RoboatsAudiofile);
				if(!storage.TryTakeTo(exportedItem.FileId.ToString(), filePath, true))
				{
					return false;
				}
			}

			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Data");
				worksheet.Cell(1, 1).Value = "id";
				worksheet.Cell(1, 2).Value = "file";

				var exportedList = exportedItems.ToList();
				for(int i = 0; i < exportedList.Count; i++)
				{
					worksheet.Cell(i + 2, 1).Value = exportedList[i].RoboatsId;
					worksheet.Cell(i + 2, 2).Value = exportedList[i].RoboatsAudiofile;
				}
				var excelFilePath = Path.Combine(exportDirectory, $"_{SelectedExportType}.xlsx");
				workbook.SaveAs(excelFilePath);
			}

			return true;
		}

		private string GetEntityTitle(IRoboatsEntity roboatsEntity)
		{
			string title;
			switch(roboatsEntity.RoboatsEntityType)
			{
				case RoboatsEntityType.DeliverySchedules:
					var deliverySchedule = roboatsEntity as DeliverySchedule;
					title = $"{deliverySchedule.DeliveryTime} ({deliverySchedule.Id})";
					break;
				case RoboatsEntityType.Streets:
					var street = roboatsEntity as RoboatsStreet;
					title = $"{street.Type} {street.Name} ({street.Id})";
					break;
				case RoboatsEntityType.WaterTypes:
					var waterType = roboatsEntity as RoboatsWaterType;
					title = $"{waterType.Nomenclature} ({waterType.Id})";
					break;
				case RoboatsEntityType.CounterpartyName:
					var counterpartyName = roboatsEntity as RoboAtsCounterpartyName;
					title = $"{counterpartyName.Name} ({counterpartyName.Id})";
					break;
				case RoboatsEntityType.CounterpartyPatronymic:
					var counterpartyPatronymic = roboatsEntity as RoboAtsCounterpartyPatronymic;
					title = $"{counterpartyPatronymic.Patronymic} ({counterpartyPatronymic.Id})";
					break;
				default:
					throw new NotSupportedException($"Тип {roboatsEntity.RoboatsEntityType} не поддерживается");
			}
			return title;
		}
	}
}
