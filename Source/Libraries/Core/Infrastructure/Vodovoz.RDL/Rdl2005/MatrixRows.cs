using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class MatrixRows
	{
		private MatrixRow[] matrixRowField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("MatrixRow")]
		public MatrixRow[] MatrixRow
		{
			get => matrixRowField;
			set => matrixRowField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}