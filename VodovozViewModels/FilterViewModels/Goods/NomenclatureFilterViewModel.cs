using System;
using System.Linq;
using QS.Project.Filter;
using QS.Services;
using Vodovoz.Domain.Goods;
using QS.DomainModel.Entity;

namespace Vodovoz.FilterViewModels.Goods
{
	public class NomenclatureFilterViewModel : FilterViewModelBase<NomenclatureFilterViewModel>
	{
		public NomenclatureFilterViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{ }

		NomenclatureCategory[] availableCategories;
		[PropertyChangedAlso(nameof(SelectedCategories))]
		public virtual NomenclatureCategory[] AvailableCategories {
			get => availableCategories;
			set => UpdateFilterField(ref availableCategories, value);
		}

		NomenclatureCategory? restrictCategory;
		[PropertyChangedAlso(
			nameof(IsDispossableTareApplicable),
			nameof(AreDilersApplicable),
			nameof(IsSaleCategoryApplicable),
			nameof(SelectedCategories)
		)]
		public virtual NomenclatureCategory? RestrictCategory {
			get => restrictCategory;
			set {
				if(UpdateFilterField(ref restrictCategory, value))
					CanChangeCategory = false;
			}
		}
		public bool CanChangeCategory { get; private set; } = true;

		SaleCategory? restrictSaleCategory;
		public virtual SaleCategory? RestrictSaleCategory {
			get => restrictSaleCategory;
			set {
				if(UpdateFilterField(ref restrictSaleCategory, value))
					CanChangeSaleCategory = false;
			}
		}
		public bool CanChangeSaleCategory { get; private set; } = true;

		bool restrictDisposbleTare;
		public virtual bool RestrictDisposbleTare {
			get => restrictDisposbleTare;
			set {
				if(UpdateFilterField(ref restrictDisposbleTare, value))
					CanChangeShowDisposableTare = false;
			}
		}
		public bool CanChangeShowDisposableTare { get; private set; } = true;

		bool restrictDilers;
		public virtual bool RestrictDilers {
			get => restrictDilers;
			set {
				if(UpdateFilterField(ref restrictDilers, value))
					CanChangeShowDilers = false;
			}
		}
		public bool CanChangeShowDilers { get; private set; } = true;

		public NomenclatureCategory[] SelectedCategories {
			get {
				if(!RestrictCategory.HasValue)
					return AvailableCategories != null && AvailableCategories.Any()
						? AvailableCategories
						: Enum.GetValues(typeof(NomenclatureCategory)).OfType<NomenclatureCategory>().ToArray();
				return new NomenclatureCategory[] { RestrictCategory.Value };
			}
		}

		public SaleCategory[] SelectedSubCategories {
			get {
				if(!RestrictSaleCategory.HasValue)
					return Enum.GetValues(typeof(SaleCategory)).OfType<SaleCategory>().ToArray();
				return new SaleCategory[] { RestrictSaleCategory.Value };
			}
		}

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
	}
}
