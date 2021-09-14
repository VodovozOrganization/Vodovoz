using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLog;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.ViewModels;
using Vodovoz.Domain.Attachments;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Attachments;
using Vodovoz.ViewModels.TempAdapters;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.ViewModels.Attachments
{
	public class AttachmentsViewModel : UoWWidgetViewModelBase, IDisposable
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private const int _maxFileNameLength = 45;
		private readonly EntityType _entityType;
		private readonly int _entityId;
		private readonly IFileChooserProvider _fileChooserProvider;
		private readonly IScanDialog _scanDialog;
		private readonly IUserRepository _userRepository;
		private bool _hasDeletedAttachments;
		private Attachment _selectedAttachment;

		private DelegateCommand _addCommand;
		private DelegateCommand _openCommand;
		private DelegateCommand _saveCommand;
		private DelegateCommand _deleteCommand;
		private DelegateCommand _scanCommand;
		
		public AttachmentsViewModel(
			IUnitOfWorkFactory uowFactory,
			IFileChooserProvider fileChooserProvider,
			IScanDialog scanDialog,
			IUserRepository userRepository,
			IAttachmentRepository attachmentRepository,
			EntityType entityType,
			int entityId)
		{
			UoW = (uowFactory ?? throw new ArgumentNullException(nameof(uowFactory))).CreateWithoutRoot();
			_fileChooserProvider = fileChooserProvider ?? throw new ArgumentNullException(nameof(fileChooserProvider));
			_scanDialog = scanDialog ?? throw new ArgumentNullException(nameof(scanDialog));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

			if(attachmentRepository == null)
			{
				throw new ArgumentNullException(nameof(attachmentRepository));
			}
			
			_entityType = entityType;
			_entityId = entityId;

			Attachments = new GenericObservableList<Attachment>();

			if(_entityId != -1)
			{
				LoadDataFromDB(attachmentRepository);
			}
		}

		public IList<Attachment> Attachments { get; }

		public Attachment SelectedAttachment
		{
			get => _selectedAttachment;
			set
			{
				if(SetField(ref _selectedAttachment, value))
				{
					OnPropertyChanged(nameof(CanSave));
					OnPropertyChanged(nameof(CanDelete));
					OnPropertyChanged(nameof(CanOpen));
				}
			}
		}

		public bool CanSave => SelectedAttachment != null;
		public bool CanDelete => SelectedAttachment != null;
		public bool CanOpen => SelectedAttachment != null;
		public bool HasChanges => _hasDeletedAttachments || Attachments.Any(file => !file.IsSaved);

		public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(
				() =>
				{
					var filePath = _fileChooserProvider.GetAttachedFileName();
			
					if(!string.IsNullOrEmpty(filePath))
					{
						_logger.Info("Чтение файла...");

						byte[] file = File.ReadAllBytes(filePath);
						
						var attachment = CreateNewAttachment(GetValidFileName(filePath), file);

						Attachments.Add(attachment);
						
						_fileChooserProvider.CloseWindow();
						_logger.Info("Ok");
					}
				}
			)
		);

		public DelegateCommand OpenCommand => _openCommand ?? (_openCommand = new DelegateCommand(
				() =>
				{
					var vodUserTempDir = _userRepository.GetTempDirForCurrentUser(UoW);

					if(string.IsNullOrWhiteSpace(vodUserTempDir))
					{
						return;
					}

					var tempFilePath = Path.Combine(Path.GetTempPath(), vodUserTempDir, SelectedAttachment.FileName);

					if(!File.Exists(tempFilePath))
					{
						File.WriteAllBytes(tempFilePath, SelectedAttachment.ByteFile);
					}

					var process = new Process();
					process.StartInfo.FileName = Path.Combine(vodUserTempDir, SelectedAttachment.FileName);
					process.Start();
				}
			)
		);

		public DelegateCommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(
				() =>
				{
					var filePath = _fileChooserProvider.GetExportFilePath(SelectedAttachment.FileName);

					if(!string.IsNullOrEmpty(filePath))
					{
						_fileChooserProvider.HideWindow();

						_logger.Info("Сохраняем файл на диск...");
						File.WriteAllBytes(filePath, SelectedAttachment.ByteFile);
						_fileChooserProvider.CloseWindow();
						_logger.Info("Ок");
					}
				}
			)
		);

		public DelegateCommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new DelegateCommand(
				() =>
				{
					if(SelectedAttachment.IsSaved)
					{
						UoW.Delete(SelectedAttachment);
						Attachments.Remove(SelectedAttachment);
						_hasDeletedAttachments = true;
					}
					else
					{
						Attachments.Remove(SelectedAttachment);
					}
				}
			)
		);

		public DelegateCommand ScanCommand => _scanCommand ?? (_scanCommand = new DelegateCommand(
				() =>
				{
					_scanDialog.GetFileFromDialog(out string fileName, out byte[] file);

					if(!string.IsNullOrEmpty(fileName))
					{
						var attachment = CreateNewAttachment(GetValidFileName(fileName), file);

						Attachments.Add(attachment);
					}
				}
			)
		);

		private void LoadDataFromDB(IAttachmentRepository attachmentRepository)
		{
			var attachments = attachmentRepository.GetAllAttachmentsForEntity(UoW, _entityType, _entityId);

			foreach(var attachment in attachments)
			{
				Attachments.Add(attachment);
			}
		}

		private string GetValidFileName(string fileName)
		{
			var attachedFileName = Path.GetFileName(fileName);

			//При необходимости обрезаем имя файла.
			if(attachedFileName.Length > _maxFileNameLength)
			{
				var ext = Path.GetExtension(attachedFileName);
				var name = Path.GetFileNameWithoutExtension(attachedFileName);
				attachedFileName = $"{name.Remove(_maxFileNameLength - ext.Length)}{ext}";
			}

			return attachedFileName;
		}

		private Attachment CreateNewAttachment(string attachedFileName, byte[] file) =>
			new Attachment
			{
				EntityId = -1,
				EntityType = _entityType,
				FileName = attachedFileName,
				ByteFile = file
			};

		public void SaveChanges(int entityId)
		{
			if(!HasChanges)
			{
				return;
			}

			var notSavedFiles = Attachments.Where(file => !file.IsSaved).ToList();

			if(notSavedFiles.Any())
			{
				foreach(var file in notSavedFiles)
				{
					file.EntityId = entityId;
					UoW.Save(file);
				}
			}

			UoW.Commit();
		}

		public void Dispose()
		{
			UoW?.Dispose();
		}
	}
}
