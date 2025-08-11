using System.Xml.Linq;
using Vodovoz.Tools;

namespace ExportTo1c.Library.ExportNodes
{
	/// <summary>
	/// Объект
	/// </summary>
	public abstract class ObjectNode : IXmlConvertable
	{
		public int Id { get; set; }

		public virtual string Type { get; private set; }

		public virtual string RuleName { get; set; }

		public abstract XElement ToXml();
	}
}
