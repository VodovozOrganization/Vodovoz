﻿using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Project.Filter;
using QS.Project.Journal;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Goods
{
	public class NomenclatureFilterViewModel : FilterViewModelBase<NomenclatureFilterViewModel>, IJournalFilterViewModel
	{
		NomenclatureCategory[] availableCategories;
		public virtual NomenclatureCategory[] AvailableCategories {
			get {
				return availableCategories != null && availableCategories.Any()
					? availableCategories
					: Enum.GetValues(typeof(NomenclatureCategory)).OfType<NomenclatureCategory>().ToArray();
			}
			set => UpdateFilterField(ref availableCategories, value);
		}

		NomenclatureCategory? restrictCategory;
		[PropertyChangedAlso(
			nameof(IsDispossableTareApplicable),
			nameof(AreDilersApplicable),
			nameof(IsSaleCategoryApplicable),
			nameof(SelectCategory)
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

		bool restrictArchive;
		public virtual bool RestrictArchive {
			get => restrictArchive;
			set {
				UpdateFilterField(ref restrictArchive, value);
				CanChangeShowArchive = true;
			}
		}
		public bool CanChangeShowArchive { get; private set; } = true;

		[PropertyChangedAlso(
			nameof(IsDispossableTareApplicable),
			nameof(AreDilersApplicable),
			nameof(IsSaleCategoryApplicable),
			nameof(RestrictCategory)
		)]
		public NomenclatureCategory? SelectCategory {
			get => restrictCategory;
			set => UpdateFilterField(ref restrictCategory, value);
		}

		SaleCategory[] availableSalesCategories;
		public SaleCategory[] AvailableSalesCategories {
			get {
				return availableSalesCategories != null && availableSalesCategories.Any()
					? availableSalesCategories
					: Enum.GetValues(typeof(SaleCategory)).OfType<SaleCategory>().ToArray();
			}
			set => UpdateFilterField(ref availableSalesCategories, value);
		}

		public SaleCategory? SelectSaleCategory {
			get => restrictSaleCategory;
			set => UpdateFilterField(ref restrictSaleCategory, value);
		}

		private IEnumerable<int> restrictedExcludedIds;
		public virtual IEnumerable<int> RestrictedExcludedIds {
			get => restrictedExcludedIds;
			set => UpdateFilterField(ref restrictedExcludedIds, value);
		}

		private Warehouse restrictedLoadedWarehouse;
		public virtual Warehouse RestrictedLoadedWarehouse {
			get => restrictedLoadedWarehouse;
			set => UpdateFilterField(ref restrictedLoadedWarehouse, value);
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

		public bool IsDefectiveBottle { get; set; }
		public bool IsShow { get; set; }
	}
}
