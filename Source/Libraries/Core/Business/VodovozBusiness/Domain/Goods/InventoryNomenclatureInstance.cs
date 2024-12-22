using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "экземпляры инвентарной номенклатуры",
		Nominative = "экземпляр инвентарной номенклатуры",
		Prepositional = "экземпляре инвентарной номенклатуры",
		PrepositionalPlural = "экземплярах инвентарной номенклатуры"
	)]
	[EntityPermission]
	[HistoryTrace]
	public class InventoryNomenclatureInstance : NomenclatureInstance
	{
		private bool _isArchive;
		private bool _isUsed;
		private string _inventoryNumber;
		
		[Display(Name = "Инвентарный номер")]
		public virtual string InventoryNumber
		{
			get => _inventoryNumber;
			set => SetField(ref _inventoryNumber, value);
		}
		
		[Display(Name = "Архивный?")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}
		
		/// <summary>
		/// Бывшее в употреблении (б/у)
		/// </summary>
		[Display(Name = "Бывшее в употреблении", ShortName = "Б/у")]
		public virtual bool IsUsed
		{
			get => _isUsed;
			set => SetField(ref _isUsed, value);
		}

		public override NomenclatureInstanceType Type => NomenclatureInstanceType.InventoryNomenclatureInstance;

		public virtual string Name => Nomenclature != null ? Nomenclature.Name : string.Empty;
		public virtual string GetInventoryNumber => GetInventoryNumberString(IsUsed, InventoryNumber);

		public override string ToString()
		{
			if(Id == 0)
			{
				return "Новый экземпляр";
			}
			
			return $"{Name} инв. номер: {GetInventoryNumber}";
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
			foreach(var validationResult in base.Validate(validationContext))
			{
				yield return validationResult;
			}

			if(string.IsNullOrWhiteSpace(InventoryNumber))
			{
				yield return new ValidationResult("Инвентарный номер не заполнен");
			}
			else if(Nomenclature != null)
			{
				using(var uow = uowFactory.CreateWithoutRoot())
				{
					var duplicatesInventory = uow.GetAll<InventoryNomenclatureInstance>()
						.Where(x => x.Id != Id
							&& x.Nomenclature.Id == Nomenclature.Id
							&& x.InventoryNumber == InventoryNumber)
						.ToList();
					
					if(duplicatesInventory.Any())
					{
						yield return new ValidationResult($"Данный инвентарный номер уже присвоен: {duplicatesInventory.First()}");
					}
				}
			}
		}

		public static string GetInventoryNumberString(bool isUsed, string inventoryNumber) =>
			isUsed ? $"Б/У - {inventoryNumber}" : $"{inventoryNumber}";
	}
}
