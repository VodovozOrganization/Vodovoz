using System;
using System.Xml.Linq;

namespace Vodovoz
{
	public interface IXmlConvertable
	{
		XElement ToXml();
	}
}

