using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Xml.Container.Entities
{
	[GeneratedCode("xsd", "4.0.30319.1")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[Serializable]
	public class ParticipantAbonent
	{
		[XmlAttribute]
		public string Id { get; set; }

		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string Inn { get; set; }

		[XmlAttribute]
		public string Kpp { get; set; }

		[XmlAttribute]
		public string ContractNumber { get; set; }
	}
}
