using System.Xml.Linq;

namespace Vodovoz.Tools
{
	public interface IXmlConvertable
	{
		XElement ToXml();
	}
}

