using System;
using System.Collections.Generic;
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
			var dictionary = new Dictionary<Type, EntityToJournalMapping> { { typeof(Subdivision), new SubdivisionToJournalMapping() } };

			Journals = dictionary;
		}
	}
}
