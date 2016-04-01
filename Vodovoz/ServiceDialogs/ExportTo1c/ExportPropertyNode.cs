using System;
using System.Xml.Linq;
using Vodovoz.Domain;
using System.Collections.Generic;

namespace Vodovoz
{	
	public class ExportPropertyNode : IXmlConvertable
	{		
		public string Name{ get; set;}

		public string Type{ get; set;}

		public List<XAttribute> AdditionalAttributes{ get; private set;}

		public IXmlConvertable ValueOrReference{get;set;}

		public ExportPropertyNode(string name, string type, IXmlConvertable value)
		{
			this.Name = name;
			this.Type = type;
			this.ValueOrReference = value;		
			this.AdditionalAttributes = new List<XAttribute>();
		}

		public ExportPropertyNode(string name, string type, decimal value)
			: this(name, type, new PropertyValue(value.ToString())){}
		
		public ExportPropertyNode(string name, string type, string value)
			: this(name, type, new PropertyValue(value)){}

		public ExportPropertyNode(string name, string type)
			:this(name,type,new PropertyNull()){}

		public XElement ToXml(){
			var xml = new XElement("Свойство",
				new XAttribute("Имя", Name),
				new XAttribute("Тип", Type),
				ValueOrReference.ToXml()
			);
			foreach (var xattr in AdditionalAttributes)
				xml.Add(xattr);
			return xml;
		}			
	}
}

