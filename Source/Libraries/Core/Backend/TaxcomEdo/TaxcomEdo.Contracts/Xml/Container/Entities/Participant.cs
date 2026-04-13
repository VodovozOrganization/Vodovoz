using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Xml.Container.Entities
{
	[GeneratedCode("xsd", "4.0.30319.1")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[Serializable]
	public class Participant
	{
		private object _itemField;

		[XmlElement("Abonent", typeof(ParticipantAbonent), Form = XmlSchemaForm.Unqualified)]
		[XmlElement("Organization", typeof(ParticipantOrganization), Form = XmlSchemaForm.Unqualified)]
		public object Item
		{
			get => _itemField;
			set => _itemField = value;
		}
	}
}
