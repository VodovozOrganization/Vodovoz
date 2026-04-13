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
	[XmlType(AnonymousType = true)]
	[Serializable]
	public class ParticipantOrganization
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public ParticipantOrganizationType Type { get; set; }
	}
}
