using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Documents
{
	public sealed class NonformalizedDocument : IDocument
	{
		public const string DefaultDirectoryInsideArchive = "MainDocument";
		
		private readonly IList<string> _certificatesForSign =  new List<string>();
		private readonly IList<byte[]> _signatures = new List<byte[]>();
		
		public NonformalizedDocument()
		{
			Recipient = new ParticipantWithAgreement();
			Sender = new Participant();
			Attachment = new FileData();
			AdditionalParameter = new DescriptionAdditionalParameter[0];
		}

		public string ExternalIdentifier { get; set; }

		public string InternalIdentifier { get; set; }

		public bool ResignRequired { get; set; }

		public DocumentType Type { get; set; }

		public DateTime Date { get; set; }

		public IEnumerable<string> CertificatesForSign => _certificatesForSign;

		public IEnumerable<byte[]> Signatures => _signatures;

		public string Number { get; set; }

		public IParticipant Sender { get; set; }
		public IParticipantWithAgreement Recipient { get; set; }

		public void AddCertificateForSign(string thumbprint)
		{
			if(string.IsNullOrEmpty(thumbprint))
			{
				throw new ArgumentNullException(nameof(thumbprint));
			}

			_certificatesForSign.Add(thumbprint);
		}

		public void AddExistingSignature(string filePath)
		{
			AddExistingSignature(File.ReadAllBytes(filePath));
		}

		public void AddExistingSignature(byte[] signature)
		{
			if(signature == null)
			{
				throw new ArgumentNullException(nameof(signature));
			}

			_signatures.Add(signature);
		}

		public byte[] Image => Attachment.Image;

		public string DocFlowId { get; set; }

		public IEnumerable<string> LinkedDocuments { get; set; }

		public DocumentContactInfo SenderContactInfo { get; set; }

		public DocumentContactInfo ReceiverContactInfo { get; set; }

		public Department Department { get; set; }
		
		//[Required("\"Вложение\"")]
		public FileData Attachment { get; set; }

		public string Comment { get; set; }

		public string Subject { get; set; }

		public string TransactionCode { get; set; }

		public decimal Sum { get; set; }

		public bool Validate(out string[] errors)
		{
			//PropertyValidator.Validate((object)this).ToArray();
			errors = Array.Empty<string>();
			return !errors.Any();
		}

		public string DealNumber { get; set; }

		public DescriptionAdditionalParameter[] AdditionalParameter { get; set; }

		public IComTaxcomWarrantCard[] Warrants { get; set; }

		public string WarrantMetaId { get; set; }
	}
}
