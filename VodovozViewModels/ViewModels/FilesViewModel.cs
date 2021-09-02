using System;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using QS.Commands;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using System.Diagnostics;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.EntityRepositories;

namespace Vodovoz.ViewModels
{
	public class FilesViewModel : UoWWidgetViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IUserRepository _userRepository;
		private readonly IFilePickerService _filePicker;
		private bool _readOnly;
		private GenericObservableList<ComplaintFile> _filesList;

		public virtual GenericObservableList<ComplaintFile> FilesList
		{
			get => _filesList;
			set => SetField(ref _filesList, value, () => FilesList);
		}
		
		public virtual bool ReadOnly
		{
			get => _readOnly;
			set => SetField(ref _readOnly, value);
		}

		#region Commands

		public DelegateCommand AddItemCommand { get; private set; }
		public DelegateCommand<ComplaintFile> OpenItemCommand { get; private set; }
		public DelegateCommand<ComplaintFile> DeleteItemCommand { get; private set; }
		public DelegateCommand<ComplaintFile> LoadItemCommand { get; private set; }

		public FilesViewModel(
			IFilePickerService filePicker,
			IInteractiveService interactiveService,
			IUnitOfWork uow,
			IUserRepository userRepository)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_filePicker = filePicker ?? throw new ArgumentNullException(nameof(filePicker));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			CreateCommands();
		}

		private void CreateCommands()
		{
			AddItemCommand = new DelegateCommand(
				() =>
				{
					if(!_filePicker.OpenSelectFilePicker(out string[] filePaths))
					{
						return;
					}

					foreach(var filePath in filePaths)
					{
						var complaintFile = new ComplaintFile
						{
							FileStorageId = Path.GetFileName(filePath)
						};

						if(complaintFile.FileStorageId.Length > 45)
						{
							_interactiveService.ShowMessage(
								ImportanceLevel.Warning,
								$"Слишком длинное имя файла: {complaintFile.FileStorageId} " +
								$"({complaintFile.FileStorageId.Length} символов).\n" +
								"Оно не должно превышать 45 символов, включая расширение (.txt, .png и т.д.).");
							continue;
						}

						complaintFile.ByteFile = File.ReadAllBytes(filePath);
						if(FilesList == null)
						{
							FilesList = new GenericObservableList<ComplaintFile>();
						}

						FilesList.Add(complaintFile);
					}
				},
				() => !ReadOnly);

			DeleteItemCommand = new DelegateCommand<ComplaintFile>(
				file => { FilesList.Remove(file); },
				file => !ReadOnly);

			OpenItemCommand = new DelegateCommand<ComplaintFile>(
				file =>
				{
					var vodUserTempDir = _userRepository.GetTempDirForCurrentUser(UoW);

					if(string.IsNullOrWhiteSpace(vodUserTempDir))
					{
						return;
					}

					var tempFilePath = Path.Combine(Path.GetTempPath(), vodUserTempDir, file.FileStorageId);

					if(!File.Exists(tempFilePath))
					{
						File.WriteAllBytes(tempFilePath, file.ByteFile);
					}

					var process = new Process();
					process.StartInfo.FileName = Path.Combine(vodUserTempDir, file.FileStorageId);
					process.Start();
				});

			LoadItemCommand = new DelegateCommand<ComplaintFile>(
				file =>
				{
					if(_filePicker.OpenSaveFilePicker(file.FileStorageId, out string filePath))
					{
						File.WriteAllBytes(filePath, file.ByteFile);
					}
				},
				file => !ReadOnly);
		}

		#endregion Commands
	}
}
