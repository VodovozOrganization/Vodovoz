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

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintFilesViewModel : EntityWidgetViewModelBase<Complaint>
	{
		private readonly IFilePickerService _filePicker;
		private readonly IUserRepository _userRepository;
		private bool readOnly;

		public virtual bool ReadOnly {
			get => readOnly;
			set => SetField(ref readOnly, value, () => ReadOnly);
		}

		public ComplaintFilesViewModel(
			Complaint entity,
			IUnitOfWork uow,
			IFilePickerService filePicker,
			ICommonServices commonServices,
			IUserRepository userRepository) : base(entity, commonServices)
		{
			_filePicker = filePicker ?? throw new ArgumentNullException(nameof(filePicker));
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
							CommonServices.InteractiveService.ShowMessage(
								ImportanceLevel.Warning,
								$"Слишком длинное имя файла: {complaintFile.FileStorageId} " +
								$"({complaintFile.FileStorageId.Length} символов).\n" +
								"Оно не должно превышать 45 символов, включая расширение (.txt, .png и т.д.).");
							continue;
						}

						complaintFile.ByteFile = File.ReadAllBytes(filePath);
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
					if(_filePicker.OpenSaveFilePicker(file.FileStorageId, out string filePath))
					{
						File.WriteAllBytes(filePath, file.ByteFile);
					}
				},
				(file) => !ReadOnly);
		}

		#endregion LoadItemCommand

		#endregion Commands
	}
}
