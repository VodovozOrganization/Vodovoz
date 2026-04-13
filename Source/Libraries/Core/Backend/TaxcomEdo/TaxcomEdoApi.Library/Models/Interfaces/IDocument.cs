using System;
using System.Collections.Generic;
using TaxcomEdo.Contracts.Xml.Container;

namespace TaxcomEdoApi.Library.Models.Interfaces
{
	public interface IDocument
	{
		string ExternalIdentifier { get; set; }

		string InternalIdentifier { get; set; }

		bool ResignRequired { get; }

		DocumentType Type { get; }
		DateTime Date { get; }

		IEnumerable<string> CertificatesForSign { get; }
		IEnumerable<byte[]> Signatures { get; }
		string Number { get; }

		IParticipant Sender { get; }

		IParticipantWithAgreement Recipient { get; }

		void AddCertificateForSign(string thumbprint);

		void AddExistingSignature(byte[] signature);

		byte[] Image { get; }

		string DocFlowId { get; set; }

		IEnumerable<string> LinkedDocuments { get; set; }

		DocumentContactInfo SenderContactInfo { get; set; }

		DocumentContactInfo ReceiverContactInfo { get; set; }

		string DealNumber { get; set; }

		Department Department { get; set; }

		string Comment { get; set; }

		string Subject { get; set; }

		DescriptionAdditionalParameter[] AdditionalParameter { get; set; }

		IComTaxcomWarrantCard[] Warrants { get; set; }

		string WarrantMetaId { get; set; }
	}
}
