using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Настройки для подбора организации, основанные на составе заказа
	/// </summary>
	public class OrganizationBasedOrderContentSettings : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private short _orderContestSet;
		private Organization _organization;
		
		/// <summary>
		/// Id
		/// </summary>
		public virtual int Id { get; set; }
		
		/// <summary>
		/// Список номенклатур, входящих в это множество
		/// </summary>
		public virtual IObservableList<Nomenclature> Nomenclatures { get; set; } = new ObservableList<Nomenclature>();
		
		/// <summary>
		/// Список групп товаров, входящих в это множество
		/// </summary>
		public virtual IObservableList<ProductGroup> ProductGroups { get; set; } = new ObservableList<ProductGroup>();

		/// <summary>
		/// Организация, устанавливаемая для этого множества
		/// </summary>
		public virtual Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		/// <summary>
		/// Номер множества для настроек
		/// </summary>
		public virtual short OrderContentSet
		{
			get => _orderContestSet;
			set => SetField(ref _orderContestSet, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var unitOfWorkFactory = (IUnitOfWorkFactory)validationContext.GetService(typeof(IUnitOfWorkFactory));

			using(var uow = unitOfWorkFactory.CreateWithoutRoot("Проверка на дубли в настройке организации для заказа"))
			{
				if(Id == 0)
				{
					if(NomenclaturesExists || ProductGroupsExists)
					{
						yield return new ValidationResult(
							"Уже есть настройка с выбранными товарами/группами, проверьте правильность выбора");
					}
					
					if(Organization != null && OrganizationExists)
					{
						yield return new ValidationResult("Уже есть настройка с выбранной организацией");
					}
				}
			}
		}

		private bool NomenclaturesExists => false;
		private bool ProductGroupsExists => false;
		private bool OrganizationExists => false;
	}
}
