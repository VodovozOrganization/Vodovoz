using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public abstract class GenericCatalog<T> where T:IDomainObject
	{
		protected Dictionary<T, int> items;
		protected ExportData exportData;
		public GenericCatalog(ExportData data)
		{
			this.items = new Dictionary<T, int>(new DomainObjectEqualityComparer<T>());
			this.exportData = data;
		}			

		protected abstract string Name{ get; }

		protected abstract PropertyNode[] GetProperties(T obj);
		public abstract ReferenceNode CreateReferenceTo(T obj);

		public int GetReferenceId(T obj)
		{
			int id;
			if (!items.TryGetValue(obj, out id))
			{
				id = ++exportData.objectCounter;
				items.Add(obj, id);
				Add(obj);
			}
			return id;
		}
			
		public void Add(T obj)
		{
			var item = new CatalogObjectNode
				{				
					Id = GetReferenceId(obj),
					CatalogueType = this.Name
				};
			item.Reference = CreateReferenceTo(obj);	
			item.Properties.AddRange(GetProperties(obj));
			exportData.Objects.Add(item);
		}

	}
}

