using System.IO;
using System.Xml;

namespace TaxcomEdoApi.SerializeUtils
{
	public sealed class FajlVersFormFixReader : XmlTextReader
	{
		public FajlVersFormFixReader(TextReader input) : base(input)
		{
		}

		public override string Value
		{
			get
			{
				var value = base.Value;

				if(NodeType == XmlNodeType.Attribute &&
					Name == "ВерсФорм" &&
					value == "1.02")
				{
					return "1.01";
				}

				return value;
			}
		}
	}
}
