using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using QS.Commands;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using System.Diagnostics;
using Vodovoz.Repositories.HumanResources;
using QS.DomainModel.UoW;
using QS.Project.Services;

namespace Vodovoz.ViewModels
{
	public class FilesViewModel : UoWWidgetViewModelBase
	{
		private IFilePickerService filePicker { get; }

		private GenericObservableList<ComplaintFile> filesList;
		public virtual GenericObservableList<ComplaintFile> FilesList {
			get => filesList;
			set => SetField(ref filesList, value, () => FilesList);
		}

		private bool readOnly;
		public virtual bool ReadOnly {
			get => readOnly;
			set => SetField(ref readOnly, value, () => ReadOnly);
		}

		#region Commands

		public DelegateCommand AddItemCommand { get; private set; }
		public DelegateCommand<ComplaintFile> OpenItemCommand { get; private set; }
		public DelegateCommand<ComplaintFile> DeleteItemCommand { get; private set; }
		public DelegateCommand<ComplaintFile> LoadItemCommand { get; private set; }

		public FilesViewModel(IInteractiveService interactiveService, IFilePickerService filePicker, IUnitOfWork uow) : base(interactiveService)
		{
			UoW = uow;
			this.filePicker = filePicker;
			CreateCommands();
		}

		private void CreateCommands()
		{
			AddItemCommand = new DelegateCommand(
				() => {

					if(filePicker.OpenSelectFilePicker(out string filePath)) 
					{
						var complaintFile = new ComplaintFile();
						complaintFile.FileStorageId = Path.GetFileName(filePath);

						complaintFile.ByteFile = File.ReadAllBytes(filePath);

						if(FilesList == null)
							FilesList = new GenericObservableList<ComplaintFile>();
						FilesList.Add(complaintFile);
					}
				},
				() => { return !ReadOnly; }
			);

			DeleteItemCommand = new DelegateCommand<ComplaintFile>(
				(file) => { FilesList.Remove(file); },
				(file) => { return !ReadOnly; }
			);

			OpenItemCommand = new DelegateCommand<ComplaintFile>(
				(file) => {

					var vodUserTempDir = UserRepository.GetTempDirForCurrentUser(UoW);

					if(string.IsNullOrWhiteSpace(vodUserTempDir))
						return;

					var tempFilePath = Path.Combine(Path.GetTempPath(),vodUserTempDir, file.FileStorageId);

					if(!File.Exists(tempFilePath))
						File.WriteAllBytes(tempFilePath, file.ByteFile);

					var process = new Process();
					process.StartInfo.FileName = Path.Combine(vodUserTempDir, file.FileStorageId);
					process.Start();
				});

			LoadItemCommand = new DelegateCommand<ComplaintFile>(
				(file) => {
					if(filePicker.OpenSaveFilePicker(file.FileStorageId, out string filePath))
						File.WriteAllBytes(filePath, file.ByteFile);
				},
				(file) => { return !ReadOnly; }
			);
		}

		#endregion Commands
	}
}
