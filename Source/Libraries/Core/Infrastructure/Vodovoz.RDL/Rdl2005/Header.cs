using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Header
	{
		private object[] itemsField;
		private ItemsChoiceType20[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("FixedHeader", typeof(bool))]
		[XmlElement("RepeatOnNewPage", typeof(bool))]
		[XmlElement("TableRows", typeof(TableRows))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType20[] ItemsElementName
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
	public enum ItemsChoiceType20
	{
		[XmlEnum("##any:")]
		Item,
		FixedHeader,
		RepeatOnNewPage,
		TableRows,
	}
}