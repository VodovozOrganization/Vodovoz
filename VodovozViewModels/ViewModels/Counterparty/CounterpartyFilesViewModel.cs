using System;
using System.Diagnostics;
using System.IO;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
    public class CounterpartyFilesViewModel : EntityWidgetViewModelBase<Domain.Client.Counterparty>
    {
        private readonly IFilePickerService filePicker;
        private readonly IUserRepository _userRepository;
        private bool readOnly;

        public virtual bool ReadOnly
        {
            get => readOnly;
            set => SetField(ref readOnly, value, () => ReadOnly);
        }

        public CounterpartyFilesViewModel(
	        Domain.Client.Counterparty entity,
	        IUnitOfWork uow,
	        IFilePickerService filePicker,
	        ICommonServices commonServices,
	        IUserRepository userRepository) 
            : base(entity, commonServices)
        {
            this.filePicker = filePicker ?? throw new ArgumentNullException(nameof(filePicker));
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
                () => {

                    if (filePicker.OpenSelectFilePicker(out string filePath))
                    {
                        var counterpartyFile = new CounterpartyFile
                        {
                            FileStorageId = Path.GetFileName(filePath),
                            ByteFile = File.ReadAllBytes(filePath)
                        };
                        Entity.AddFile(counterpartyFile);
                    }
                },
                () => { return !ReadOnly; }
            );
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
                (file) => {

                    var vodUserTempDir = _userRepository.GetTempDirForCurrentUser(UoW);

                    if (string.IsNullOrWhiteSpace(vodUserTempDir))
                    {
	                    return;
                    }

                    var tempFilePath = Path.Combine(Path.GetTempPath(), vodUserTempDir, file.FileStorageId);

                    if (!File.Exists(tempFilePath))
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
                (file) => {
                    if (filePicker.OpenSaveFilePicker(file.FileStorageId, out string filePath))
                        File.WriteAllBytes(filePath, file.ByteFile);
                },
                (file) => { return !ReadOnly; }
            );
        }

        #endregion LoadItemCommand

        #endregion Commands
    }
}
