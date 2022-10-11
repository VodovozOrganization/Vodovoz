using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using Vodovoz.Domain.Cash;
using Vodovoz.EntityRepositories;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class CashlessRequestFilesViewModel : EntityWidgetViewModelBase<CashlessRequest>
	{
		private readonly IFileDialogService _fileDialogService;
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
