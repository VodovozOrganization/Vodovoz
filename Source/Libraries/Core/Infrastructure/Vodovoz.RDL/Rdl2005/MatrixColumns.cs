using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class MatrixColumns
	{
		private MatrixColumn[] matrixColumnField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("MatrixColumn")]
		public MatrixColumn[] MatrixColumn
		{
			get => matrixColumnField;
			set => matrixColumnField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}