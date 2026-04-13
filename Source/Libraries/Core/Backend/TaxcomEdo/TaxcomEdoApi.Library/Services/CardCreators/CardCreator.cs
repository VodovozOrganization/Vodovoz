using System;
using System.Collections.Generic;
using System.Linq;
using DateTimeHelpers;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdo.Contracts.Xml.Container.Entities;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;
using TaxcomEdoApi.Library.Models;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Services.CardCreators
{
	public abstract class CardCreator
	{
		protected readonly IDocument Document;
		protected readonly IList<DescriptionAdditionalParameter> AdditionalParameters = new List<DescriptionAdditionalParameter>();
		
		protected CardCreator(IDocument document)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
		}
		
		protected virtual Card CreateCardFromDocument()
		{
			var documentReceiver = Document.Recipient;
			var documentSender = Document.Sender;
			
			var card = new Card
			{
				Description = new Description(),
				Identifiers = new DefinitionIdentifiers
				{
					ExternalIdentifier = Document.ExternalIdentifier,
					InternalId = Document.InternalIdentifier
				}
			};

			Participant receiver;
			if(documentReceiver != null)
			{
				receiver = new Participant
				{
					Item = new ParticipantAbonent
					{
						Inn = documentReceiver.Inn,
						Kpp = documentReceiver.Kpp,
						Id = documentReceiver.Identifier,
						Name = documentReceiver.Name,
						ContractNumber = documentReceiver.Agreement.Name
					}
				};
			}
			else
			{
				receiver = new Participant();
			}

			card.Receiver = receiver;
			Participant sender;
			if(documentSender != null)
			{
				sender = new Participant
				{
					Item = new ParticipantAbonent
					{
						Inn = documentSender.Inn,
						Kpp = documentSender.Kpp,
						Id = documentSender.Identifier,
						Name = documentSender.Name
					}
				};
			}
			else
			{
				sender = new Participant();
			}

			card.Sender = sender;
			var documentType = Document.Type;
			var definitionTypeName = documentType.ToDefinitionTypeName();
			card.SetDocumentTypeName(definitionTypeName, Document.ResignRequired);
			card.Description.Title = Document.Subject;
			card.Description.Comment = Document.Comment;
			card.Description.Date = Document.Date.ToCardDescriptionDateTimeString();
			
			AdditionalParameters.Add(CreateDocumentTypeAdditionalParameter());
			
			return card;
		}

		protected DescriptionAdditionalParameter CreateDocumentTypeAdditionalParameter()
		{
			return new()
			{
				Name = CardConstants.DocumentType,
				Value = Document.Type.ToString()
			};
		}

		protected virtual void FillAdditionalData()
		{
			FillLinkedDocumentCollection();
			FillContactInfo();
			FillDealNumber();
			FillDepartmentInfo();
			FillWarrantMetaId();
		}
		
		protected virtual void FillLinkedDocumentCollection()
		{
			if(Document.LinkedDocuments == null || !Document.LinkedDocuments.Any())
			{
				return;
			}

			var additionalParameters =
				Document.LinkedDocuments.Select((Func<string, DescriptionAdditionalParameter>)(_ =>
					new DescriptionAdditionalParameter
					{
						Name = CardConstants.LinkedDocument,
						Value = _
					}));
			
			if(!additionalParameters.All((Func<DescriptionAdditionalParameter, bool>)(_ => Guid.TryParse(_.Value, out Guid _))))
			{
				throw new InvalidOperationException("Идентификатор связанного документа должен иметь тип GUID!");
			}

			foreach(var additionalParameter in additionalParameters)
			{
				AdditionalParameters.Add(additionalParameter);
			}
		}
		
		protected virtual void FillContactInfo()
		{
			if(Document.SenderContactInfo != null)
			{
				if(!string.IsNullOrEmpty(Document.SenderContactInfo.Department))
				{
					AdditionalParameters.Add(new DescriptionAdditionalParameter
					{
						Name = CardConstants.SenderDepartment,
						Value = Document.SenderContactInfo.Department
					});
				}

				if(!string.IsNullOrEmpty(Document.SenderContactInfo.Person))
				{
					AdditionalParameters.Add(new DescriptionAdditionalParameter
					{
						Name = CardConstants.SenderFullName,
						Value = Document.SenderContactInfo.Person
					});
				}

				if(!string.IsNullOrEmpty(Document.SenderContactInfo.Contact))
				{
					AdditionalParameters.Add(new DescriptionAdditionalParameter
					{
						Name = CardConstants.SenderContact,
						Value = Document.SenderContactInfo.Contact
					});
				}
			}
			
			var receiverInfo = Document.ReceiverContactInfo;

			if(receiverInfo is null)
			{
				return;
			}

			if(!string.IsNullOrEmpty(receiverInfo.Department))
			{
				AdditionalParameters.Add(new DescriptionAdditionalParameter
				{
					Name = CardConstants.ReceiverDepartment,
					Value = receiverInfo.Department
				});
			}

			if(!string.IsNullOrEmpty(receiverInfo.Person))
			{
				AdditionalParameters.Add(new DescriptionAdditionalParameter
				{
					Name = CardConstants.ReceiverFullName,
					Value = receiverInfo.Person
				});
			}

			if(string.IsNullOrEmpty(receiverInfo.Contact))
			{
				return;
			}

			AdditionalParameters.Add(new DescriptionAdditionalParameter
			{
				Name = CardConstants.ReceiverContact,
				Value = receiverInfo.Contact
			});
		}
		
		protected virtual void FillDealNumber()
		{
			if(string.IsNullOrWhiteSpace(Document.DealNumber))
			{
				return;
			}

			AdditionalParameters.Add(new DescriptionAdditionalParameter
			{
				Name = CardConstants.DealNumber,
				Value = Document.DealNumber
			});
		}
		
		protected virtual void FillDepartmentInfo()
		{
			if(Document.Department is null)
			{
				return;
			}

			if(Document.Department.Id == Guid.Empty)
			{
				throw new InvalidOperationException("Идентификатор департамента не должен быть NULL!");
			}

			AdditionalParameters.Add(new DescriptionAdditionalParameter
			{
				Name = CardConstants.OwnerDepartmentId,
				Value = Document.Department.Id.ToString()
			});
		}
		
		protected virtual void FillWarrantMetaId()
		{
			if(string.IsNullOrWhiteSpace(Document.WarrantMetaId))
			{
				return;
			}

			AdditionalParameters.Add(new DescriptionAdditionalParameter
			{
				Name = CardConstants.WarrantMetaId,
				Value = Document.WarrantMetaId
			});
		}
	}
}
