using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;

namespace Vodovoz.ViewModels.Dialogs.Counterparties
{
	public class CounterpartyFilesViewModel : EntityWidgetViewModelBase<Domain.Client.Counterparty>
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly IUserRepository _userRepository;
		private bool _readOnly;

		public virtual bool ReadOnly
		{
			get => _readOnly;
			set => SetField(ref _readOnly, value, () => ReadOnly);
		}

		public CounterpartyFilesViewModel(
			Domain.Client.Counterparty entity,
			IUnitOfWork uow,
			IFileDialogService fileDialogService,
			ICommonServices commonServices,
			IUserRepository userRepository)
			: base(entity, commonServices)
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
					var dialogSettings = new DialogSettings();
					dialogSettings.SelectMultiple = false;
					dialogSettings.Title = "Открыть";
					dialogSettings.FileFilters.Add(new DialogFileFilter("Все файлы (*.*)", "*.*"));

					var result = _fileDialogService.RunOpenFileDialog(dialogSettings);
					if(result.Successful)
					{
						var counterpartyFile = new CounterpartyFile
						{
							FileStorageId = Path.GetFileName(result.Path),
							ByteFile = File.ReadAllBytes(result.Path)
						};
						Entity.AddFile(counterpartyFile);
					}
				},
				() => !ReadOnly);
		}

		#endregion AddItemCommand

		#region DeleteItemCommand

		public DelegateCommand<CounterpartyFile> DeleteItemCommand { get; private set; }

		private void CreateDeleteItemCommand()
		{
			DeleteItemCommand = new DelegateCommand<CounterpartyFile>(
				(file) => Entity.RemoveFile(file),
				(file) => !ReadOnly
			);
		}

		#endregion DeleteItemCommand

		#region OpenItemCommand

		public DelegateCommand<CounterpartyFile> OpenItemCommand { get; private set; }

		private void CreateOpenItemCommand()
		{
			OpenItemCommand = new DelegateCommand<CounterpartyFile>(
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

		public DelegateCommand<CounterpartyFile> LoadItemCommand { get; private set; }

		private void CreateLoadItemCommand()
		{
			LoadItemCommand = new DelegateCommand<CounterpartyFile>(
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
