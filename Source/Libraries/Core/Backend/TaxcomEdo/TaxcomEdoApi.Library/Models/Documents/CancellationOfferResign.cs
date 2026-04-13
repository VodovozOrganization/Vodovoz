using System;
using System.Collections.Generic;
using System.Linq;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Documents
{
	public class CancellationOfferResign : IDocument
	{
		public CancellationOfferResign()
		{
			Signatures = new List<byte[]>();
			AdditionalParameter = new DescriptionAdditionalParameter[0];
		}

		public string ExternalIdentifier { get; set; }

		public string InternalIdentifier { get; set; }

		public bool ResignRequired => false;

		public DocumentType Type => DocumentType.CancellationOfferResign;

		public DateTime Date { get; set; }

		public string Number { get; set; }

		public void AddCertificateForSign(string thumbprint)
		{
			throw new InvalidOperationException("Для переподписи нельзя добавить сертификат");
		}

		public void AddExistingSignature(byte[] signature)
		{
			if(signature == null)
				throw new ArgumentNullException(nameof(signature));
			if(this.Signatures == null)
				this.Signatures = (IEnumerable<byte[]>)new List<byte[]>();
			((List<byte[]>)this.Signatures).Add(signature);
		}

		public IEnumerable<string> CertificatesForSign => null;

		public IEnumerable<byte[]> Signatures { get; set; }

		byte[] IDocument.Image => (byte[])null;

		public string DocFlowId { get; set; }

		public IEnumerable<string> LinkedDocuments { get; set; }

		public DocumentContactInfo SenderContactInfo { get; set; }

		public DocumentContactInfo ReceiverContactInfo { get; set; }

		public Department Department { get; set; }

		public bool Validate(out string[] errors)
		{
			List<string> stringList = new List<string>();
			if(!this.Signatures.Any<byte[]>())
				stringList.Add("не указана переподпись");
			errors = stringList.ToArray();
			return !((IEnumerable<string>)errors).Any<string>();
		}

		public IParticipant Sender { get; set; }

		public IParticipantWithAgreement Recipient { get; set; }

		public string DealNumber { get; set; }

		public string Comment { get; set; }

		public string Subject { get; set; }

		public DescriptionAdditionalParameter[] AdditionalParameter { get; set; }

		public IComTaxcomWarrantCard[] Warrants { get; set; }

		public string WarrantMetaId { get; set; }
	}
}
