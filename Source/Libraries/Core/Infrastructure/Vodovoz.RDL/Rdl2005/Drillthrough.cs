using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Drillthrough
	{
		private object[] itemsField;
		private ItemsChoiceType7[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("BookmarkLink", typeof(string))]
		[XmlElement("Parameters", typeof(Parameters))]
		[XmlElement("ReportName", typeof(string))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType7[] ItemsElementName
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
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
	public enum ItemsChoiceType7
	{
		[XmlEnum("##any:")]
		Item,
		BookmarkLink,
		Parameters,
		ReportName,
	}
}