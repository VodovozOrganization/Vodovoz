using System;
using System.IO;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Domain.Complaints;
using System.Diagnostics;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintFilesViewModel : EntityWidgetViewModelBase<Complaint>
	{
		private readonly IFilePickerService filePicker;
		private bool readOnly;

		public virtual bool ReadOnly {
			get => readOnly;
			set => SetField(ref readOnly, value, () => ReadOnly);
		}

		public ComplaintFilesViewModel(Complaint entity, IUnitOfWork uow, IFilePickerService filePicker, ICommonServices commonServices) : base(entity, commonServices)
		{
			this.filePicker = filePicker ?? throw new ArgumentNullException(nameof(filePicker));
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
				() => {

					if(filePicker.OpenSelectFilePicker(out string filePath)) {
						var complaintFile = new ComplaintFile();
						complaintFile.FileStorageId = Path.GetFileName(filePath);
						complaintFile.ByteFile = File.ReadAllBytes(filePath);
						Entity.AddFile(complaintFile);
					}
				},
				() => { return !ReadOnly; }
			);
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
				(file) => {

					var vodUserTempDir = UserRepository.GetTempDirForCurrentUser(UoW);

					if(string.IsNullOrWhiteSpace(vodUserTempDir))
						return;

					var tempFilePath = Path.Combine(Path.GetTempPath(), vodUserTempDir, file.FileStorageId);

					if(!File.Exists(tempFilePath))
						File.WriteAllBytes(tempFilePath, file.ByteFile);

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
				(file) => {
					if(filePicker.OpenSaveFilePicker(file.FileStorageId, out string filePath))
						File.WriteAllBytes(filePath, file.ByteFile);
				},
				(file) => { return !ReadOnly; }
			);
		}

		#endregion LoadItemCommand

		#endregion Commands
	}
}
