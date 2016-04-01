using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Vodovoz.ExportTo1c
{
	public class ExportComissionNode : IXmlConvertable
	{
		public List<int> Comissions{ get; private set;}

		public ExportComissionNode()
		{
			Comissions = new List<int>();
		}

		public System.Xml.Linq.XElement ToXml()
		{
			var xml = new XElement("КомиссияПоСтрокамТабличнойЧасти");
			for(int i=0;i<Comissions.Count;i++){
				var xLine = new XElement("Строка");
				xLine.Value = Comissions[i].ToString();
				xLine.Add(new XAttribute("НомерСтроки",i.ToString()));
				xml.Add(xLine);
			}
			return xml;
		}
	}
}

