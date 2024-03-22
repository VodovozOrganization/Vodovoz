using System;
using System.IO;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using System.Diagnostics;
using QS.Dialog;
using Vodovoz.EntityRepositories;
using QS.Project.Services.FileDialog;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintFilesViewModel : EntityWidgetViewModelBase<Complaint>
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly IUserRepository _userRepository;
		private bool _readOnly;

		public virtual bool ReadOnly {
			get => _readOnly;
			set => SetField(ref _readOnly, value, () => ReadOnly);
		}

		public ComplaintFilesViewModel(
			Complaint entity,
			IUnitOfWork uow,
			IFileDialogService fileDialogService,
			ICommonServices commonServices,
			IUserRepository userRepository) : base(entity, commonServices)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			UoW = uow;
			CreateCommands();
		}

		#region Commands

		private void CreateCommands()
		{
			CreateAddItemCommand();
			CreateDeleteItemCommand();
			CreateOpenItemCommand();
			CreateLoadItemCommand();
		}

		#region AddItemCommand

		public DelegateCommand AddItemCommand { get; private set; }

		private void CreateAddItemCommand()
		{
			AddItemCommand = new DelegateCommand(
				() =>
				{
					var result = _fileDialogService.RunOpenFileDialog();
					if(!result.Successful)
					{
						return;
					}

					foreach(var filePath in result.Paths)
					{
						var complaintFile = new ComplaintFile
						{
							FileStorageId = Path.GetFileName(filePath)
						};

						if(complaintFile.FileStorageId.Length > 45)
						{
							CommonServices.InteractiveService.ShowMessage(
								ImportanceLevel.Warning,
								$"Слишком длинное имя файла: {complaintFile.FileStorageId} " +
								$"({complaintFile.FileStorageId.Length} символов).\n" +
								"Оно не должно превышать 45 символов, включая расширение (.txt, .png и т.д.).");
							continue;
						}

						complaintFile.ByteFile = File.ReadAllBytes(filePath);
						
						if(complaintFile.ByteFile.Length > 16_250_000)
						{
							CommonServices.InteractiveService.ShowMessage(
								ImportanceLevel.Warning,
								$"Слишком большой размер файла: {complaintFile.FileStorageId} ");
							continue;
						}
						
						Entity.AddFile(complaintFile);
					}
				},
				() => !ReadOnly);
		}

		#endregion AddItemCommand

		#region DeleteItemCommand

		public DelegateCommand<ComplaintFile> DeleteItemCommand { get; private set; }

		private void CreateDeleteItemCommand()
		{
			DeleteItemCommand = new DelegateCommand<ComplaintFile>(
				(file) => Entity.RemoveFile(file),
				(file) => !ReadOnly
			);
		}

		#endregion DeleteItemCommand

		#region OpenItemCommand

		public DelegateCommand<ComplaintFile> OpenItemCommand { get; private set; }

		private void CreateOpenItemCommand()
		{
			OpenItemCommand = new DelegateCommand<ComplaintFile>(
				(file) =>
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
		}

		#endregion OpenItemCommand

		#region LoadItemCommand

		public DelegateCommand<ComplaintFile> LoadItemCommand { get; private set; }

		private void CreateLoadItemCommand()
		{
			LoadItemCommand = new DelegateCommand<ComplaintFile>(
				(file) =>
				{
					var dialogSettings = new DialogSettings();
					dialogSettings.Title = "Сохранить";
					dialogSettings.FileName = file.FileStorageId;

					var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
					if(result.Successful)
					{
						File.WriteAllBytes(result.Path, file.ByteFile);
					}
				},
				(file) => !ReadOnly);
		}

		#endregion LoadItemCommand

		#endregion Commands
	}
}
