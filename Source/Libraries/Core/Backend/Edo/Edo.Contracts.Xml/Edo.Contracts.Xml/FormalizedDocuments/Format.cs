using System;
using System.Xml.Serialization;

namespace Edo.Contracts.Xml.FormalizedDocuments
{
	[Serializable]
	public enum Format
	{
		[XmlEnum("5.01")] Format5_01,
		[XmlEnum("5.02")] Format5_02,
		[XmlEnum("5.03")] Format5_03
	}
}
