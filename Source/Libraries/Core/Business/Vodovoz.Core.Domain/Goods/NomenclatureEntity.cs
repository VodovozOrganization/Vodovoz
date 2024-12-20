using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Core.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "номенклатуры",
		Nominative = "номенклатура")]
	[EntityPermission]
	[HistoryTrace]
	public class NomenclatureEntity : PropertyChangedBase, IDomainObject, IBusinessObject, INamed, IHasAttachedFilesInformations<NomenclatureFileInformation>
	{
		private int _id;
		private string _name;
		private NomenclatureCategory _category;
		private bool _isAccountableInTrueMark;
		private string _gtin;
		private MeasurementUnits _unit;
		private VAT _vAT = VAT.Vat18;
		private NomenclatureEntity _dependsOnNomenclature;
		private IObservableList<NomenclatureFileInformation> _attachedFileInformations = new ObservableList<NomenclatureFileInformation>();
		private IObservableList<NomenclaturePriceEntity> _nomenclaturePrice = new ObservableList<NomenclaturePriceEntity>();
		private IObservableList<AlternativeNomenclaturePriceEntity> _alternativeNomenclaturePrices = new ObservableList<AlternativeNomenclaturePriceEntity>();

		public NomenclatureEntity()
		{
			Category = NomenclatureCategory.water;
		}

		public virtual IUnitOfWork UoW { set; get; }

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set
			{
				if(SetField(ref _id, value))
				{
					UpdateFileInformations();
				}
			}
		}

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Категория")]
		public virtual NomenclatureCategory Category
		{
			get => _category;
			//Нельзя устанавливать, см. логику в Nomenclature.cs
			protected set => SetField(ref _category, value);
		}

		[Display(Name = "Подлежит учету в Честном Знаке")]
		public virtual bool IsAccountableInTrueMark
		{
			get => _isAccountableInTrueMark;
			set => SetField(ref _isAccountableInTrueMark, value);
		}

		[Display(Name = "Номер товарной продукции GTIN")]
		public virtual string Gtin
		{
			get => _gtin;
			set => SetField(ref _gtin, value);
		}

		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<NomenclatureFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

		/// <summary>
		/// Единица измерения
		/// </summary>
		[Display(Name = "Единица измерения")]
		public virtual MeasurementUnits Unit
		{
			get => _unit;
			set => SetField(ref _unit, value);
		}

		/// <summary>
		/// НДС
		/// </summary>
		[Display(Name = "НДС")]
		public virtual VAT VAT
		{
			get => _vAT;
			set => SetField(ref _vAT, value);
		}

		/// <summary>
		/// Цены
		/// </summary>
		[Display(Name = "Цены")]
		public virtual IObservableList<NomenclaturePriceEntity> NomenclaturePrice
		{
			get => _nomenclaturePrice;
			set => SetField(ref _nomenclaturePrice, value);
		}

		/// <summary>
		/// Альтернативные цены
		/// </summary>
		[Display(Name = "Альтернативные цены")]
		public virtual IObservableList<AlternativeNomenclaturePriceEntity> AlternativeNomenclaturePrices
		{
			get => _alternativeNomenclaturePrices;
			set => SetField(ref _alternativeNomenclaturePrices, value);
		}

		/// <summary>
		/// Влияющая номенклатура
		/// </summary>
		[Display(Name = "Влияющая номенклатура")]
		public virtual NomenclatureEntity DependsOnNomenclature
		{
			get => _dependsOnNomenclature;
			set => SetField(ref _dependsOnNomenclature, value);
		}

		/// <summary>
		/// Числовое значение НДС
		/// </summary>
		/// <returns></returns>
		[Display(Name = "Числовое значение НДС")]
		public virtual decimal VatNumericValue
		{
			get
			{
				switch(VAT)
				{
					case VAT.No:
						return 0m;
					case VAT.Vat10:
						return 0.10m;
					case VAT.Vat18:
						return 0.18m;
					case VAT.Vat20:
						return 0.20m;
					default:
						return 0m;
				}
			}
		}

		public virtual void AddFileInformation(string fileName)
		{
			if(AttachedFileInformations.Any(a => a.FileName == fileName))
			{
				return;
			}

			AttachedFileInformations.Add(new NomenclatureFileInformation
			{
				NomenclatureId = Id,
				FileName = fileName
			});
		}

		public virtual void RemoveFileInformation(string filename)
		{
			if(!AttachedFileInformations.Any(fi => fi.FileName == filename))
			{
				return;
			}

			AttachedFileInformations.Remove(AttachedFileInformations.First(x => x.FileName == filename));
		}

		private void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.NomenclatureId = Id;
			}
		}

		public virtual decimal GetPrice(decimal? itemsCount, bool useAlternativePrice = false)
		{
			if(itemsCount < 1)
			{
				itemsCount = 1;
			}

			decimal price = 0m;
			if(DependsOnNomenclature != null)
			{
				price = DependsOnNomenclature.GetPrice(itemsCount, useAlternativePrice);
			}
			else
			{
				var nomPrice = (useAlternativePrice
						? AlternativeNomenclaturePrices.Cast<NomenclaturePriceEntityBase>()
						: NomenclaturePrice.Cast<NomenclaturePriceEntityBase>())
					.OrderByDescending(p => p.MinCount)
					.FirstOrDefault(p => p.MinCount <= itemsCount);
				price = nomPrice?.Price ?? 0;
			}
			return price;
		}
	}
}
