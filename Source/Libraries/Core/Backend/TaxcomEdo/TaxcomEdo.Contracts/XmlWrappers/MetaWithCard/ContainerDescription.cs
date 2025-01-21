namespace TaxcomEdo.Contracts.XmlWrappers.MetaXml
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://api-invoice.taxcom.ru/meta")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://api-invoice.taxcom.ru/meta", IsNullable = false)]
	public partial class ContainerDescription
	{
		private ContainerDescriptionDocFlow[] docFlowField;

		private string requestDateTimeField;

		private bool isLastField;

		private bool isLastFieldSpecified;

		private string lastRecordDateTimeField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("DocFlow")]
		public ContainerDescriptionDocFlow[] DocFlow
		{
			get { return this.docFlowField; }
			set { this.docFlowField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string RequestDateTime
		{
			get { return this.requestDateTimeField; }
			set { this.requestDateTimeField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public bool IsLast
		{
			get { return this.isLastField; }
			set { this.isLastField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool IsLastSpecified
		{
			get { return this.isLastFieldSpecified; }
			set { this.isLastFieldSpecified = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string LastRecordDateTime
		{
			get { return this.lastRecordDateTimeField; }
			set { this.lastRecordDateTimeField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public partial class ContainerDescriptionDocFlow
	{

		private Description descriptionField;

		private ContainerDescriptionDocFlowDocument[] documentsField;

		private string idField;

		private ContainerDescriptionDocFlowStatus statusField;

		private bool statusFieldSpecified;

		private ContainerDescriptionDocFlowInternalStatus internalStatusField;

		private bool internalStatusFieldSpecified;

		private ContainerDescriptionDocFlowErrorType errorTypeField;

		private bool errorTypeFieldSpecified;

		private string errorDescriptionField;

		private string statusChangeDateTimeField;

		/// <remarks/>
		public Description Description
		{
			get { return this.descriptionField; }
			set { this.descriptionField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("Document", IsNullable = false)]
		public ContainerDescriptionDocFlowDocument[] Documents
		{
			get { return this.documentsField; }
			set { this.documentsField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Id
		{
			get { return this.idField; }
			set { this.idField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public ContainerDescriptionDocFlowStatus Status
		{
			get { return this.statusField; }
			set { this.statusField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool StatusSpecified
		{
			get { return this.statusFieldSpecified; }
			set { this.statusFieldSpecified = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public ContainerDescriptionDocFlowInternalStatus InternalStatus
		{
			get { return this.internalStatusField; }
			set { this.internalStatusField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool InternalStatusSpecified
		{
			get { return this.internalStatusFieldSpecified; }
			set { this.internalStatusFieldSpecified = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public ContainerDescriptionDocFlowErrorType ErrorType
		{
			get { return this.errorTypeField; }
			set { this.errorTypeField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool ErrorTypeSpecified
		{
			get { return this.errorTypeFieldSpecified; }
			set { this.errorTypeFieldSpecified = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ErrorDescription
		{
			get { return this.errorDescriptionField; }
			set { this.errorDescriptionField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string StatusChangeDateTime
		{
			get { return this.statusChangeDateTimeField; }
			set { this.statusChangeDateTimeField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class Description
	{

		private DescriptionAdditionalParameter[] additionalDataField;

		private string titleField;

		private string dateField;

		private string commentField;

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("AdditionalParameter", IsNullable = false)]
		public DescriptionAdditionalParameter[] AdditionalData
		{
			get { return this.additionalDataField; }
			set { this.additionalDataField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Title
		{
			get { return this.titleField; }
			set { this.titleField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Date
		{
			get { return this.dateField; }
			set { this.dateField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Comment
		{
			get { return this.commentField; }
			set { this.commentField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class DescriptionAdditionalParameter
	{

		private string nameField;

		private string valueField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Name
		{
			get { return this.nameField; }
			set { this.nameField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Value
		{
			get { return this.valueField; }
			set { this.valueField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class FileInfo
	{

		private string pathField;

		private string nameField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Path
		{
			get { return this.pathField; }
			set { this.pathField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Name
		{
			get { return this.nameField; }
			set { this.nameField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class Signer
	{

		private object itemField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Certificate", typeof(SignerCertificate))]
		[System.Xml.Serialization.XmlElementAttribute("Person", typeof(SignerPerson))]
		public object Item
		{
			get { return this.itemField; }
			set { this.itemField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class SignerCertificate
	{

		private string thumbprintField;

		private string serialNumberField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Thumbprint
		{
			get { return this.thumbprintField; }
			set { this.thumbprintField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string SerialNumber
		{
			get { return this.serialNumberField; }
			set { this.serialNumberField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class SignerPerson
	{

		private string lastNameField;

		private string firstNameField;

		private string patronimicField;

		private string innField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string LastName
		{
			get { return this.lastNameField; }
			set { this.lastNameField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string FirstName
		{
			get { return this.firstNameField; }
			set { this.firstNameField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Patronimic
		{
			get { return this.patronimicField; }
			set { this.patronimicField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Inn
		{
			get { return this.innField; }
			set { this.innField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class Participant
	{

		private object itemField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Abonent", typeof(ParticipantAbonent))]
		[System.Xml.Serialization.XmlElementAttribute("Organization", typeof(ParticipantOrganization))]
		public object Item
		{
			get { return this.itemField; }
			set { this.itemField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class ParticipantAbonent
	{

		private string idField;

		private string nameField;

		private string innField;

		private string kppField;

		private string contractNumberField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Id
		{
			get { return this.idField; }
			set { this.idField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Name
		{
			get { return this.nameField; }
			set { this.nameField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Inn
		{
			get { return this.innField; }
			set { this.innField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Kpp
		{
			get { return this.kppField; }
			set { this.kppField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ContractNumber
		{
			get { return this.contractNumberField; }
			set { this.contractNumberField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class ParticipantOrganization
	{

		private string nameField;

		private ParticipantOrganizationType typeField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Name
		{
			get { return this.nameField; }
			set { this.nameField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public ParticipantOrganizationType Type
		{
			get { return this.typeField; }
			set { this.typeField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public enum ParticipantOrganizationType
	{

		/// <remarks/>
		SpecOperator,
	}

	/// <remarks/>
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(Card))]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class Definition
	{

		private DefinitionIdentifiers identifiersField;

		private DefinitionType typeField;

		/// <remarks/>
		public DefinitionIdentifiers Identifiers
		{
			get { return this.identifiersField; }
			set { this.identifiersField = value; }
		}

		/// <remarks/>
		public DefinitionType Type
		{
			get { return this.typeField; }
			set { this.typeField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class DefinitionIdentifiers
	{

		private string internalIdField;

		private string externalIdentifierField;

		private string parentDocumentInternalIdField;

		private string parentDocumentExternalIdentifierField;

		private string internalDocumentGroupIdField;

		private string externalDocumentGroupIdentifierField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string InternalId
		{
			get { return this.internalIdField; }
			set { this.internalIdField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ExternalIdentifier
		{
			get { return this.externalIdentifierField; }
			set { this.externalIdentifierField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ParentDocumentInternalId
		{
			get { return this.parentDocumentInternalIdField; }
			set { this.parentDocumentInternalIdField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ParentDocumentExternalIdentifier
		{
			get { return this.parentDocumentExternalIdentifierField; }
			set { this.parentDocumentExternalIdentifierField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string InternalDocumentGroupId
		{
			get { return this.internalDocumentGroupIdField; }
			set { this.internalDocumentGroupIdField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ExternalDocumentGroupIdentifier
		{
			get { return this.externalDocumentGroupIdentifierField; }
			set { this.externalDocumentGroupIdentifierField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class DefinitionType
	{

		private DefinitionTypeName nameField;

		private bool nameFieldSpecified;

		private bool resignRequiredField;

		private bool resignRequiredFieldSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public DefinitionTypeName Name
		{
			get { return this.nameField; }
			set { this.nameField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool NameSpecified
		{
			get { return this.nameFieldSpecified; }
			set { this.nameFieldSpecified = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public bool ResignRequired
		{
			get { return this.resignRequiredField; }
			set { this.resignRequiredField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool ResignRequiredSpecified
		{
			get { return this.resignRequiredFieldSpecified; }
			set { this.resignRequiredFieldSpecified = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public enum DefinitionTypeName
	{

		/// <remarks/>
		Invoice,

		/// <remarks/>
		CorrectiveInvoice,

		/// <remarks/>
		Account,

		/// <remarks/>
		Statement,

		/// <remarks/>
		StatementAppendix,

		/// <remarks/>
		Consignment,

		/// <remarks/>
		PaymentOrder,

		/// <remarks/>
		Contract,

		/// <remarks/>
		ComplexStatementAndInvoice,

		/// <remarks/>
		GuaranteeLetter,

		/// <remarks/>
		Other,

		/// <remarks/>
		SendingTimeConfirmation,

		/// <remarks/>
		ReceiveNotification,

		/// <remarks/>
		SpecificationNotice,

		/// <remarks/>
		FormalizedConsignmentVendor,

		/// <remarks/>
		FormalizedConsignmentCustomer,

		/// <remarks/>
		FormalizedStatementVendor,

		/// <remarks/>
		FormalizedStatementCustomer,

		/// <remarks/>
		CancellationOffer,

		/// <remarks/>
		ReconciliationStatement,

		/// <remarks/>
		OffsettingStatement,

		/// <remarks/>
		Ks11,

		/// <remarks/>
		Ks2,

		/// <remarks/>
		Ks3,

		/// <remarks/>
		Report,

		/// <remarks/>
		Notification,

		/// <remarks/>
		Sheet,

		/// <remarks/>
		EdoAgreement,

		/// <remarks/>
		Registry,

		/// <remarks/>
		InvoiceForPayment,

		/// <remarks/>
		Specification,

		/// <remarks/>
		AdditionalAgreement,

		/// <remarks/>
		TradeConsigment,

		/// <remarks/>
		ProductWithdrawal,

		/// <remarks/>
		Shipment,

		/// <remarks/>
		Etrn1,

		/// <remarks/>
		Etrn2,

		/// <remarks/>
		Etrn3,

		/// <remarks/>
		Etrn4,

		/// <remarks/>
		Etrn5,

		/// <remarks/>
		Etrn6,

		/// <remarks/>
		FormalizedWorkResultVendor,

		/// <remarks/>
		FormalizedWorkResultCustomer,

		/// <remarks/>
		FormalizedTradingVendor,

		/// <remarks/>
		FormalizedTradingCustomer,

		/// <remarks/>
		ExpInvoice,

		/// <remarks/>
		ExpInvoiceAndPrimaryAccountingDocumentVendor,

		/// <remarks/>
		ExpInvoiceAndPrimaryAccountingDocumentCustomer,

		/// <remarks/>
		PrimaryAccountingDocumentVendor,

		/// <remarks/>
		PrimaryAccountingDocumentCustomer,

		/// <remarks/>
		CorExpInvoice,

		/// <remarks/>
		CorExpInvoiceAndPrimaryAccountingDocumentVendor,

		/// <remarks/>
		CorExpInvoiceAndPrimaryAccountingDocumentCustomer,

		/// <remarks/>
		CorPrimaryAccountingDocumentVendor,

		/// <remarks/>
		CorPrimaryAccountingDocumentCustomer,

		/// <remarks/>
		TracingAccepted,

		/// <remarks/>
		TracingRejected,

		/// <remarks/>
		TracingCancellationAccepted,

		/// <remarks/>
		TracingCancellationRejected,

		/// <remarks/>
		UnitPack,

		/// <remarks/>
		UnitUnpack,

		/// <remarks/>
		Desadv,

		/// <remarks/>
		Recadv,

		/// <remarks/>
		RejectDesadv,

		/// <remarks/>
		Aperak,

		/// <remarks/>
		Pricat,

		/// <remarks/>
		ReplyPricat,

		/// <remarks/>
		RejectPricat,

		/// <remarks/>
		Orders,

		/// <remarks/>
		Ordrsp,

		/// <remarks/>
		RejectOrders,
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://api-invoice.taxcom.ru/card")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://api-invoice.taxcom.ru/card", IsNullable = false)]
	public partial class Card : Definition
	{

		private Description descriptionField;

		private Participant senderField;

		private Participant receiverField;

		private Signer[] signersField;

		/// <remarks/>
		public Description Description
		{
			get { return this.descriptionField; }
			set { this.descriptionField = value; }
		}

		/// <remarks/>
		public Participant Sender
		{
			get { return this.senderField; }
			set { this.senderField = value; }
		}

		/// <remarks/>
		public Participant Receiver
		{
			get { return this.receiverField; }
			set { this.receiverField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
		public Signer[] Signers
		{
			get { return this.signersField; }
			set { this.signersField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public partial class ContainerDescriptionDocFlowDocument
	{

		private Definition itemField;

		private ContainerDescriptionDocFlowDocumentStatus statusField;

		private ContainerDescriptionDocFlowDocumentFiles filesField;

		private string reglamentCodeField;

		private string transactionCodeField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Card", typeof(Card))]
		[System.Xml.Serialization.XmlElementAttribute("Definition", typeof(Definition))]
		public Definition Item
		{
			get { return this.itemField; }
			set { this.itemField = value; }
		}

		/// <remarks/>
		public ContainerDescriptionDocFlowDocumentStatus Status
		{
			get { return this.statusField; }
			set { this.statusField = value; }
		}

		/// <remarks/>
		public ContainerDescriptionDocFlowDocumentFiles Files
		{
			get { return this.filesField; }
			set { this.filesField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ReglamentCode
		{
			get { return this.reglamentCodeField; }
			set { this.reglamentCodeField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string TransactionCode
		{
			get { return this.transactionCodeField; }
			set { this.transactionCodeField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public partial class ContainerDescriptionDocFlowDocumentStatus
	{

		private string orderInDocFlowField;

		private ContainerDescriptionDocFlowDocumentStatusDirection directionField;

		private ContainerDescriptionDocFlowDocumentStatusTransactionResultType transactionResultTypeField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
		public string OrderInDocFlow
		{
			get { return this.orderInDocFlowField; }
			set { this.orderInDocFlowField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public ContainerDescriptionDocFlowDocumentStatusDirection Direction
		{
			get { return this.directionField; }
			set { this.directionField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public ContainerDescriptionDocFlowDocumentStatusTransactionResultType TransactionResultType
		{
			get { return this.transactionResultTypeField; }
			set { this.transactionResultTypeField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public enum ContainerDescriptionDocFlowDocumentStatusDirection
	{

		/// <remarks/>
		Incoming,

		/// <remarks/>
		Outgoing,
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public enum ContainerDescriptionDocFlowDocumentStatusTransactionResultType
	{

		/// <remarks/>
		InProgress,

		/// <remarks/>
		Completed,

		/// <remarks/>
		Error,

		/// <remarks/>
		Warning,
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public partial class ContainerDescriptionDocFlowDocumentFiles
	{

		private FileInfo mainImageField;

		private FileInfo[] mainImageSignatureField;

		private FileInfo dataImageField;

		private FileInfo[] dataImageSignatureField;

		private FileInfo externalCardField;

		/// <remarks/>
		public FileInfo MainImage
		{
			get { return this.mainImageField; }
			set { this.mainImageField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("MainImageSignature")]
		public FileInfo[] MainImageSignature
		{
			get { return this.mainImageSignatureField; }
			set { this.mainImageSignatureField = value; }
		}

		/// <remarks/>
		public FileInfo DataImage
		{
			get { return this.dataImageField; }
			set { this.dataImageField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("DataImageSignature")]
		public FileInfo[] DataImageSignature
		{
			get { return this.dataImageSignatureField; }
			set { this.dataImageSignatureField = value; }
		}

		/// <remarks/>
		public FileInfo ExternalCard
		{
			get { return this.externalCardField; }
			set { this.externalCardField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public enum ContainerDescriptionDocFlowStatus
	{

		/// <remarks/>
		NotStarted,

		/// <remarks/>
		InProgress,

		/// <remarks/>
		Succeed,

		/// <remarks/>
		Warning,

		/// <remarks/>
		Error,
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public enum ContainerDescriptionDocFlowInternalStatus
	{

		/// <remarks/>
		None,

		/// <remarks/>
		OnNegotiation,

		/// <remarks/>
		Negotiated,

		/// <remarks/>
		FailNegotiation,

		/// <remarks/>
		OnSign,

		/// <remarks/>
		SignedAndSent,

		/// <remarks/>
		FailSign,
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public enum ContainerDescriptionDocFlowErrorType
	{

		/// <remarks/>
		ImportFailed,

		/// <remarks/>
		VerificationError,

		/// <remarks/>
		SignaturesCheckFailed,

		/// <remarks/>
		SendingError,

		/// <remarks/>
		DocflowError,

		/// <remarks/>
		Unknown,
	}
}
