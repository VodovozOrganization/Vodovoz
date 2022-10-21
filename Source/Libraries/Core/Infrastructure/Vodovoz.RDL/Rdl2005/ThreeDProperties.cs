using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class ThreeDProperties
	{
		private object[] itemsField;
		private ItemsChoiceType26[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Clustered", typeof(bool))]
		[XmlElement("DepthRatio", typeof(uint))]
		[XmlElement("DrawingStyle", typeof(ThreeDPropertiesTypeDrawingStyle))]
		[XmlElement("Enabled", typeof(bool))]
		[XmlElement("GapDepth", typeof(uint))]
		[XmlElement("HeightRatio", typeof(uint))]
		[XmlElement("Inclination", typeof(string), DataType = "integer")]
		[XmlElement("Perspective", typeof(uint))]
		[XmlElement("ProjectionMode", typeof(ThreeDPropertiesTypeProjectionMode))]
		[XmlElement("Rotation", typeof(string), DataType = "integer")]
		[XmlElement("Shading", typeof(ThreeDPropertiesTypeShading))]
		[XmlElement("WallThickness", typeof(uint))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType26[] ItemsElementName
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
	public enum ThreeDPropertiesTypeDrawingStyle
	{
		Cube,
		Cylinder,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ThreeDPropertiesTypeProjectionMode
	{
		Perspective,
		Orthographic,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ThreeDPropertiesTypeShading
	{
		None,
		Simple,
		Real,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType26
	{
		[XmlEnum("##any:")]
		Item,
		Clustered,
		DepthRatio,
		DrawingStyle,
		Enabled,
		GapDepth,
		HeightRatio,
		Inclination,
		Perspective,
		ProjectionMode,
		Rotation,
		Shading,
		WallThickness,
	}
}