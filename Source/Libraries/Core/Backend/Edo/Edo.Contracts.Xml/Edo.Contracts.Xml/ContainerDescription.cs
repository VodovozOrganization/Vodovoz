using System;
using System.Xml.Serialization;

namespace Edo.Contracts.Xml
{
	[Serializable]
	public class ContainerDescription
	{
		[XmlAttribute(AttributeName = "RequestDateTime")]
		public string RequestDateTimeString { get; set; }
		[XmlIgnore]
		public DateTime RequestDateTime => Convert.ToDateTime(RequestDateTimeString);
		[XmlAttribute]
		public bool IsLast { get; set; }
		[XmlElement]
		public DocFlow DocFlow { get; set; }
	}

	[Serializable]
	public class DocFlow
	{
		[XmlAttribute]
		public Guid Id { get; set; }
		[XmlAttribute]
		public string Status { get; set; }
		[XmlAttribute(AttributeName = "StatusChangeDateTime")]
		public string StatusChangeDateTimeString { get; set; }
		[XmlIgnore]
		public DateTime StatusChangeDateTime => Convert.ToDateTime(StatusChangeDateTimeString);
		public Document[] Documents { get; set; }
	}
	
	[Serializable]
	public class Document
	{
		[XmlElement]
		public Definition Definition { get; set; }
	}

	[Serializable]
	public class Definition
	{
		[XmlElement]
		public DefinitionIdentifiers Identifiers { get; set; }
		[XmlElement]
		public DefinitionType Type { get; set; }
	}
	
	[Serializable]
	public class DefinitionIdentifiers
	{
		[XmlAttribute]
		public string InternalId { get; set; }
		[XmlAttribute]
		public string ExternalIdentifier { get; set; }
		[XmlAttribute]
		public string ParentDocumentInternalId { get; set; }
		[XmlAttribute]
		public string ParentDocumentExternalIdentifier { get; set; }
		[XmlAttribute]
		public string InternalDocumentGroupId { get; set; }
		[XmlAttribute]
		public string ExternalDocumentGroupIdentifier { get; set; }
	}

	[Serializable]
	public class DefinitionType
	{
		[XmlAttribute]
		public string Name { get; set; }
		[XmlAttribute]
		public bool ResignRequired { get; set; }
	}
}
