using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.PrintableDocuments;

namespace Vodovoz.ViewModels.Widgets.Print
{
	public class DocumentsPrinterSettingsViewModel : WidgetViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private UserSettings _userSettings;
		private CustomPrinterPrintDocumentType? _selectedDocumentType;
		private DocumentPrinterSetting _selectedPrinterSetting;

		public DocumentsPrinterSettingsViewModel(ICommonServices commonServices)
		{
			if(commonServices is null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			_interactiveService = commonServices.InteractiveService;

			AddPrinterSettingCommand = new DelegateCommand(AddPrinterSetting, () => CanAddPrinterSetting);
			ConfigurePrinterSettingCommand = new DelegateCommand(ConfigurePrinterSetting, () => CanConfigurePrinterSetting);
			RemovePrinterSettingCommand = new DelegateCommand(RemovePrinterSetting, () => CanRemovePrinterSetting);
		}

		public DelegateCommand AddPrinterSettingCommand { get; }
		public DelegateCommand ConfigurePrinterSettingCommand { get; }
		public DelegateCommand RemovePrinterSettingCommand { get; }

		public GenericObservableList<DocumentPrinterSetting> ObservablePrinterSettings =>
			UserSettings?.ObservableDocumentPrinterSettings ?? new GenericObservableList<DocumentPrinterSetting>(new List<DocumentPrinterSetting>());

		public UserSettings UserSettings
		{
			get => _userSettings;
			set
			{
				if(!(_userSettings is null))
				{
					throw new InvalidOperationException($"Свойство {nameof(UserSettings)} уже установлено");
				}

				SetField(ref _userSettings, value);
			}
		}

		[PropertyChangedAlso(nameof(CanAddPrinterSetting))]
		public CustomPrinterPrintDocumentType? SelectedDocumentType
		{
			get => _selectedDocumentType;
			set => SetField(ref _selectedDocumentType, value);
		}

		[PropertyChangedAlso(
			nameof(CanConfigurePrinterSetting),
			nameof(CanRemovePrinterSetting))]
		public DocumentPrinterSetting SelectedPrinterSetting
		{
			get => _selectedPrinterSetting;
			set => SetField(ref _selectedPrinterSetting, value);
		}

		public bool CanAddPrinterSetting =>
			!(SelectedDocumentType is null)
			&& ObservablePrinterSettings.All(s => s.DocumentType != SelectedDocumentType);

		public bool CanConfigurePrinterSetting => !(SelectedPrinterSetting is null);
		public bool CanRemovePrinterSetting => !(SelectedPrinterSetting is null);

		private void AddPrinterSetting()
		{
			if(!CanAddPrinterSetting)
			{
				return;
			}

			var newPrinterSetting = new DocumentPrinterSetting
			{
				UserSettings = UserSettings,
				DocumentType = SelectedDocumentType.Value,
				PrinterName = "NewPrinter"
			};

			UserSettings.ObservableDocumentPrinterSettings.Add(newPrinterSetting);
			SelectedDocumentType = null;
		}

		private void ConfigurePrinterSetting()
		{
			if(!CanConfigurePrinterSetting)
			{
				return;
			}
		}

		private void RemovePrinterSetting()
		{
			if(!CanRemovePrinterSetting)
			{
				return;
			}

			UserSettings.ObservableDocumentPrinterSettings.Remove(SelectedPrinterSetting);
			SelectedDocumentType = null;
		}
	}
}
