using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class BackgroundImage
	{
		private object[] itemsField;
		private ItemsChoiceType4[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("BackgroundRepeat", typeof(string))]
		[XmlElement("MIMEType", typeof(string))]
		[XmlElement("Source", typeof(BackgroundImageTypeSource))]
		[XmlElement("Value", typeof(string))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType4[] ItemsElementName
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
	[XmlType(AnonymousType = true)]
	public enum BackgroundImageTypeSource
	{
		External,
		Embedded,
		Database,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType4
	{
		[XmlEnum("##any:")]
		Item,
		BackgroundRepeat,
		MIMEType,
		Source,
		Value,
	}
}