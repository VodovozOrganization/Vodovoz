using Bitrix;
using Bitrix.DTO;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;

namespace BitrixIntegration.Processors
{
	public class ProductGroupProcessor : IProductGroupProcessor
	{
		private readonly IBitrixClient _bitrixClient;
		private readonly INomenclatureRepository _nomenclatureRepository;

		public ProductGroupProcessor(IBitrixClient bitrixClient,
			INomenclatureRepository nomenclatureRepository)
		{
			_bitrixClient = bitrixClient ?? throw new ArgumentNullException(nameof(bitrixClient));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
		}

		public ProductGroup ProcessProductGroup(IUnitOfWork uow, DealProductItem productFromDeal)
		{
			var product = _bitrixClient.GetProduct(productFromDeal.ProductId).GetAwaiter().GetResult();
			if(product == null)
			{
				throw new Exception($"Продукт с id {productFromDeal.ProductId} не найден в битриксе");
			}

			var allProductGroups = product.Category.IsOurProduct.Split('/');
			var lastGroupName = allProductGroups[allProductGroups.Length - 1];

			if(MatchNomenclatureGroupByName(uow, lastGroupName, out var productGroup))
			{
				return productGroup;
			}

			var reversedProductGroups = allProductGroups.Take(allProductGroups.Length - 1).Reverse();

			IList<ProductGroup> allNewProductGroups = new List<ProductGroup>
			{
				new ProductGroup { Name = lastGroupName }
			};

			foreach(var reversedProductGroupName in reversedProductGroups)
			{
				if(MatchNomenclatureGroupByName(uow, reversedProductGroupName, out var matchedProductGroup))
				{
					allNewProductGroups.Add(matchedProductGroup);
				}
				else
				{
					var newUnmatchedProductGroup = new ProductGroup()
					{
						Name = reversedProductGroupName
					};
					allNewProductGroups.Add(newUnmatchedProductGroup);
				}
			}

			if(allNewProductGroups.Any())
			{
				for(var i = 0; i < allNewProductGroups.Count - 1; i++)
				{
					allNewProductGroups[i].Parent = allNewProductGroups[i + 1];
					uow.Save(allNewProductGroups[i]);
				}
			}

			uow.Save(allNewProductGroups.Last());
			return allNewProductGroups.First();
		}

		private bool MatchNomenclatureGroupByName(IUnitOfWork uow, string lastGroup, out ProductGroup outProductGroup)
		{
			var group = _nomenclatureRepository.GetProductGroupByName(uow, lastGroup);
			if(group != null)
			{
				outProductGroup = group;
				return true;
			}

			outProductGroup = null;
			return false;
		}
	}
}
