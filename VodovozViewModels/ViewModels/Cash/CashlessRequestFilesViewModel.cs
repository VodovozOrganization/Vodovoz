using System;
using System.Diagnostics;
using System.IO;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class CashlessRequestFilesViewModel : EntityWidgetViewModelBase<CashlessRequest>
	{
		private readonly IFilePickerService _filePicker;
		private readonly IUserRepository _userRepository;
		private bool _readOnly;

		public virtual bool ReadOnly
		{
			get => _readOnly;
			set => SetField(ref _readOnly, value);
		}

		public CashlessRequestFilesViewModel(
			CashlessRequest entity,
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
						var cashlessRequestFile = new CashlessRequestFile
						{
							FileStorageId = Path.GetFileName(filePath)
						};

						if(cashlessRequestFile.FileStorageId.Length > 45)
						{
							CommonServices.InteractiveService.ShowMessage(
								ImportanceLevel.Warning,
								$"Слишком длинное имя файла: {cashlessRequestFile.FileStorageId} " +
								$"({cashlessRequestFile.FileStorageId.Length} символов).\n" +
								"Оно не должно превышать 45 символов, включая расширение (.txt, .png и т.д.).");
							continue;
						}

						cashlessRequestFile.ByteFile = File.ReadAllBytes(filePath);
						Entity.AddFile(cashlessRequestFile);
					}
				},
				() => !ReadOnly);
		}

		#endregion AddItemCommand

		#region DeleteItemCommand

		public DelegateCommand<CashlessRequestFile> DeleteItemCommand { get; private set; }

		private void CreateDeleteItemCommand()
		{
			DeleteItemCommand = new DelegateCommand<CashlessRequestFile>(
				(file) => Entity.RemoveFile(file),
				(file) => !ReadOnly
			);
		}

		#endregion DeleteItemCommand

		#region OpenItemCommand

		public DelegateCommand<CashlessRequestFile> OpenItemCommand { get; private set; }

		private void CreateOpenItemCommand()
		{
			OpenItemCommand = new DelegateCommand<CashlessRequestFile>(
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

		public DelegateCommand<CashlessRequestFile> LoadItemCommand { get; private set; }

		private void CreateLoadItemCommand()
		{
			LoadItemCommand = new DelegateCommand<CashlessRequestFile>(
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
