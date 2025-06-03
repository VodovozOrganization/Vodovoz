using NHibernate.Util;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Specialized;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace Vodovoz.Core.Domain.Goods.Recomendations
{
	/// <summary>
	/// Рекомендация номенклатур
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "рекомендацию",
		AccusativePlural = "рекомендации",
		Genitive = "рекомендации",
		GenitivePlural = "рекомендаций",
		Nominative = "Рекомендация",
		NominativePlural = "Рекомендации",
		Prepositional = "рекомендации",
		PrepositionalPlural = "рекомендациях")]
	[HistoryTrace]
	public class Recomendation : PropertyChangedBase, IDomainObject, INamed
	{
		private int _id;
		private string _name;
		private bool _isArchive;
		private PersonType? _personType;
		private RoomType? _roomType;
		private IObservableList<RecomendationItem> _items = new ObservableList<RecomendationItem>();

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			protected set
			{
				if(SetField(ref _id, value))
				{
					UpdateItems();
				}
			}
		}

		/// <summary>
		/// Название
		/// </summary>
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Архив
		/// </summary>
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		/// <summary>
		/// Тип контрагента
		/// </summary>
		public virtual PersonType? PersonType
		{
			get => _personType;
			set => SetField(ref _personType, value);
		}

		/// <summary>
		/// Тип помещения
		/// </summary>
		public virtual RoomType? RoomType
		{
			get => _roomType;
			set => SetField(ref _roomType, value);
		}

		/// <summary>
		/// Строки рекомендации
		/// </summary>
		public virtual IObservableList<RecomendationItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		/// <summary>
		/// Добавляет строку рекомендации с указанным идентификатором номенклатуры и приоритетом
		/// </summary>
		/// <param name="nomenclatureId">Идентификатор номенклатуры</param>
		/// <param name="priority">Приоритет</param>
		/// <returns></returns>
		public virtual bool TryAddItem(int nomenclatureId, int priority)
		{
			if(Items.Any(x => x.NomenclatureId == nomenclatureId))
			{
				return false;
			}

			var item = RecomendationItem.Create(Id, nomenclatureId, priority);

			Items.Add(item);

			return true;
		}

		public virtual bool TryRemoveItem(int nomenclatureId)
		{
			if(!Items.Any(x => x.NomenclatureId == nomenclatureId))
			{
				return false;
			}

			var itemsToRemove = Items.Where(x => x.NomenclatureId == nomenclatureId).ToArray();

			foreach(var itemToRemove in itemsToRemove)
			{
				Items.Remove(itemToRemove);
			}

			UpdateItems();

			return true;
		}

		public virtual void UpdatePriority()
		{
			foreach(var item in Items)
			{
				item.Priority = Items.IndexOf(item) + 1;
			}
		}

		protected virtual void UpdateItems()
		{
			foreach(var item in Items)
			{
				item.RecomendationId = Id;
			}
		}
	}
}
