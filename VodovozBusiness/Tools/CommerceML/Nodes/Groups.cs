using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Tools.CommerceML.Nodes
{
	public class Groups : IGuidNode, IXmlConvertable 
	{
		IList<ProductGroup> treeOfGroups;

		public Groups(Export export)
		{
			myExport = export;
			myExport.OnProgressPlusOneTask("Выгружаем группы товаров");

			var all = export.UOW.GetAll<ProductGroup>();
			treeOfGroups = all.Where(x => x.Parent == null).ToList();
		}

		Export myExport;

		public Guid Guid => throw new NotImplementedException();

		public XElement ToXml()
		{
			var xml = new XElement("Группы");
			AddGroups(xml, treeOfGroups);
			return xml;
		}

		private void AddGroups(XElement xml, IList<ProductGroup> groups)
		{
			foreach(var group in groups)
			{
				if(!group.ExportToOnlineStore)
					continue;

				var groupxml = new XElement("Группа");
				groupxml.Add(new XElement("Ид", group.GetOrCreateGuid(myExport.UOW)));
				groupxml.Add(new XElement("Наименование", group.Name));
				var childGroupsxml = new XElement("Группы");
				AddGroups(childGroupsxml, group.Childs);
				groupxml.Add(childGroupsxml);
				xml.Add(groupxml);
			}
		}

		public int[] ToExportIds()
		{
			var ids = new List<int>();
			SearchForExport(ids, treeOfGroups);
			return ids.ToArray();
		}

		void SearchForExport(List<int> ids, IList<ProductGroup> groups)
		{
			foreach(var group in groups) {
				if(!group.ExportToOnlineStore)
					continue;

				ids.Add(group.Id);
				if(group.Childs.Any())
					SearchForExport(ids, group.Childs);
			}
		}
	}
}
