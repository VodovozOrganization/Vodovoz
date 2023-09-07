using System.Threading.Tasks;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;

namespace EmailPrepareWorker.Prepares
{
	public interface IEmailDocumentPreparer
	{
		Task<EmailAttachment> PrepareDocument(IEmailableDocument document, CounterpartyEmailType counterpartyEmailType);
	}
}
