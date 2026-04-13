using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;

namespace TaxcomEdoApi.Library.Models.Containers
{
	public class Container
	{
		public bool ExportCardsAsExternalFiles = true;
		
		private readonly List<IContainerDocflow> _docflows = new List<IContainerDocflow>();
		private IContainerWarrant _warrant;
		public byte[] _containerWarrantImage;

		public DateTime? LastRecordDateTime { get; set; }

		public bool? IsLast { get; set; }

		public virtual IEnumerable<IContainerDocflow> Docflows => _docflows;

		public virtual IContainerWarrant Warrant
		{
			get => this._warrant;
			set => this._warrant = value;
		}

		public static void SetDefaultXmlNamespace(XElement xelem, XNamespace xmlns)
		{
			foreach(XElement xelement in xelem.DescendantsAndSelf())
				xelement.Name = xmlns.GetName(xelement.Name.LocalName);
		}
		
		public void AddDocflows(IEnumerable<IContainerDocflow> docflows)
		{
			foreach(var docflow in docflows)
			{
				_docflows.Add(docflow);
			}
		}
		
		public void AddDocflow(IContainerDocflow docflow)
		{
			_docflows.Add(docflow);
		}

		public DateTime? CreationDateTime { get; internal set; }
	}
}
