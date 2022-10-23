using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Grouping
	{
		private object[] itemsField;
		private ItemsChoiceType17[] itemsElementNameField;
		private string nameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataCollectionName", typeof(string))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementOutput", typeof(GroupingTypeDataElementOutput))]
		[XmlElement("Filters", typeof(Filters))]
		[XmlElement("GroupExpressions", typeof(GroupExpressions))]
		[XmlElement("Label", typeof(string))]
		[XmlElement("PageBreakAtEnd", typeof(bool))]
		[XmlElement("PageBreakAtStart", typeof(bool))]
		[XmlElement("Parent", typeof(string))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType17[] ItemsElementName
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlAttribute(DataType = "normalizedString")]
		public string Name
		{
			get => nameField;
			set => nameField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType17
	{
		[XmlEnum("##any:")]
		Item,
		CustomProperties,
		DataCollectionName,
		DataElementName,
		DataElementOutput,
		Filters,
		GroupExpressions,
		Label,
		PageBreakAtEnd,
		PageBreakAtStart,
		Parent,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum GroupingTypeDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
	}
}