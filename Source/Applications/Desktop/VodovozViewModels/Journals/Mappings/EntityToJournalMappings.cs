using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Journals.Mappings.Nomenclatures;
using Vodovoz.ViewModels.Journals.Mappings.Organizations;
using Vodovoz.ViewModels.Journals.Mappings.ProductGroups;
using Vodovoz.ViewModels.Journals.Mappings.Subdivisions;

namespace Vodovoz.ViewModels.Journals.Mappings
{
	/// <summary>
	/// Класс для хранения всех зарегистрированных соответствий Сущность - Журнал
	/// </summary>
	public class EntityToJournalMappings
	{
		public IReadOnlyDictionary<Type, EntityToJournalMapping> Journals { get; private set; }

		public EntityToJournalMappings()
		{
			Initialize();
		}

		private void Initialize()
		{
			var productGroupToJournalMapping = new ProductGroupToJournalMapping();
			productGroupToJournalMapping.ConfigureJournal(x => x.Filter.IsGroupSelectionMode = true);
			
			var dictionary = new Dictionary<Type, EntityToJournalMapping>
			{
				{ typeof(Subdivision), new SubdivisionToJournalMapping() },
				{ typeof(ProductGroup), productGroupToJournalMapping },
				{ typeof(Nomenclature), new NomenclatureToJournalMapping() },
				{ typeof(Organization), new OrganizationToJournalMapping() }
			};

			Journals = dictionary;
		}
	}
}
