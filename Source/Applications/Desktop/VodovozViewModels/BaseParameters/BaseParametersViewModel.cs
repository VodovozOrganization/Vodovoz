using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;

namespace Vodovoz.ViewModels.BaseParameters
{
	public class BaseParametersViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ISettingsController _settingsController;
		private List<Setting> _settings = new List<Setting>();
		private List<Setting> _settingsToDelete = new List<Setting>();

		public BaseParametersViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ISettingsController settingsController) : base(navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot();
			_settings = _unitOfWork.GetAll<Setting>().ToList();

			Title = "Парметры приложения";
		}

		public List<Setting> Settings => _settings;

		private Setting _selectedSetting;

		public Setting SelectedSetting
		{
			get { return _selectedSetting; }
			set { _selectedSetting = value; }
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
			_settings.Add(new Setting { Name = "Новый параметр", StrValue = "Значение" });
			OnPropertyChanged(nameof(Settings));
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
			OnPropertyChanged(nameof(Settings));
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
			foreach(var setting in _settings)
			{
				_unitOfWork.Save(setting);
			}
			foreach(var setting in _settingsToDelete)
			{
				_unitOfWork.Delete(setting);
			}

			_unitOfWork.Commit();

			Close(false, CloseSource.Save);
		}
		#endregion

		#endregion

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
