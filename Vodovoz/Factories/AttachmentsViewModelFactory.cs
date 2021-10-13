using System.Collections.Generic;
using QS.Attachments;
using QS.Attachments.Factories;
using QS.Attachments.ViewModels.Widgets;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using QSAttachment;
using Attachment = QS.Attachments.Domain.Attachment;

namespace Vodovoz.Factories
{
	public class AttachmentsViewModelFactory : IAttachmentsViewModelFactory
	{
		private readonly IAttachmentFactory _attachmentFactory = new AttachmentFactory();
		private readonly IFilePickerService _filePickerService = new GtkFilePicker();
		private readonly IScanDialogService _scanDialogService = new ScanDialogService();

		public AttachmentsViewModel CreateNewAttachmentsViewModel(IList<Attachment> attachments) =>
			new AttachmentsViewModel(
				_attachmentFactory, _filePickerService, _scanDialogService, ServicesConfig.UserService.CurrentUserId, attachments)
			{
				TempSubDirectory = "Vodovoz"
			};
	}
}
