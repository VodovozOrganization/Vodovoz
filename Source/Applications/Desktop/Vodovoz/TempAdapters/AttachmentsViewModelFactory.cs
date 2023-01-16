using System.Collections.Generic;
using QS.Attachments;
using QS.Attachments.Factories;
using QS.Attachments.ViewModels.Widgets;
using QS.Dialog.GtkUI.FileDialog;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QSAttachment;
using Vodovoz.Factories;
using Attachment = QS.Attachments.Domain.Attachment;

namespace Vodovoz.TempAdapters
{
	public class AttachmentsViewModelFactory : IAttachmentsViewModelFactory
	{
		private readonly IAttachmentFactory _attachmentFactory = new AttachmentFactory();
		private readonly IFileDialogService _fileDialogService = new FileDialogService();
		private readonly IScanDialogService _scanDialogService = new ScanDialogService();

		public AttachmentsViewModel CreateNewAttachmentsViewModel(IList<Attachment> attachments) =>
			new AttachmentsViewModel(
				_attachmentFactory, _fileDialogService, _scanDialogService, ServicesConfig.UserService.CurrentUserId, attachments)
			{
				TempSubDirectory = "Vodovoz"
			};
	}
}
