using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class MatrixRows
	{
		private List<MatrixRow> matrixRowField = new List<MatrixRow>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("MatrixRow")]
		public List<MatrixRow> MatrixRow
		{
			get => matrixRowField;
			set => matrixRowField = value;
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
}
