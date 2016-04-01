using System;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace Vodovoz.ExportTo1c
{
	public abstract class ObjectNode : IXmlConvertable
	{		
		public int Id{get;set;}

		public virtual string Type{get;}

		public virtual string RuleName{get;set;}

		public abstract XElement ToXml();
	}
}

