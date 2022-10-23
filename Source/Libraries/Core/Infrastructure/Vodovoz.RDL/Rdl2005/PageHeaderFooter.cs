using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class PageHeaderFooter
	{
		private object[] itemsField;
		private ItemsChoiceType34[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Height", typeof(string), DataType = "normalizedString")]
		[XmlElement("PrintOnFirstPage", typeof(bool))]
		[XmlElement("PrintOnLastPage", typeof(bool))]
		[XmlElement("ReportItems", typeof(ReportItems))]
		[XmlElement("Style", typeof(Style))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType34[] ItemsElementName
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
	public enum ItemsChoiceType34
	{
		[XmlEnum("##any:")]
		Item,
		Height,
		PrintOnFirstPage,
		PrintOnLastPage,
		ReportItems,
		Style,
	}
}