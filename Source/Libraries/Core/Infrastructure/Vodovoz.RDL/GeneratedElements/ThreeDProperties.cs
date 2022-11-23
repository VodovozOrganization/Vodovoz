using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class ThreeDProperties
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType26> itemsElementNameField = new List<ItemsChoiceType26>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

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
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore()]
		public List<ItemsChoiceType26> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType26[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType26>() : value.ToList();
		}

		[XmlIgnore()]
		public List<XmlAttribute> AnyAttrList
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => AnyAttrList.ToArray();
			set => AnyAttrList = value == null ? new List<XmlAttribute>() : value.ToList();
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
