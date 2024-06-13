using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using QS.Commands;
using QS.Dialog;
using QS.ViewModels.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Contacts;

namespace Vodovoz.ViewModels.Reports
{
	public class IncomingCallsAnalysisReportViewModel : UowDialogViewModelBase
	{
		#region Help

		private const string _help = "<b>Загрузка файла</b>: " +
									"Для выгрузки данных, необходимо выбрать файл и нажать \"Прочитать данные\"\n" +
									"Загружать можно только файлы с расширением .csv и\n" +
									"он должен содержать только номера телефонов в первом столбце\n" +
									"Стандартный файл Экселевских форматов .xls и .xlsx можно конвертировать в .csv\n" +
									"через пункт меню в Экселе Сохранить как, выбрав нужный формат\n\n" +
									"<b>Экспорт данных</b>:\n" +
									"Производится по кнопке \"Экспорт\"\n" +
									"Сохраняются данные также в .csv формате";

		#endregion
		
		private const string _csvFileFilter = "Comma Separated Values File (*.csv)";
		private const string _noFileSelected = "Файл не выбран";
		private const string _doneProgress = "Готово";
		private const string _errorProgress = "При загрузке данных произошла ошибка";
		private readonly IInteractiveService _interactiveService;
		private readonly IPhoneRepository _phoneRepository;
		private readonly IFileDialogService _fileDialogService;
		private readonly DialogSettings _exportDialogSettings;
		private readonly DialogSettings _loadDialogSettings;
		private DelegateCommand _chooseFileCommand;
		private DelegateCommand _createReportCommand;
		private DelegateCommand _exportCommand = null;
		private DelegateCommand _helpCommand = null;
		private bool _canCreateReport;
		private string _selectedFilePath;
		private string _selectedFileTitle = "Выбрать файл";
		private string _progressTitle;

		public IncomingCallsAnalysisReportViewModel(
			IUnitOfWorkFactory uowFactory,
			INavigationManager navigationManager,
			IInteractiveService interactiveService,
			IPhoneRepository phoneRepository,
			IFileDialogService fileDialogService)
			: base(uowFactory, navigationManager)
		{
			Title = "Анализ входящих звонков";
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			_exportDialogSettings = CreateExportDialogSettings();
			_loadDialogSettings = CreateLoadDialogSettings();
		}

		public GenericObservableList<IncomingCallsAnalysisReportNode> Nodes { get; } =
			new GenericObservableList<IncomingCallsAnalysisReportNode>();
		
		public string LoadingProgress => "Загружаем данные...";

		public bool IsLoadingData { get; set; }

		public bool CanCreateReport
		{
			get => _canCreateReport;
			set => SetField(ref _canCreateReport, value);
		}
		
		public string SelectedFileTitle
		{
			get => _selectedFileTitle;
			set => SetField(ref _selectedFileTitle, value);
		}
		
		public string ProgressTitle
		{
			get => _progressTitle;
			set => SetField(ref _progressTitle, value);
		}

		public bool CanExport => !IsLoadingData && Nodes.Any();

		public DelegateCommand ChooseFileCommand => _chooseFileCommand ?? (_chooseFileCommand = new DelegateCommand(
				() =>
				{
					var result = _fileDialogService.RunOpenFileDialog(_loadDialogSettings);

					if(!result.Successful)
					{
						UpdateCreateReportState(false, _noFileSelected);
						return;
					}

					_selectedFilePath = result.Path;
					if(string.IsNullOrWhiteSpace(_selectedFilePath))
					{
						_interactiveService.ShowMessage(ImportanceLevel.Warning, _noFileSelected);
						UpdateCreateReportState(false, _noFileSelected);
						return;
					}
				
					UpdateCreateReportState(true, _selectedFilePath);
				})
			);

		public DelegateCommand CreateReportCommand => _createReportCommand ?? (_createReportCommand = new DelegateCommand(
			() =>
			{
				try
				{
					_incomingCallsNumbers = File.ReadAllLines(_selectedFilePath)
						.Select(x => x.TrimStart('+', '7'));

					CreateReport();
					ProgressTitle = _doneProgress;
				}
				catch
				{
					ProgressTitle = _errorProgress;
					throw;
				}
				finally
				{
					IsLoadingData = false;
					OnPropertyChanged(nameof(CanExport));
				}
			})
		);
		
		public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(
			() =>
			{
				var result = _fileDialogService.RunSaveFileDialog(_exportDialogSettings);

				if(!result.Successful)
				{
					return;
				}

				if(string.IsNullOrWhiteSpace(result.Path))
				{
					throw new InvalidOperationException($"Не был заполнен путь выгрузки: {nameof(result.Path)}");
				}

				var exportString = GetCsvString(Nodes);
				try
				{
					File.WriteAllText(result.Path, exportString, Encoding.UTF8);
				}
				catch(IOException)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error,
						"Не удалось сохранить файл выгрузки. Возможно не закрыт предыдущий файл выгрузки", "Ошибка");
				}
			})
		);
		
		public DelegateCommand HelpCommand => _helpCommand ?? (_helpCommand = new DelegateCommand(
			() =>
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, _help);
			})
		);
		
		private IEnumerable<string> _incomingCallsNumbers { get; set; }

		public string ReturnStringFromNullableParameter<TValue>(TValue? parameter)
			where TValue : struct
		{
			switch(parameter)
			{
				case null:
					return string.Empty;
				case DateTime date:
					return date.ToShortDateString();
				default:
					return parameter.ToString();
			}
		}
		
		private void CreateReport()
		{
			Nodes.Clear();
			var newData = _phoneRepository.GetLastOrderIdAndDeliveryDateByPhone(UoW, _incomingCallsNumbers);

			foreach(var data in newData)
			{
				Nodes.Add(data);
			}
		}

		private string GetCsvString(IEnumerable<IncomingCallsAnalysisReportNode> nodes)
		{
			var sb = new StringBuilder();
			sb.AppendLine("Номер телефона;Id клиента;Id точки доставки;№ последнего заказа;Дата последнего заказа");

			foreach(var node in nodes)
			{
				var counterpartyId = ReturnStringFromNullableParameter(node.CounterpartyId);
				var dpId = ReturnStringFromNullableParameter(node.DeliveryPointId);
				var lastOrderId = ReturnStringFromNullableParameter(node.LastOrderId);
				var lastOrderDeliveryDate = ReturnStringFromNullableParameter(node.LastOrderDeliveryDate);
				sb.AppendLine($"{node.PhoneDigitsNumber};{counterpartyId};{dpId};{lastOrderId};{lastOrderDeliveryDate}");
			}

			return sb.ToString();
		}

		private DialogSettings CreateExportDialogSettings()
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				FileName = $"Анализ входящих звонков {DateTime.Today:dd-MM-yyyy}"
			};
			dialogSettings.FileFilters.Add(new DialogFileFilter(_csvFileFilter, "*.csv"));
			
			return dialogSettings;
		}
		
		private DialogSettings CreateLoadDialogSettings()
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Открыть",
			};
			dialogSettings.FileFilters.Add(new DialogFileFilter(_csvFileFilter, "*.csv"));
			
			return dialogSettings;
		}
		
		private void UpdateCreateReportState(bool canCreateReport, string title)
		{
			CanCreateReport = canCreateReport;
			SelectedFileTitle = title;
		}
	}
}
