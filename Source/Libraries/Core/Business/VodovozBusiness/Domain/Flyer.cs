using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Flyers;

namespace Vodovoz.Domain
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "рекламные листовки",
		Nominative = "рекламная листовка",
		Prepositional = "рекламной листовке",
		PrepositionalPlural = "рекламных листовках"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class Flyer : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private Nomenclature _flyerNomenclature;
		private bool _isForFirstOrder;
		private bool _isActive;
		private IList<FlyerActionTime> _flyerActionTimes = new List<FlyerActionTime>();
		GenericObservableList<FlyerActionTime> _observableFlyerActionTimes;
		
		public virtual int Id { get; set; }

		[Display(Name = "Номенклатура листовки")]
		public virtual Nomenclature FlyerNomenclature
		{
			get => _flyerNomenclature;
			set => SetField(ref _flyerNomenclature, value);
		}

		[Display(Name = "Только для первого заказа")]
		public virtual bool IsForFirstOrder
		{
			get => _isForFirstOrder;
			set => SetField(ref _isForFirstOrder, value);
		}

		[Display(Name = "Время действия листовки")]
		public virtual IList<FlyerActionTime> FlyerActionTimes
		{
			get => _flyerActionTimes;
			set => SetField(ref _flyerActionTimes, value);
		}
		
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<FlyerActionTime> ObservableFlyerActionTimes =>
			_observableFlyerActionTimes ??
			(_observableFlyerActionTimes = new GenericObservableList<FlyerActionTime>(FlyerActionTimes));

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(FlyerNomenclature == null)
			{
				yield return new ValidationResult("Необходимо выбрать номенклатуру",
					new[] { nameof(FlyerNomenclature) });
			}

			if(Id == 0 && FlyerNomenclature != null)
			{
				if(!(validationContext.GetService(typeof(IFlyerRepository)) is IFlyerRepository flyerRepository))
				{
					throw new ArgumentException("Не был передан необходимый аргумент IFlyerRepository");
				}

				var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
				if(flyerRepository.ExistsFlyerForNomenclatureId(uowFactory.CreateWithoutRoot(), FlyerNomenclature.Id))
				{
					yield return new ValidationResult("Листовка с данной номенклатурой уже создана",
						new[] { nameof(FlyerNomenclature) });
				}
			}

			if(!FlyerActionTimes.Any())
			{
				yield return new ValidationResult("Необходимо заполнить дату старта и активировать листовку",
					new[] { nameof(FlyerActionTimes) });
			}
		}
	}
}
