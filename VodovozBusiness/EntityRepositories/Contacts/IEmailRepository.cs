using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.EntityRepositories
{
	public interface IEmailRepository
	{
		List<StoredEmail> GetAllEmailsForOrder(IUnitOfWork uow, int orderId);
		StoredEmail GetStoredEmailByMessageId(IUnitOfWork uow, string messageId);
		bool HaveSendedEmail(int orderId, OrderDocumentType type);
		bool CanSendByTimeout(string address, int orderId);

		#region EmailType

		IList<EmailType> GetEmailTypes(IUnitOfWork uow);
		EmailType EmailTypeWithPurposeExists(IUnitOfWork uow, EmailPurpose emailPurpose);

		#endregion
	}
}
