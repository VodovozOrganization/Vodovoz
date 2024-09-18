using Microsoft.Extensions.Options;
using NHibernate.Util;
using QS.Attachments;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Common;
using Vodovoz.EntityRepositories;
using Vodovoz.Presentation.ViewModels.Attributes;

namespace Vodovoz.Presentation.ViewModels.AttachedFiles
{
	[SkipWidgetRegistration]
	public class AttachedFileInformationsViewModel : WidgetViewModelBase
	{
		private const int _maxFileLength = 255;

		private readonly IOptions<FileSecurityOptions> _fileSecurityOptions;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IInteractiveService _interactiveService;
		private readonly IUserRepository _userRepository;
		private readonly IFileDialogService _fileDialogService;
		private readonly IScanDialogService _scanDialogService;

		private FileInformation _selectedFile;
		private IEnumerable<FileInformation> _fileInformations;

		private bool _readOnly;

		public AttachedFileInformationsViewModel(
			IOptions<FileSecurityOptions> fileSecurityOptions,
			IUnitOfWork unitOfWork,
			IInteractiveService interactiveService,
			IUserRepository userRepository,
			IFileDialogService fileDialogService,
			IScanDialogService scanDialogService)
		{
			_fileSecurityOptions = fileSecurityOptions
				?? throw new ArgumentNullException(nameof(fileSecurityOptions));
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));
			_userRepository = userRepository
				?? throw new ArgumentNullException(nameof(userRepository));
			_fileDialogService = fileDialogService
				?? throw new ArgumentNullException(nameof(fileDialogService));
			_scanDialogService = scanDialogService
				?? throw new ArgumentNullException(nameof(scanDialogService));

			AddCommand = new DelegateCommand(AddHandler, () => CanAdd);
			DeleteCommand = new DelegateCommand(DeleteHandler, () => CanDelete);
			DeleteCommand.CanExecuteChangedWith(this, vm => vm.CanDelete);

			ClearPersistentInformationCommand = new DelegateCommand(ClearPersistentInformation);

			OpenCommand = new DelegateCommand(OpenHandler, () => CanOpen);
			OpenCommand.CanExecuteChangedWith(this, vm => vm.CanOpen);
			SaveCommand = new DelegateCommand(SaveHandler, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, vm => vm.CanSave);

			ScanCommand = new DelegateCommand(ScanHandler);
		}

		public event EventHandler OnFileInformationChanged;

		public Dictionary<string, byte[]> AttachedFiles { get; } = new Dictionary<string, byte[]>();

		public List<string> FilesToAddOnSave { get; } = new List<string>();

		public List<string> FilesToUpdateOnSave { get; } = new List<string>();

		public List<string> FilesToDeleteOnSave { get; } = new List<string>();

		public List<string> FilesMissingOnStorage { get; } = new List<string>();

		public Action<string> AddFileCallback { get; set; }
		public Action<string> DeleteFileCallback { get; set; }

		[PropertyChangedAlso(
			nameof(CanAdd),
			nameof(CanDelete))]
		public virtual bool ReadOnly
		{
			get => _readOnly;
			set => SetField(ref _readOnly, value);
		}

		[PropertyChangedAlso(
			nameof(CanOpen),
			nameof(CanSave),
			nameof(CanDelete))]
		public FileInformation SelectedFile
		{
			get => _selectedFile;
			set => SetField(ref _selectedFile, value);
		}

		public IEnumerable<FileInformation> FileInformations
		{
			get => _fileInformations;
			set
			{
				var oldEnumerable = _fileInformations;

				if(SetField(ref _fileInformations, value))
				{
					if(oldEnumerable is INotifyCollectionChanged oldNotifyCollectionChanged)
					{
						oldNotifyCollectionChanged.CollectionChanged -= OnFileInformationCollectionChanged;
					}

					if(_fileInformations is INotifyCollectionChanged newNotifyCollectionChanged)
					{
						newNotifyCollectionChanged.CollectionChanged += OnFileInformationCollectionChanged;
					}

					if(oldEnumerable is INotifyCollectionElementChanged oldNotifyCollectionElementChanged)
					{
						oldNotifyCollectionElementChanged.ContentChanged -= OnFileInformationContentChanged;
						oldNotifyCollectionElementChanged.PropertyOfElementChanged -= OnFileInformationPropertyOfElementChanged;
					}

					if(_fileInformations is INotifyCollectionElementChanged newNotifyCollectionElementChanged)
					{
						newNotifyCollectionElementChanged.ContentChanged += OnFileInformationContentChanged;
						newNotifyCollectionElementChanged.PropertyOfElementChanged += OnFileInformationPropertyOfElementChanged;
					}
				}
			}
		}

		public bool CanAdd => !ReadOnly;

		public bool CanOpen => SelectedFile != null
			&& !FilesMissingOnStorage.Contains(SelectedFile.FileName)
			&& !_fileSecurityOptions.Value.RestrictedToOpenExtensions.Any(extension => Path.GetExtension(SelectedFile.FileName).EndsWith(extension));

		public bool CanSave => SelectedFile != null
			&& !FilesMissingOnStorage.Contains(SelectedFile.FileName);

		public bool CanDelete => SelectedFile != null && !ReadOnly;

		#region Commands

		public DelegateCommand AddCommand { get; }
		public DelegateCommand DeleteCommand { get; }
		public DelegateCommand ClearPersistentInformationCommand { get; }
		public DelegateCommand OpenCommand { get; }
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand ScanCommand { get; }

		#endregion Commands

		public void InitializeLoadedFiles(Dictionary<string, byte[]> loadedFiles)
		{
			foreach(var file in loadedFiles)
			{
				AttachedFiles.Add(file.Key, file.Value);
			}

			FilesMissingOnStorage.AddRange(
				FileInformations
					.Where(fi => !AttachedFiles.ContainsKey(fi.FileName))
					.Select(fi => fi.FileName));
		}

		#region Command Handlers

		private void AddHandler()
		{
			var result = _fileDialogService.RunOpenFileDialog();

			if(!result.Successful)
			{
				return;
			}

			foreach(var filePath in result.Paths)
			{
				AddAttachmentFile(filePath);
			}
		}

		private void AddAttachmentFile(string filePath)
		{
			try
			{
				var fileName = Path.GetFileNameWithoutExtension(filePath) +
					" " +
					DateTimeOffset.UtcNow.ToUnixTimeSeconds() +
					Path.GetExtension(filePath);

				if(fileName.Length > _maxFileLength)
				{
					_interactiveService.ShowMessage(
						ImportanceLevel.Warning,
						$"Слишком длинное имя файла: {fileName} " +
						$"({fileName.Length} символов).\n" +
						$"Оно не должно превышать {_maxFileLength} символов, включая расширение (.txt, .png и т.д.).");

					return;
				}

				var fileContent = File.ReadAllBytes(filePath);

				if(!AttachedFiles.ContainsKey(fileName))
				{
					AttachedFiles.Add(fileName, fileContent);
					FilesToAddOnSave.Add(fileName);
				}
				else if(FilesToDeleteOnSave.Contains(fileName))
				{
					AttachedFiles[fileName] = fileContent;
					FilesToDeleteOnSave.Remove(fileName);
					FilesToUpdateOnSave.Add(fileName);
				}

				if(!FileInformations.Any(fi => fi.FileName == fileName))
				{
					AddFileCallback?.Invoke(fileName);
				}
			}
			catch(UnauthorizedAccessException)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Программа не смогла получить доступ к указанному файлу",
					"Ошибка открытия файла");
			}
			catch(IOException ex)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					ex.Message,
					"Ошибка открытия файла");
			}
		}

		private void DeleteHandler()
		{
			if(FilesToDeleteOnSave.Contains(SelectedFile.FileName))
			{
				return;
			}

			if(FilesToUpdateOnSave.Contains(SelectedFile.FileName))
			{
				FilesToUpdateOnSave.Remove(SelectedFile.FileName);
			}

			if(FilesToAddOnSave.Contains(SelectedFile.FileName))
			{
				FilesToAddOnSave.Remove(SelectedFile.FileName);
			}

			FilesToDeleteOnSave.Add(SelectedFile.FileName);

			DeleteFileCallback?.Invoke(SelectedFile.FileName);
		}

		private void OpenHandler()
		{
			var vodovozUserTempDirectory = _userRepository.GetTempDirForCurrentUser(_unitOfWork);

			if(string.IsNullOrWhiteSpace(vodovozUserTempDirectory))
			{
				return;
			}

			var tempFilePath = Path.Combine(Path.GetTempPath(), vodovozUserTempDirectory, SelectedFile.FileName);

			if(!File.Exists(tempFilePath))
			{
				File.WriteAllBytes(tempFilePath, AttachedFiles[SelectedFile.FileName]);
			}

			var process = new Process
			{
				EnableRaisingEvents = true
			};

			process.StartInfo.FileName = Path.Combine(vodovozUserTempDirectory, SelectedFile.FileName);

			process.Exited += OnProcessExited;
			process.Start();
		}

		private void SaveHandler()
		{
			var extension = Path.GetExtension(SelectedFile.FileName);

			if(_fileSecurityOptions.Value.RestrictedToOpenExtensions.Any(e => extension.EndsWith(e))
				&& !_interactiveService.Question(
					"Файл является потенциально опасным для открытия, вы уверены, что хотите сохранить файл?",
					"Вы уверены?"))
			{
				return;
			}

			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				FileName = SelectedFile.FileName
			};

			dialogSettings.FileFilters.Add(new DialogFileFilter($"Файл {extension}", extension));

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(result.Successful)
			{
				File.WriteAllBytes(result.Path, AttachedFiles[SelectedFile.FileName]);
			}
		}

		private void ScanHandler()
		{
			if(_scanDialogService.GetFileFromDialog(out string fileName, out byte[] fileContent))
			{
				fileName = Path.GetFileNameWithoutExtension(fileName) +
					" " +
					DateTimeOffset.UtcNow.ToUnixTimeSeconds() +
					Path.GetExtension(fileName);

				if(!AttachedFiles.ContainsKey(fileName))
				{
					AttachedFiles.Add(fileName, fileContent);
					FilesToAddOnSave.Add(fileName);
				}
				else if(FilesToDeleteOnSave.Contains(fileName))
				{
					AttachedFiles[fileName] = fileContent;
					FilesToDeleteOnSave.Remove(fileName);
					FilesToUpdateOnSave.Add(fileName);
				}

				if(!FileInformations.Any(fi => fi.FileName == fileName))
				{
					AddFileCallback?.Invoke(fileName);
				}
			}
		}

		private void ClearPersistentInformation()
		{
			AttachedFiles.Clear();
			FilesToAddOnSave.Clear();
			FilesToUpdateOnSave.Clear();
			FilesToDeleteOnSave.Clear();
		}

		#endregion Command Handlers

		private void OnProcessExited(object sender, EventArgs e)
		{
			if(sender is Process process)
			{
				File.Delete(process.StartInfo.FileName);
				process.Exited -= OnProcessExited;
			}
		}

		#region FileInformations Changed Notification

		private void OnFileInformationCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			FireOnFileInformationChanged();
		}

		private void OnFileInformationPropertyOfElementChanged(object sender, PropertyChangedEventArgs e)
		{
			FireOnFileInformationChanged();
		}

		private void OnFileInformationContentChanged(object sender, EventArgs e)
		{
			FireOnFileInformationChanged();
		}

		private void FireOnFileInformationChanged()
		{
			OnFileInformationChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion FileInformations Changed Notification
	}
}
