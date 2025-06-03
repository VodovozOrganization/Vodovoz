using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Goods.Recomendations
{
	/// <summary>
	/// Строка рекомендаций
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative ="строка рекомендаций",
		AccusativePlural = "строки рекомендаций",
		Genitive = "строки рекомендаций",
		GenitivePlural = "строк рекомендаций",
		Nominative = "строка рекомендации",
		NominativePlural = "строки рекомендаций",
		Prepositional = "строке рекомендации",
		PrepositionalPlural = "строках рекомендаций")]
	[HistoryTrace]
	public class RecomendationItem : PropertyChangedBase, IDomainObject
	{
		private int _nomenclatureId;
		private int _id;
		private int _priority;
		private int _recomendationId;

		/// <summary>
		/// Конструктор по умолчанию для NHibernate
		/// </summary>
		protected RecomendationItem()
		{
		}

		/// <summary>
		/// Конструктор для создания новой строки рекомендаций
		/// </summary>
		/// <param name="nomenclatureId">Идентификатор номенклатуры</param>
		/// <param name="priority"></param>
		private RecomendationItem(int recomendationId, int nomenclatureId, int priority)
		{
			RecomendationId = recomendationId;
			NomenclatureId = nomenclatureId;
			Priority = priority;
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			protected set => SetField(ref _id, value);
		}

		public virtual int RecomendationId
		{
			get => _recomendationId;
			set => SetField(ref _recomendationId, value);
		}

		public virtual int NomenclatureId
		{
			get => _nomenclatureId;
			protected set => SetField(ref _nomenclatureId, value);
		}

		public virtual int Priority
		{
			get => _priority;
			set => SetField(ref _priority, value);
		}

		public static RecomendationItem Create(int recomendationId, int nomenclatureId, int priority)
		{
			return new RecomendationItem(recomendationId, nomenclatureId, priority);
		}
	}
}
