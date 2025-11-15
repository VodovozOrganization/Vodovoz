using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;

namespace Vodovoz.ViewModels.BaseParameters
{
	public class BaseParametersViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICommonServices _commonServices;
		private readonly ISettingsController _settingsController;
		public GenericObservableList<Setting> _settings;
		private List<Setting> _settingsToDelete = new List<Setting>();

		public BaseParametersViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ICommonServices commonServices,
			ISettingsController settingsController) : base(navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot();
			_settings = new GenericObservableList<Setting>(_unitOfWork.GetAll<Setting>().ToList());

			Title = "Параметры приложения";
		}

		public GenericObservableList<Setting> Settings => _settings;

		private Setting _selectedSetting;

		public Setting SelectedSetting
		{
			get => _selectedSetting;
			set => SetField(ref _selectedSetting, value);
		}

		private bool HasDuplicatedSettingsInList()
		{
			var duplicatedSettingNames = _settings
				.GroupBy(s => s.Name)
				.Where(sg => sg.Count() > 1)
				.Select(sg => sg.Key)
				.ToList();

			if(duplicatedSettingNames.Count != 0)
			{
				foreach(var settingName in duplicatedSettingNames)
				{
					_commonServices.InteractiveService.ShowMessage(
						QS.Dialog.ImportanceLevel.Warning,
						$"Параметр \"{settingName}\" добавлен несколько раз"
						);

					return true;
				}
			}

			return false;
		}

		#region Commands

		#region AddParameterCommand

		private DelegateCommand _addParameterCommand;
		public DelegateCommand AddParameterCommand
		{
			get
			{
				if(_addParameterCommand == null)
				{
					_addParameterCommand = new DelegateCommand(AddParameter, () => CanAddParameter);
					_addParameterCommand.CanExecuteChangedWith(this, x => x.CanAddParameter);
				}
				return _addParameterCommand;
			}
		}

		public bool CanAddParameter => true;

		private void AddParameter()
		{
			_settings.Add(new Setting { Name = "Новый параметр", StrValue = "Значение", Description = "Описание" });
		}
		#endregion

		#region RemoveParameterCommand
		private DelegateCommand _removeParameterCommand;
		public DelegateCommand RemoveParameterCommand
		{
			get
			{
				if(_removeParameterCommand == null)
				{
					_removeParameterCommand = new DelegateCommand(RemoveParameter, () => CanRemoveParameter);
					_removeParameterCommand.CanExecuteChangedWith(this, x => x.CanRemoveParameter);
				}
				return _removeParameterCommand;
			}
		}

		public bool CanRemoveParameter => true;

		private void RemoveParameter()
		{
			if(SelectedSetting == null)
			{
				return;
			}

			_settingsToDelete.Add(SelectedSetting);
			_settings.Remove(SelectedSetting);
		}
		#endregion

		#region SaveParametersCommand
		private DelegateCommand _saveParametersCommand;
		public DelegateCommand SaveParametersCommand
		{
			get
			{
				if(_saveParametersCommand == null)
				{
					_saveParametersCommand = new DelegateCommand(SaveParameters, () => CanSaveParameters);
					_saveParametersCommand.CanExecuteChangedWith(this, x => x.CanSaveParameters);
				}
				return _saveParametersCommand;
			}
		}

		public bool CanSaveParameters => true;

		private void SaveParameters()
		{
			if(HasDuplicatedSettingsInList())
			{
				return;
			}

			foreach(var setting in _settings)
			{
				_unitOfWork.Save(setting);
			}

			foreach(var setting in _settingsToDelete)
			{
				_unitOfWork.Delete(setting);
			}

			_unitOfWork.Commit();

			_settingsController.RefreshSettings();

			Close(false, CloseSource.Save);
		}
		#endregion

		#region CancelCommand
		private DelegateCommand _cancelCommand;
		public DelegateCommand CancelCommand
		{
			get
			{
				if(_cancelCommand == null)
				{
					_cancelCommand = new DelegateCommand(Cancel, () => CanCancel);
					_cancelCommand.CanExecuteChangedWith(this, x => x.CanCancel);
				}
				return _cancelCommand;
			}
		}

		public bool CanCancel => true;

		private void Cancel()
		{
			Close(false, CloseSource.Cancel);
		}

		#endregion

		#endregion

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
