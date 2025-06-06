using QS.DomainModel.Entity;
using QS.Project.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Goods
{
	public class NomenclatureFilterViewModel : FilterViewModelBase<NomenclatureFilterViewModel>
	{
		private NomenclatureCategory[] _availableCategories;
		private NomenclatureCategory? _restrictCategory;
		private SaleCategory? _restrictSaleCategory;
		private bool _restrictDisposbleTare;
		private bool _restrictDilers;
		private bool _restrictArchive;
		private bool _onlyOnlineNomenclatures;
		private SaleCategory[] _availableSalesCategories;
		private IEnumerable<int> _restrictedExcludedIds;
		private Warehouse _restrictedLoadedWarehouse;
		private GlassHolderType? _glassHolderType;

		public NomenclatureFilterViewModel(Action<NomenclatureFilterViewModel> filterParams = null)
		{
			if(filterParams != null)
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}
		
		public virtual NomenclatureCategory[] AvailableCategories {
			get {
				return _availableCategories != null && _availableCategories.Any()
					? _availableCategories
					: Enum.GetValues(typeof(NomenclatureCategory)).OfType<NomenclatureCategory>().ToArray();
			}
			set => UpdateFilterField(ref _availableCategories, value);
		}

		[PropertyChangedAlso(
			nameof(IsDispossableTareApplicable),
			nameof(AreDilersApplicable),
			nameof(IsSaleCategoryApplicable),
			nameof(SelectCategory)
		)]
		public virtual NomenclatureCategory? RestrictCategory {
			get => _restrictCategory;
			set {
				if(SetField(ref _restrictCategory, value))
				{
					CanChangeCategory = false;

					if(RestrictCategory != NomenclatureCategory.equipment)
					{
						GlassHolderType = null;
					}

					Update();
				}
			}
		}

		public bool CanChangeCategory { get; private set; } = true;

		public virtual SaleCategory? RestrictSaleCategory {
			get => _restrictSaleCategory;
			set {
				if(UpdateFilterField(ref _restrictSaleCategory, value))
					CanChangeSaleCategory = false;
			}
		}
		public bool CanChangeSaleCategory { get; private set; } = true;

		public virtual bool RestrictDisposbleTare {
			get => _restrictDisposbleTare;
			set {
				if(UpdateFilterField(ref _restrictDisposbleTare, value))
					CanChangeShowDisposableTare = false;
			}
		}
		public bool CanChangeShowDisposableTare { get; private set; } = true;

		public virtual bool RestrictDilers {
			get => _restrictDilers;
			set {
				if(UpdateFilterField(ref _restrictDilers, value))
					CanChangeShowDilers = false;
			}
		}
		public bool CanChangeShowDilers { get; private set; } = true;

		public virtual bool RestrictArchive {
			get => _restrictArchive;
			set {
				UpdateFilterField(ref _restrictArchive, value);
			}
		}

		public bool CanChangeShowArchive{ get; set; } = true;

		[PropertyChangedAlso(
			nameof(IsDispossableTareApplicable),
			nameof(AreDilersApplicable),
			nameof(IsSaleCategoryApplicable),
			nameof(RestrictCategory)
		)]
		public NomenclatureCategory? SelectCategory {
			get => _restrictCategory;
			set => UpdateFilterField(ref _restrictCategory, value);
		}

		public SaleCategory[] AvailableSalesCategories {
			get {
				return _availableSalesCategories != null && _availableSalesCategories.Any()
					? _availableSalesCategories
					: Enum.GetValues(typeof(SaleCategory)).OfType<SaleCategory>().ToArray();
			}
			set => UpdateFilterField(ref _availableSalesCategories, value);
		}

		public SaleCategory? SelectSaleCategory {
			get => _restrictSaleCategory;
			set => UpdateFilterField(ref _restrictSaleCategory, value);
		}

		public virtual IEnumerable<int> RestrictedExcludedIds {
			get => _restrictedExcludedIds;
			set => UpdateFilterField(ref _restrictedExcludedIds, value);
		}

		public virtual Warehouse RestrictedLoadedWarehouse {
			get => _restrictedLoadedWarehouse;
			set => UpdateFilterField(ref _restrictedLoadedWarehouse, value);
		}

		public GlassHolderType? GlassHolderType
		{
			get => _glassHolderType;
			set => UpdateFilterField(ref _glassHolderType, value);
		}

		/// <summary>
		/// Показывать только номенклатуры, отправляемые в ИПЗ(сайт и МП)
		/// </summary>
		public virtual bool OnlyOnlineNomenclatures
		{
			get => _onlyOnlineNomenclatures;
			set => UpdateFilterField(ref _onlyOnlineNomenclatures, value);
		}

		public bool CanChangeOnlyOnlineNomenclatures{ get; set; } = true;

		public bool IsSaleCategoryApplicable {
			get {
				if(RestrictCategory.HasValue)
					return Nomenclature.GetCategoriesWithSaleCategory().Contains(RestrictCategory.Value);
				return false;
			}
		}

		public bool AreDilersApplicable {
			get {
				if(RestrictCategory.HasValue)
					return RestrictCategory.Value == NomenclatureCategory.water;
				return false;
			}
		}

		public bool IsDispossableTareApplicable {
			get {
				if(RestrictCategory.HasValue)
					return RestrictCategory.Value == NomenclatureCategory.water;
				return false;
			}
		}

		public bool IsDefectiveBottle { get; set; }
		public override bool IsShow { get; set; } = true;
	}
}
