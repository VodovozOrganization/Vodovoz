using System;
using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.Mappings.Nomenclatures;
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
			var dictionary = new Dictionary<Type, EntityToJournalMapping>
			{
				{ typeof(Subdivision), new SubdivisionToJournalMapping() },
				{ typeof(ProductGroup), new ProductGroupToJournalMapping() },
				{ typeof(Nomenclature), new NomenclatureToJournalMapping() }
			};

			Journals = dictionary;
		}
	}
}
