using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml.Linq;
using Vodovoz.Domain;
using QSOrmProject;
using QSBusinessCommon.Domain;
using QSBanks;
using Vodovoz.ExportTo1c;

namespace Vodovoz
{	
	public class ExportReferenceNode:IXmlConvertable
	{
		public int Id{ get; set;}
		private bool skipId;
		public List<ExportPropertyNode> Properties{get;set;}
		public ExportReferenceNode()
		{
			Properties = new List<ExportPropertyNode>();
		}			

		public ExportReferenceNode(params ExportPropertyNode[] properties)
			:this()
		{			
			this.skipId = true;
			this.Properties.AddRange(properties);
		}

		public ExportReferenceNode(int id, params ExportPropertyNode[] properties)
			:this()
		{
			this.Id=id;
			this.Properties.AddRange(properties);
		}

		public XElement ToXml()
		{
			var xml = new XElement("Ссылка");
			if (!skipId)
				xml.Add(
					new XAttribute("Нпп", Id)
				);
			Properties.ForEach(prop=>xml.Add(prop.ToXml()));
			return xml;
		}
	}
}

