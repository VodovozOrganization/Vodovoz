using System.Collections.Generic;
using System.Xml.Linq;
using Vodovoz.Tools;

namespace ExportTo1c.Library.ExportNodes
{
	/// <summary>
	/// Комиссия
	/// </summary>
	public class ComissionNode : IXmlConvertable
	{
		public List<int> Comissions { get; private set; }

		public ComissionNode()
		{
			Comissions = new List<int>();
		}

		public XElement ToXml()
		{
			var xml = new XElement("КомиссияПоСтрокамТабличнойЧасти");
			for(int i = 0; i < Comissions.Count; i++)
			{
				var xLine = new XElement("Строка");
				xLine.Value = Comissions[i].ToString();
				xLine.Add(new XAttribute("НомерСтроки", i.ToString()));
				xml.Add(xLine);
			}

			return xml;
		}
	}
}
