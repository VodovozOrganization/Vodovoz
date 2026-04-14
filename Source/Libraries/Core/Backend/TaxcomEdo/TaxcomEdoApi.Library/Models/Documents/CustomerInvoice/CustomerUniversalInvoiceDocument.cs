using System;
using System.Collections.Generic;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Documents.CustomerInvoice
{
	//TODO написать полное соответствие классу
	public abstract class CustomerUniversalInvoiceDocument : IDocument
	{
		public const string DefaultDirectoryInsideArchive = "CustomerInformation";
		
		private readonly IList<string> _certificatesForSign = new List<string>();
		private readonly IList<byte[]> _signatures = new List<byte[]>();
		
		public string ExternalIdentifier { get; set; }
		public string InternalIdentifier { get; set; }
		public bool ResignRequired { get; private set; }
		public DocumentType Type { get; private set; }
		public IFileData MainSignatureData { get; set; }
		
		public IEnumerable<string> CertificatesForSign => _certificatesForSign;
		public IEnumerable<byte[]> Signatures => _signatures;

		public abstract string FileIdentifier { get; }
		
		public abstract string Number { get; }
		public bool Validate(out string[] errors)
		{
			throw new NotImplementedException();
		}

		public IParticipant Sender { get; }
		public IParticipantWithAgreement Recipient { get; }

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
		//TODO проверить свойство и что туда попадает
		public IFileData DataImage { get; set; }
		public string DocFlowId { get; set; }
		public IEnumerable<string> LinkedDocuments { get; set; }
		public DocumentContactInfo SenderContactInfo { get; set; }
		public DocumentContactInfo ReceiverContactInfo { get; set; }
		public string DealNumber { get; set; }
		public abstract byte[] DocumentToByteArray();

		public Department Department { get; set; }
		public string Comment { get; set; }
		public string Subject { get; set; }
		public DescriptionAdditionalParameter[] AdditionalParameter { get; set; }
		public IComTaxcomWarrantCard[] Warrants { get; set; }
		public string WarrantMetaId { get; set; }
	}
}
