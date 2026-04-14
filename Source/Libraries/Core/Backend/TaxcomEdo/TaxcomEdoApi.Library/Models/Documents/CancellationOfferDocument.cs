using System;
using System.Collections.Generic;
using System.Linq;
using Edo.Contracts.Xml.Transactions.CancellationOffers;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Documents
{
	public class CancellationOfferDocument : IDocument
	{
		private readonly IList<string> _certificatesForSign = new List<string>();
		private readonly IList<byte[]> _signatures = new List<byte[]>();
		
		public CancellationOffer WrapperXml { get; set; }
		
		public string ExternalIdentifier { get; set; }

		public string InternalIdentifier { get; set; }

		public bool ResignRequired { get; private set; }

		public DocumentType Type => DocumentType.CancellationOffer;

		public DateTime Date { get; set; }

		public IEnumerable<string> CertificatesForSign => _certificatesForSign;

		public IEnumerable<byte[]> Signatures => _signatures;

		public string Number { get; set; }

		public bool Validate(out string[] errors)
		{
			/*List<string> stringList = new List<string>();
			stringList.AddRange((IEnumerable<string>)PropertyValidator.Validate((object)this).ToArray());
			errors = stringList.ToArray();
			return !((IEnumerable<string>)errors).Any<string>();*/
			errors = Array.Empty<string>();
			return true;
		}

		public IParticipant Sender { get; set; }

		public IParticipantWithAgreement Recipient { get; set; }

		public void AddCertificateForSign(string thumbprint)
		{
			if(string.IsNullOrEmpty(thumbprint))
			{
				throw new ArgumentNullException(nameof(thumbprint));
			}

			if(_certificatesForSign.Any())
			{
				throw new InvalidOperationException("Предложение об аннулировании может содержать только одного подписанта.");
			}

			_certificatesForSign.Add(thumbprint);
		}

		public void AddExistingSignature(byte[] signature)
		{
			if(signature is null)
			{
				throw new ArgumentNullException(nameof(signature));
			}

			if(Image is null)
			{
				throw new InvalidOperationException(
					"Подпись можно добавлять только к документу, полученному путем импорта из XML (метод ImportFromXmlBytes)");
			}

			_signatures.Add(signature);
		}

		public byte[] Image { get; private set; }

		public FileData Attachment { get; set; }

		public string DocFlowId { get; set; }

		public IEnumerable<string> LinkedDocuments { get; set; }

		public DocumentContactInfo SenderContactInfo { get; set; }

		public DocumentContactInfo ReceiverContactInfo { get; set; }

		public string DealNumber { get; set; }

		public Department Department { get; set; }

		public string Comment { get; set; }

		public string Subject { get; set; }

		public DescriptionAdditionalParameter[] AdditionalParameter { get; set; }

		public IComTaxcomWarrantCard[] Warrants { get; set; }

		public string WarrantMetaId { get; set; }

		//[RequiredLength(MinLength = 1, MaxLength = 40, PropertyName = "Версия передающей программы")]
		public string ProgramVersion { get; set; }

		public string FileIdentifier { get; set; }

		public PersonSigner Signer { get; set; }

		public string CancellationFileName { get; set; }

		public IEnumerable<byte[]> InvoiceSignatures { get; set; }

		public CancellationOfferDocument()
		{
			Signer = new PersonSigner();
			Sender = new Participant();
			Recipient = new ParticipantWithAgreement();
			InvoiceSignatures = new List<byte[]>();
		}
	}
}
