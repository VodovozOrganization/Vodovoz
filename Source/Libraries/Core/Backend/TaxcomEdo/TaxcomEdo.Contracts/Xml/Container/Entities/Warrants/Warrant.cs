using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using TaxcomEdo.Contracts.Xml.Container.Entities.Warrant;

namespace TaxcomEdo.Contracts.Xml.Container.Entities.Warrants
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/warrant")]
	[XmlRoot(Namespace = "http://api-invoice.taxcom.ru/warrant", IsNullable = false)]
	public class Warrant
	{
		public Warrant() { }

		public Warrant(WarrantCard[] warrantCards)
		{
			WarrantCards = warrantCards;
		}
		
		/// <remarks/>
		[XmlElement("WarrantCard")]
		public WarrantCard[] WarrantCards { get; set; }
		
		public static Warrant Create(WarrantCard[] warrantCards) => new Warrant(warrantCards);
	}
	
}
