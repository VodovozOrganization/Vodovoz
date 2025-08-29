using System;
using System.Xml.Serialization;

namespace Edo.Contracts.Xml.FormalizedDocuments
{
	[Serializable]
	public class TextInformation
	{
		[XmlAttribute("Идентиф")]
		public string Key { get; set; }
		
		[XmlAttribute("Значен")]
		public string Value { get; set; }
	}
}
