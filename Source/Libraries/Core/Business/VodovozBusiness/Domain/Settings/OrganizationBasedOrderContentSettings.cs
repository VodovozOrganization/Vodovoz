using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Organizations;
using Vodovoz.Settings.Organizations;

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
			var organizationSettings = validationContext.GetService<IOrganizationSettings>();

			if(organizationSettings == null)
			{
				throw new InvalidOperationException(
					$"В контейнере зависимостей {typeof(OrganizationBasedOrderContentSettings)} не зарегистрирован сервис {nameof(IOrganizationSettings)}");
			}
			
			if(!validationContext.Items.TryGetValue("OtherSetsSettings", out var data)
				|| !(data is IList<OrganizationBasedOrderContentSettings> otherSetsData))
			{
				throw new InvalidOperationException("Не переданы данные по остальным множествам! Валидация не возможна");
			}
			
			var sb = new StringBuilder();
			IEnumerable<ProductGroup> productGroupsFromOtherSets = null;
			
			if(Nomenclatures.Any() && otherSetsData.Any())
			{
				sb.Clear();
				
				foreach(var nomenclature in Nomenclatures)
				{
					var nomenclaturesFromOtherSets = otherSetsData.SelectMany(x => x.Nomenclatures);
					var message = $"{nomenclature.Id} - {nomenclature.Name}";

					if(nomenclaturesFromOtherSets.FirstOrDefault(x => x.Id == nomenclature.Id) != null)
					{
						sb.AppendLine(message);
						continue;
					}
					
					productGroupsFromOtherSets = otherSetsData.SelectMany(x => x.ProductGroups);

					foreach(var productGroup in productGroupsFromOtherSets)
					{
						if(nomenclature.IsBelongsProductGroup(productGroup))
						{
							sb.AppendLine(message);
						}
					}
				}

				if(sb.Length > 0)
				{
					yield return new ValidationResult(
						$"В множестве {OrderContentSet} указаны пересекающиеся с другим множеством товары" +
						$" или товары принадлежат товарной группе другого множества\n{sb}");
				}
			}
			
			if(ProductGroups.Any() && otherSetsData.Any())
			{
				sb.Clear();

				if(productGroupsFromOtherSets is null)
				{
					productGroupsFromOtherSets = otherSetsData.SelectMany(x => x.ProductGroups);
				}

				foreach(var productGroup in ProductGroups)
				{
					foreach(var productGroupFromOtherSets in productGroupsFromOtherSets)
					{
						if(productGroup.IsBelongsOf(productGroupFromOtherSets))
						{
							sb.AppendLine($"{productGroup.Id} - {productGroup.Name}");
						}
					}
				}

				if(sb.Length > 0)
				{
					yield return new ValidationResult(
						$"В множестве {OrderContentSet} указаны пересекающиеся с другим множеством группы товаров\n{sb}");
				}
			}

			if(Organizations.Any())
			{
				if(!ProductGroups.Any() && !Nomenclatures.Any())
				{
					yield return new ValidationResult(
						$"Если заполнены организации у множества {OrderContentSet}, то должны быть заполнены и товары/группы товаров");
				}
			}

			if(OrderContentSet == 1 && (Organizations.Count > 1
				|| (Organizations.Count == 1 && Organizations[0].Id != organizationSettings.KulerServiceOrganizationId)))
			{
				yield return new ValidationResult(
					"В первом множестве либо не должно Организаций, либо выбрана только одна - Кулер Сервис");
			}
			
			if(OrderContentSet != 1 && Organizations.Any(x => x.Id == organizationSettings.KulerServiceOrganizationId))
			{
				yield return new ValidationResult(
					$"В {OrderContentSet} множестве не может быть организации Кулер Сервис");
			}
		}
	}
}
