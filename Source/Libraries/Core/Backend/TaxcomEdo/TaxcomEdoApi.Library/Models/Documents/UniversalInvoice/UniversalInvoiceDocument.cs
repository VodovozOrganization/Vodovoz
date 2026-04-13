using System;
using System.Collections.Generic;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Documents.UniversalInvoice
{
	/// <summary>
	/// Обертка УПД(универсальный передаточный документ)
	/// </summary>
	public abstract class UniversalInvoiceDocument : IDocument
	{
		public const string DefaultDirectoryInsideArchive = "Invoice";
		
		private readonly IList<string> _certificatesForSign = new List<string>();
		private readonly IList<byte[]> _signatures = new List<byte[]>();
		
		public string ExternalIdentifier { get; set; }
		public string InternalIdentifier { get; set; }
		public bool ResignRequired => true;
		public DocumentType Type => DocumentType.ExpInvoiceAndPrimaryAccountingDocumentVendor;
		public IFileData MainSignatureData { get; set; }
		
		public IEnumerable<string> CertificatesForSign => _certificatesForSign;
		public IEnumerable<byte[]> Signatures => _signatures;

		public abstract string FileIdentifier { get; }
		
		public abstract string Number { get; }
		public IParticipant Sender { get; set; }
		public IParticipantWithAgreement Recipient { get; set; }

		public abstract DateTime Date { get; }
		
		public abstract string CorrectionNumber { get; }
		
		public abstract string CorrectionDate { get; }
		
		public abstract decimal TotalAmountIncludingTaxes { get; }

		public void AddCertificateForSign(string thumbprint)
		{
			if(string.IsNullOrWhiteSpace(thumbprint))
			{
				throw new ArgumentNullException(nameof(thumbprint));
			}
			
			_certificatesForSign.Add(thumbprint);
		}

		public void AddExistingSignature(byte[] signature)
		{
			throw new NotImplementedException();
		}

		public byte[] Image { get; }

		public abstract byte[] DocumentToByteArray();
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
		public IFileData AttachmentFile { get; set; }
	}
}
