using System.Linq;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using QS.Commands;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using System.Diagnostics;

namespace Vodovoz.ViewModels
{
	public class FilesViewModel : WidgetViewModelBase
	{
		private bool IsFileOpen { get; set; }

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

		private string tempFilePath;
		public virtual string TempFilePath {
			get => tempFilePath;
			set => SetField(ref tempFilePath, value, () => TempFilePath);
		}

		#region Commands

		public DelegateCommand<string> AddItemCommand { get; private set; }
		public DelegateCommand<ComplaintFile> OpenItemCommand { get; private set; }
		public DelegateCommand<ComplaintFile> DeleteItemCommand { get; private set; }

		private void CreateCommands()
		{
			AddItemCommand = new DelegateCommand<string>(
				(string filePath) => {

					var byteFile = new List<byte>();

					var complaintFile = new ComplaintFile();
					complaintFile.FileStorageId = Path.GetFileName(filePath);

					using(BinaryReader sr = new BinaryReader(File.Open(filePath, FileMode.Open))) {
						while(sr.PeekChar() > -1)
							byteFile.Add(sr.ReadByte());

					}

					complaintFile.ByteFile = byteFile.ToArray();

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
				(file) => 
				{
					TempFilePath = Path.Combine(Path.GetTempPath(),file.FileStorageId);
					File.WriteAllBytes(TempFilePath, file.ByteFile);

					var process = new Process();
					process.StartInfo.FileName = Path.Combine(Path.GetTempPath(), file.FileStorageId);
					process.EnableRaisingEvents = true;
					process.Exited += (sender, e) => DeleteTempFiles();
					IsFileOpen = true;
					process.Start();
				},
				(file) => { return !IsFileOpen; });
		}

		#endregion Commands

		public FilesViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
			CreateCommands();
		}

		public void DeleteTempFiles()
		{
			if(File.Exists(TempFilePath)) 
			{
				//Снимаем установленный атрибут только на чтение если есть.
				File.SetAttributes(TempFilePath, FileAttributes.Normal);
				File.Delete(TempFilePath);
			}
			tempFilePath = string.Empty;
			IsFileOpen = false;
		}

	}
}
