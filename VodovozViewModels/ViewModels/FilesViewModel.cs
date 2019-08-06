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

namespace Vodovoz.ViewModels
{
	public class FilesViewModel : UoWWidgetViewModelBase
	{
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

		public DelegateCommand<string> AddItemCommand { get; private set; }
		public DelegateCommand<ComplaintFile> OpenItemCommand { get; private set; }
		public DelegateCommand<ComplaintFile> DeleteItemCommand { get; private set; }

		public FilesViewModel(IInteractiveService interactiveService, IUnitOfWork uow) : base(interactiveService)
		{
			UoW = uow;
			CreateCommands();
		}

		private void CreateCommands()
		{
			AddItemCommand = new DelegateCommand<string>(
				(string filePath) => {

					var complaintFile = new ComplaintFile();
					complaintFile.FileStorageId = Path.GetFileName(filePath);

					complaintFile.ByteFile = File.ReadAllBytes(filePath);

					if(FilesList == null)
						FilesList = new GenericObservableList<ComplaintFile>();
					FilesList.Add(complaintFile);
				},
				(string file) => { return !ReadOnly; }
			);

			DeleteItemCommand = new DelegateCommand<ComplaintFile>(
				(file) => {
					FilesList.Remove(file);
				},
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
		}

		#endregion Commands
	}
}
