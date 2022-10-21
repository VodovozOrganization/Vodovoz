using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Legend
	{
		private object[] itemsField;
		private ItemsChoiceType24[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("InsidePlotArea", typeof(bool))]
		[XmlElement("Layout", typeof(LegendTypeLayout))]
		[XmlElement("Position", typeof(LegendTypePosition))]
		[XmlElement("Style", typeof(Style))]
		[XmlElement("Visible", typeof(bool))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType24[] ItemsElementName
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
	public enum LegendTypeLayout
	{
		Column,
		Row,
		Table,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum LegendTypePosition
	{
		TopLeft,
		TopCenter,
		TopRight,
		LeftTop,
		LeftCenter,
		LeftBottom,
		RightTop,
		RightCenter,
		RightBottom,
		BottomLeft,
		BottomCenter,
		BottomRight,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType24
	{
		[XmlEnum("##any:")]
		Item,
		InsidePlotArea,
		Layout,
		Position,
		Style,
		Visible,
	}
}