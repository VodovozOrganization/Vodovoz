using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;

namespace EmailPrepareWorker.Prepares
{
	public interface IEmailDocumentPreparer
	{
		EmailAttachment PrepareDocument(IEmailableDocument document, CounterpartyEmailType counterpartyEmailType, string connectionString);
		EmailAttachment PrepareOfferAgreementDocument(IUnitOfWork unitOfWork, CounterpartyContract contract, string connectionString);
	}
}
