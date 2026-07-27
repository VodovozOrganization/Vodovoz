using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Domain.Client;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;

namespace EmailPrepareWorker.Prepares
{
	public interface IEmailDocumentPreparer
	{
		EmailAttachment PrepareDocument(IEmailableDocument document, CounterpartyEmailType counterpartyEmailType, string connectionString);
		EmailAttachment PrepareOfferAgreementDocument(IUnitOfWork unitOfWork, CounterpartyContract contract, string connectionString);
	}
}
