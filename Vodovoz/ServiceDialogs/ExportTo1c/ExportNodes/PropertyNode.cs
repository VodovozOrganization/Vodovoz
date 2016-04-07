using System;
using System.Xml.Linq;
using Vodovoz.Domain;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c
{	
	public class PropertyNode : IXmlConvertable
	{		
		public string Name{ get; set;}

		public string Type{ get; set;}

		public List<XAttribute> AdditionalAttributes{ get; private set;}

		public IXmlConvertable ValueOrReference{get;set;}

		public PropertyNode(string name, string type, IXmlConvertable value)
		{
			this.Name = name;
			this.Type = type;
			this.ValueOrReference = value;		
			this.AdditionalAttributes = new List<XAttribute>();
		}

		public PropertyNode(string name, string type, decimal value)
			: this(name, type, new PropertyValueNode(value.ToString())){}
		
		public PropertyNode(string name, string type, string value)
			: this(name, type, new PropertyValueNode(value)){}

		public PropertyNode(string name, string type)
			:this(name,type,new PropertyNullNode()){}

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

