using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml.Linq;
using Vodovoz.Domain;
using QSOrmProject;
using QSBusinessCommon.Domain;
using QSBanks;
using Vodovoz.ExportTo1c;

namespace Vodovoz.ExportTo1c
{	
	public class ReferenceNode:IXmlConvertable
	{
		public int Id{ get; set;}
		private bool skipId;
		public List<PropertyNode> Properties{get;set;}
		public ReferenceNode()
		{
			Properties = new List<PropertyNode>();
		}			

		public ReferenceNode(params PropertyNode[] properties)
			:this()
		{			
			this.skipId = true;
			this.Properties.AddRange(properties);
		}

		public ReferenceNode(int id, params PropertyNode[] properties)
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

