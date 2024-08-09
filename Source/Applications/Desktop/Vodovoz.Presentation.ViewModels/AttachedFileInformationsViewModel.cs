using NHibernate.Util;
using QS.Attachments;
using QS.Commands;
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
using Vodovoz.EntityRepositories;
using VodovozBusiness.Domain.Common;

namespace Vodovoz.Presentation.ViewModels
{
	public class AttachedFileInformationsViewModel : WidgetViewModelBase
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly IScanDialogService _scanDialogService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserRepository _userRepository;
		private FileInformation _selectedFile;
		private IEnumerable<FileInformation> _fileInformations;

		public AttachedFileInformationsViewModel(
			IUnitOfWork unitOfWork,
			IUserRepository userRepository,
			IFileDialogService fileDialogService,
			IScanDialogService scanDialogService)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_scanDialogService = scanDialogService ?? throw new ArgumentNullException(nameof(scanDialogService));

			AddCommand = new DelegateCommand(AddHandler);
			DeleteCommand = new DelegateCommand(DeleteHandler, () => CanDelete);
			DeleteCommand.CanExecuteChangedWith(this, vm => vm.CanDelete);

			OpenCommand = new DelegateCommand(OpenHandler, () => CanOpen);
			OpenCommand.CanExecuteChangedWith(this, vm => vm.CanOpen);
			SaveCommand = new DelegateCommand(SaveHandler, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, vm => vm.CanSave);

			ScanCommand = new DelegateCommand(ScanHandler);
		}

		public Dictionary<string, byte[]> AttachedFiles { get; } = new Dictionary<string, byte[]>();

		public List<string> FilesToAddOnSave { get; } = new List<string>();

		public List<string> FilesToUpdateOnSave { get; } = new List<string>();

		public List<string> FilesToDeleteOnSave { get; } = new List<string>();

		public event EventHandler OnFileInformationChanged;
		public Action<string> AddFileHandler { get; set; }
		public Action<string> DeleteFileHandler { get; set; }

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

					if(_fileInformations is INotifyCollectionElementChanged  newNotifyCollectionElementChanged)
					{
						newNotifyCollectionElementChanged.ContentChanged += OnFileInformationContentChanged;
						newNotifyCollectionElementChanged.PropertyOfElementChanged += OnFileInformationPropertyOfElementChanged;
					}
				}
			}
		}

		public bool CanOpen => SelectedFile != null;
		public bool CanSave => SelectedFile != null;
		public bool CanDelete => SelectedFile != null;

		#region Commands

		public DelegateCommand AddCommand { get; }
		public DelegateCommand DeleteCommand { get; }
		public DelegateCommand OpenCommand { get; }
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand ScanCommand { get; }

		#endregion Commands

		private void AddHandler()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.SelectMultiple = false;
			dialogSettings.Title = "Открыть";
			dialogSettings.FileFilters.Add(new DialogFileFilter("Все файлы (*.*)", "*.*"));

			var result = _fileDialogService.RunOpenFileDialog(dialogSettings);

			if(result.Successful && AddFileHandler != null)
			{
				using var fileStream = new FileStream(result.Path, FileMode.Open);

				var fileName = result.Path.Substring(
						result.Path.LastIndexOf(Path.DirectorySeparatorChar))
					.Trim(Path.DirectorySeparatorChar);

				using var ms = new MemoryStream();

				fileStream.CopyTo(ms);
				var fileContent = ms.ToArray();

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
					AddFileHandler.Invoke(fileName);
				}
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

			DeleteFileHandler.Invoke(SelectedFile.FileName);
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

			var process = new Process();
			process.StartInfo.FileName = Path.Combine(vodovozUserTempDirectory, SelectedFile.FileName);
			process.EnableRaisingEvents = true;

			process.Exited += OnProcessExited;
			process.Start();
		}

		private void OnProcessExited(object sender, EventArgs e)
		{
			if(sender is Process process)
			{
				File.Delete(process.StartInfo.FileName);
				process.Exited -= OnProcessExited;
			}
		}

		private void SaveHandler()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.FileName = SelectedFile.FileName;
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

				using var memoryStream = new MemoryStream(fileContent);

				if(!FileInformations.Any(fi => fi.FileName == fileName))
				{
					AddFileHandler.Invoke(fileName);
				}
			}
		}

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

		public void InitializeLoadedFiles(Dictionary<string, byte[]> loadedFiles)
		{
			foreach(var file in loadedFiles)
			{
				AttachedFiles.Add(file.Key, file.Value);
			}
		}
	}
}
