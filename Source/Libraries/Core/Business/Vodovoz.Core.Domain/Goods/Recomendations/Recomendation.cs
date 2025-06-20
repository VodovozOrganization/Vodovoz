using Microsoft.Extensions.DependencyInjection;
using NHibernate.Util;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
	[EntityPermission]
	public class Recomendation : PropertyChangedBase, IDomainObject, INamed, IValidatableObject
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
		[Display(Name = "Идентификатор")]
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
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Архив
		/// </summary>
		[Display(Name = "Архив")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		/// <summary>
		/// Тип контрагента
		/// </summary>
		[Display(Name = "Тип контрагента")]
		public virtual PersonType? PersonType
		{
			get => _personType;
			set => SetField(ref _personType, value);
		}

		/// <summary>
		/// Тип помещения
		/// </summary>
		[Display(Name = "Тип объекта")]
		public virtual RoomType? RoomType
		{
			get => _roomType;
			set => SetField(ref _roomType, value);
		}

		/// <summary>
		/// Строки рекомендации
		/// </summary>
		[Display(Name = "Строки рекомендации")]
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

		/// <summary>
		/// Убирает строку рекомендации с указанным идентификатором номенклатуры
		/// </summary>
		/// <param name="nomenclatureId">Идентификатор номенклатуры</param>
		/// <returns></returns>
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

		/// <summary>
		/// Обновление приоритетов, согласно порядку в списке
		/// </summary>
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

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название не может быть пустым", new[] { nameof(Name) });
			}
			else if(Name.Length > 255)
			{
				yield return new ValidationResult("Название не может быть длиннее 255 символов", new[] { nameof(Name) });
			}

			if(!IsArchive)
			{
				var unitOfWorkFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();

				using(var unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Проверка параметров существующих рекомендаций"))
				{
					var result = unitOfWork.Session
						.Query<Recomendation>()
						.Where(x => !x.IsArchive
							&& x.Id != Id
							&& x.RoomType == RoomType
							&& x.PersonType == PersonType)
						.Count() == 0;

					if(!result)
					{
						yield return new ValidationResult(
							"Уже существует активная рекомендация с такими параметрами", new[]
							{
							nameof(RoomType),
							nameof(PersonType)
							});
					}
				}
			}
		}
	}
}
