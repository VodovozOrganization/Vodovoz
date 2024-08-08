using QS.Commands;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using VodovozBusiness.Domain.Common;

namespace Vodovoz.Presentation.ViewModels
{
	public class AttachedFileInformationsViewModel : WidgetViewModelBase
	{
		private FileInformation _selectedFile;
		private IEnumerable<FileInformation> _fileInformations;
		private readonly IFileDialogService _fileDialogService;

		public event EventHandler OnFileInformationChanged;
		public Action<string, Stream> AddFileHandler { get; set; }
		public Action<string> DeleteFileHandler { get; set; }

		public AttachedFileInformationsViewModel(IFileDialogService fileDialogService)
		{
			AddCommand = new DelegateCommand(AddHandler);
			DeleteCommand = new DelegateCommand(DeleteHandler, () => CanDelete);
			DeleteCommand.CanExecuteChangedWith(this, vm => vm.CanDelete);

			OpenCommand = new DelegateCommand(OpenHandler, () => CanOpen);
			OpenCommand.CanExecuteChangedWith(this, vm => vm.CanOpen);
			SaveCommand = new DelegateCommand(SaveHandler, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, vm => vm.CanSave);

			ScanCommand = new DelegateCommand(ScanHandler);
			_fileDialogService = fileDialogService;
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
				FileStream fileStream = new FileStream(result.Path, FileMode.Open);
				AddFileHandler.Invoke(
					result.Path.Substring(
						result.Path.LastIndexOf(Path.DirectorySeparatorChar))
					.Trim(Path.DirectorySeparatorChar), fileStream);
			}
		}

		private void DeleteHandler()
		{
			DeleteFileHandler.Invoke(SelectedFile.FileName);
		}

		private void OpenHandler()
		{
			throw new NotImplementedException();
		}

		private void SaveHandler()
		{
			throw new NotImplementedException();
		}

		private void ScanHandler()
		{
			throw new NotImplementedException();
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
	}
}
