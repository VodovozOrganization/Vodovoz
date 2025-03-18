using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Settings;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Настройки для подбора организации, основанные на составе заказа
	/// </summary>
	public class OrganizationBasedOrderContentSettings : PropertyChangedBase, IOrganizations, IDomainObject, IValidatableObject
	{
		private short _orderContentSet;
		private IObservableList<Organization> _organizations = new ObservableList<Organization>();
		
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
		public virtual IObservableList<Organization> Organizations
		{
			get => _organizations;
			set => SetField(ref _organizations, value);
		}

		/// <summary>
		/// Номер множества для настроек
		/// </summary>
		public virtual short OrderContentSet
		{
			get => _orderContentSet;
			set => SetField(ref _orderContentSet, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var unitOfWorkFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
			var orderOrganizationSettingsRepository = validationContext.GetRequiredService<IOrderOrganizationSettingsRepository>();
			
			using(var uow = unitOfWorkFactory.CreateWithoutRoot("Проверка на дубли в настройке организации для заказа"))
			{
				var sb = new StringBuilder();
				
				if(Nomenclatures.Any())
				{
					var duplicateNomenclatures = 
						orderOrganizationSettingsRepository.GetSameNomenclaturesInOrganizationBasedOrderContentSettings(
							uow, Nomenclatures.Select(x => x.Id).ToArray(), Id);

					if(duplicateNomenclatures.Any())
					{
						sb.Clear();
						
						foreach(var duplicateNomenclature in duplicateNomenclatures)
						{
							sb.AppendLine($"{duplicateNomenclature.Id} - {duplicateNomenclature.Name}");
						}
						
						yield return new ValidationResult(
							$"В множестве {OrderContentSet} указаны пересекающиеся с другим множеством товары\n{sb}");
					}
				}
				
				if(ProductGroups.Any())
				{
					var duplicateProductGroups = 
						orderOrganizationSettingsRepository.GetSameProductGroupsInOrganizationBasedOrderContentSettings(
							uow, ProductGroups, Id);

					if(duplicateProductGroups.Any())
					{
						sb.Clear();
						
						foreach(var duplicateNomenclature in duplicateProductGroups)
						{
							sb.AppendLine($"{duplicateNomenclature.Id} - {duplicateNomenclature.Name}");
						}
						
						yield return new ValidationResult(
							$"В множестве {OrderContentSet} указаны пересекающиеся с другим множеством группы товаров\n{sb}");
					}
				}

				//уточнить по пересечению организаций
				/*if(Organization != null)
				{
					if(orderOrganizationSettingsRepository.OrganizationBasedOrderContentSettingsWithOrganizationExists(
						   uow, Organization.Id))
					{
						yield return new ValidationResult($"В множестве {OrderContentSet} указана организация из другого множества");
					}

					if(!ProductGroups.Any() && !Nomenclatures.Any())
					{
						yield return new ValidationResult(
							$"Если заполнена организация у множества {OrderContentSet}, то должны быть заполнены и товары/группы товаров");
					}
				}*/
			}
		}
	}
}
