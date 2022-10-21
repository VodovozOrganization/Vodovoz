using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class BorderColorStyleWidth
	{
		private object[] itemsField;
		private ItemsChoiceType3[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Bottom", typeof(string))]
		[XmlElement("Default", typeof(string))]
		[XmlElement("Left", typeof(string))]
		[XmlElement("Right", typeof(string))]
		[XmlElement("Top", typeof(string))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType3[] ItemsElementName
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
	public enum ItemsChoiceType3
	{
		[XmlEnum("##any:")]
		Item,
		Bottom,
		Default,
		Left,
		Right,
		Top,
	}
}