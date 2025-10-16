using Mailjet.Api.Abstractions;
using System.Collections.Generic;

namespace BitrixApi.Library.Services
{
	public interface IEmailAttachmentsCreateService
	{
		IEnumerable<EmailAttachment> CreateGeneralBillAttachments(int counterpartyId, int organizationId);
		IEnumerable<EmailAttachment> CreateNotPaidOrdersBillAttachments(int counterpartyId, int organizationId);
		IEnumerable<EmailAttachment> CreateRevisionAttachments(int counterpartyId, int organizationId);
		byte[] MergePdfs(IEnumerable<byte[]> pdfs);
	}
}