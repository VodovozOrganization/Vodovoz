using QS.Commands;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.IO;
using Vodovoz.Domain.Roboats;
using Vodovoz.Factories;
using Vodovoz.Models;

namespace Vodovoz.ViewModels.Dialogs.Roboats
{
	public class RoboatsEntityViewModel : WidgetViewModelBase
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly RoboatsFileStorage _roboatsFileStorage;

		private bool _deleteFileNeeded;
		private DelegateCommand _addAudioFileCommand;
		private DelegateCommand _deleteAudioFileCommand;
		private DelegateCommand _rollbackAudioFileCommand;
		private string _audioFileWarningMessage;
		private readonly bool _canEdit;
		public IRoboatsEntity Entity { get; }

		public RoboatsEntityViewModel(
			IRoboatsEntity entity,
			RoboatsFileStorageFactory roboatsFileStorageFactory,
			IFileDialogService fileDialogService,
			ICurrentPermissionService permissionService
		)
		{
			if(roboatsFileStorageFactory is null)
			{
				throw new ArgumentNullException(nameof(roboatsFileStorageFactory));
			}
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			Entity = entity ?? throw new ArgumentNullException(nameof(entity));
			_roboatsFileStorage = roboatsFileStorageFactory.CreateStorage(Entity.RoboatsEntityType);
			_roboatsFileStorage.FilesRefreshed += RoboatsFileStorage_FilesRefreshed;
			_roboatsFileStorage.Refresh();

			_canEdit = permissionService.ValidatePresetPermission("can_edit_roboats_catalogs");
		}

		public bool CanDeleteAudioFile => !string.IsNullOrWhiteSpace(Entity.RoboatsAudiofile) && CanEdit;
		public bool CanRollbackAudioFile => !string.IsNullOrWhiteSpace(Entity.NewRoboatsAudiofile) && CanEdit;
		public bool CanEdit => _canEdit;

		public string AudioFile
		{
			get
			{
				if(string.IsNullOrWhiteSpace(Entity.NewRoboatsAudiofile))
				{
					return Entity.RoboatsAudiofile;
				}
				return Path.GetFileName(Entity.NewRoboatsAudiofile);
			}
		}

		public string AudioFileWarningMessage
		{
			get => _audioFileWarningMessage;
			set => SetField(ref _audioFileWarningMessage, value);
		}

		public DelegateCommand AddAudioFileCommand
		{
			get
			{
				if(_addAudioFileCommand == null)
				{
					_addAudioFileCommand = new DelegateCommand(AddAudioFile, () => CanEdit);
					_addAudioFileCommand.CanExecuteChangedWith(this, x => x.CanEdit);
				}
				return _addAudioFileCommand;
			}
		}

		public DelegateCommand DeleteAudioFileCommand
		{
			get
			{
				if(_deleteAudioFileCommand == null)
				{
					_deleteAudioFileCommand = new DelegateCommand(DeleteAudioFile, () => CanDeleteAudioFile);
					_deleteAudioFileCommand.CanExecuteChangedWith(this, x => x.CanDeleteAudioFile);
				}
				return _deleteAudioFileCommand;
			}
		}

		public DelegateCommand RollbackAudioFileCommand
		{
			get
			{
				if(_rollbackAudioFileCommand == null)
				{
					_rollbackAudioFileCommand = new DelegateCommand(RollbackAudioFile, () => CanRollbackAudioFile);
					_rollbackAudioFileCommand.CanExecuteChangedWith(this, x => x.CanRollbackAudioFile);
				}
				return _rollbackAudioFileCommand;
			}
		}

		private void AddAudioFile()
		{
			DialogSettings dialogSettings = new DialogSettings();
			dialogSettings.Title = "Выберите аудиозапись";
			dialogSettings.PlatformType = DialogPlatformType.Auto;
			dialogSettings.FileFilters.Add(new DialogFileFilter("Аудиофайлы", "*.mp3", "*.wav"));
			dialogSettings.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

			var result = _fileDialogService.RunOpenFileDialog(dialogSettings);
			if(!result.Successful)
			{
				return;
			}

			SetNewAudioFilePath(result.Path);
			OnPropertyChanged(nameof(AudioFile));
		}

		private void DeleteAudioFile()
		{
			_deleteFileNeeded = true;
			Entity.RoboatsAudiofile = null;
			SetNewAudioFilePath(null);
		}

		private void RollbackAudioFile()
		{
			SetNewAudioFilePath(null);
		}

		public void SetNewAudioFilePath(string path)
		{
			Entity.NewRoboatsAudiofile = path;
			_deleteFileNeeded = true;
			OnPropertyChanged(nameof(AudioFile));
			OnPropertyChanged(nameof(CanDeleteAudioFile));
			OnPropertyChanged(nameof(CanRollbackAudioFile));
		}

		public bool Save()
		{
			if(!TryCommitFiles())
			{
				return false;
			}

			return true;
		}

		private bool TryCommitFiles()
		{
			var saved = true;
			var deleted = true;
			Guid newFileGuid = Guid.NewGuid();
			Guid? oldFileGuid = Entity.FileId;

			if(!string.IsNullOrWhiteSpace(Entity.NewRoboatsAudiofile))
			{
				saved = _roboatsFileStorage.TryPut(Entity.NewRoboatsAudiofile, newFileGuid.ToString(), true);
				if(saved)
				{
					Entity.FileId = newFileGuid;
					Entity.RoboatsAudiofile = Path.GetFileName(Entity.NewRoboatsAudiofile);
				}
				else
				{
					return false;
				}
			}

			if(_deleteFileNeeded && oldFileGuid.HasValue)
			{
				deleted = _roboatsFileStorage.TryDelete(oldFileGuid.Value.ToString());
			}

			return saved && deleted;
		}

		private void RoboatsFileStorage_FilesRefreshed(object sender, EventArgs e)
		{
			ValidateSavedAudioFile();
		}

		private void ValidateSavedAudioFile()
		{
			var savedfileExists = _roboatsFileStorage.FileExist(Entity.FileId?.ToString());
			if(!savedfileExists && !string.IsNullOrWhiteSpace(Entity.RoboatsAudiofile) && string.IsNullOrWhiteSpace(Entity.NewRoboatsAudiofile))
			{
				AudioFileWarningMessage = $"Файл {Entity.RoboatsAudiofile} отсутствует в хранилище или к нему нет доступа.";
			}
			else
			{
				AudioFileWarningMessage = null;
			}
		}
	}
}
