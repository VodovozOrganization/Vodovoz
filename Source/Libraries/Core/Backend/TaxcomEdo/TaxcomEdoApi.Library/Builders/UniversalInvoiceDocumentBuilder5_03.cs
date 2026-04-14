using Edo.Contracts.Xml.Documents.FormalizedDocuments.UPD;
using TaxcomEdoApi.Library.Models.Documents;
using TaxcomEdoApi.Library.Models.Documents.UniversalInvoice._5_03;

namespace TaxcomEdoApi.Library.Builders
{
	public class UniversalInvoiceDocumentBuilder5_03
	{
		private UniversalInvoiceDocument5_03 _upd = new UniversalInvoiceDocument5_03();

		public UniversalInvoiceDocumentBuilder5_03 WrapperXml(UniversalTransferDocument5_03 wrapperXml)
		{
			_upd.WrapperXml = wrapperXml;
			
			return this;
		}
		
		public UniversalInvoiceDocumentBuilder5_03 ExternalIdentifier(string externalIdentifier)
		{
			_upd.ExternalIdentifier = externalIdentifier;
			return this;
		}
		
		public UniversalInvoiceDocumentBuilder5_03 AddCertificateForSign(string thumbprint)
		{
			_upd.AddCertificateForSign(thumbprint);
			return this;
		}
		
		public UniversalInvoiceDocumentBuilder5_03 Sender(string edoAccountId)
		{
			_upd.Sender = new Participant
			{
				Identifier = edoAccountId
			};

			return this;
		}

		public UniversalInvoiceDocumentBuilder5_03 Recipient(string edoAccountId)
		{
			_upd.Recipient = new ParticipantWithAgreement
			{
				Identifier = edoAccountId
			};

			return this;
		}

		public UniversalInvoiceDocument5_03 Build()
		{
			var createdUpd = _upd;
			_upd = new UniversalInvoiceDocument5_03();
			return createdUpd;
		}

		public static UniversalInvoiceDocumentBuilder5_03 Create() => new UniversalInvoiceDocumentBuilder5_03();
	}
}
