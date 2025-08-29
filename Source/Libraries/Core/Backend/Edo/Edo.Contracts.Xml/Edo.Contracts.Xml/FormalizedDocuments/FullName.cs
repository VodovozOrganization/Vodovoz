using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Edo.Contracts.Xml.FormalizedDocuments
{
	/// <summary>
	/// Полное имя
	/// </summary>
	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlRoot("ФИОТип")]
	[Serializable]
	public class FullName
	{
		/// <summary>
		/// Фамилия
		/// </summary>
		[XmlAttribute("Фамилия")]
		public string LastName { get; set; }
		
		/// <summary>
		/// Имя
		/// </summary>
		[XmlAttribute("Имя")]
		public string Name { get; set; }

		/// <summary>
		/// Отчество
		/// </summary>
		[XmlAttribute("Отчество")]
		public string Patronymic { get; set; }
	}
}
