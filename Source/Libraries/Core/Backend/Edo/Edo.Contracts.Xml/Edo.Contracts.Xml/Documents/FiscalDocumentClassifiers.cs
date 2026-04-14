using System;
using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Documents
{
	[Serializable]
	public enum FiscalDocumentClassifiers
	{
		[XmlEnum("1115131")]
		KND1115131,
		[XmlEnum("1115132")]
		KND1115132,
		[XmlEnum("1115112")]
		KND1115112,
		[XmlEnum("1115111")]
		KND1115111,
		[XmlEnum("1115110")]
		KND1115110
	}
}
