using System;
using System.Linq;
using System.Xml.Serialization;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;

namespace TaxcomEdo.Contracts.Xml.Container
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(Namespace = "http://api-invoice.taxcom.ru/meta")]
	[XmlRoot(Namespace = "http://api-invoice.taxcom.ru/meta", IsNullable = false)]
	public partial class ContainerDescription
	{
		/// <remarks/>
		[XmlElement("DocFlow")]
		public ContainerDescriptionDocFlow[] DocFlow { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string RequestDateTime { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public bool IsLast { get; set; }

		/// <remarks/>
		[XmlIgnore]
		public bool IsLastSpecified { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string LastRecordDateTime { get; set; }
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public partial class ContainerDescriptionDocFlow
	{
		/// <remarks/>
		public Description Description { get; set; }

		/// <remarks/>
		[XmlArrayItem("Document", IsNullable = false)]
		public ContainerDescriptionDocFlowDocument[] Documents { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string Id { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public ContainerDocFlowStatus Status { get; set; }

		/// <remarks/>
		[XmlIgnore]
		public bool StatusSpecified { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public ContainerDocFlowInternalStatus InternalStatus { get; set; }

		/// <remarks/>
		[XmlIgnore]
		public bool InternalStatusSpecified { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public DocFlowErrorType ErrorType { get; set; }
		
		public bool ShouldSerializeErrorType()
		{
			return ErrorType != DocFlowErrorType.None;
		}

		/// <remarks/>
		[XmlIgnore]
		public bool ErrorTypeSpecified { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string ErrorDescription { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string StatusChangeDateTime { get; set; }
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class Description
	{
		/// <remarks/>
		[XmlArrayItem("AdditionalParameter", IsNullable = false)]
		public DescriptionAdditionalParameter[] AdditionalData { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string Title { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string Date { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string Comment { get; set; }

		public void AddAdditionalParameter(DescriptionAdditionalParameter parameter)
		{
			if(AdditionalData is null)
			{
				throw new InvalidOperationException("Массив AdditionalData должен быть проинициализирован. Смотри логику работы с классом Card");
			}

			var parameters = AdditionalData.ToList();
			parameters.Add(parameter);
			
			AdditionalData = parameters.ToArray();
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class DescriptionAdditionalParameter
	{
		/// <remarks/>
		[XmlAttribute]
		public string Name { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string Value { get; set; }
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class FileInfo
	{
		/// <remarks/>
		[XmlAttribute]
		public string Path { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string Name { get; set; }

		public static FileInfo Create(string path) => new FileInfo
		{
			Path = path,
		};
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class Signer
	{
		/// <remarks/>
		[XmlElement("Certificate", typeof(SignerCertificate))]
		[XmlElement("Person", typeof(SignerPerson))]
		public object Item { get; set; }
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class SignerCertificate
	{
		/// <remarks/>
		[XmlAttribute]
		public string Thumbprint { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string SerialNumber { get; set; }
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public partial class SignerPerson
	{
		/// <remarks/>
		[XmlAttribute]
		public string LastName { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string FirstName { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string Patronimic { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string Inn { get; set; }
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public partial class ContainerDescriptionDocFlowDocument
	{
		/// <remarks/>
		[XmlElement("Card", typeof(Card))]
		[XmlElement("Definition", typeof(Definition))]
		public Definition Item { get; set; }

		/// <remarks/>
		public ContainerDescriptionDocFlowDocumentStatus Status { get; set; }

		/// <remarks/>
		public ContainerDescriptionDocFlowDocumentFiles Files { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string ReglamentCode { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string TransactionCode { get; set; }
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public partial class ContainerDescriptionDocFlowDocumentStatus
	{
		/// <remarks/>
		[XmlAttribute(DataType = "integer")]
		public string OrderInDocFlow { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public DocFlowDirection Direction { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public ContainerDescriptionDocFlowDocumentStatusTransactionResultType TransactionResultType { get; set; }
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
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
	[Serializable]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public partial class ContainerDescriptionDocFlowDocumentFiles
	{
		/// <remarks/>
		public FileInfo MainImage { get; set; }

		/// <remarks/>
		[XmlElement("MainImageSignature")]
		public FileInfo[] MainImageSignature { get; set; }

		/// <remarks/>
		public FileInfo DataImage { get; set; }

		/// <remarks/>
		[XmlElement("DataImageSignature")]
		public FileInfo[] DataImageSignature { get; set; }

		/// <remarks/>
		public FileInfo ExternalCard { get; set; }
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public enum ContainerDocFlowStatus
	{
		Unknown,
		InProgress,
		Succeed,
		Warning,
		Error,
		NotStarted,
		CompletedWithDivergences,
		NotAccepted,
		WaitingForCancellation,
		Cancelled,
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public enum ContainerDocFlowInternalStatus
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
		
		/// <remarks/>
		Unknown,
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public enum DocFlowErrorType
	{
		None,
		
		/// <remarks/>
		Unknown,
		
		/// <remarks/>
		ImportFailed,

		/// <remarks/>
		VerificationError,

		/// <remarks/>
		SignaturesCheckFailed,

		/// <remarks/>
		SendingError,

		/// <remarks/>
		DocflowError
	}
}
