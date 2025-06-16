using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Goods;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.ViewModels.Orders
{
	public partial class RecomendationsForOrderDualListViewModel :
		DualTreeViewNodesTransferViewModel<RecomendationsForOrderDualListViewModel.LeftNode, RecomendationsForOrderDualListViewModel.RightNode>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGenericRepository<Nomenclature> _nomenclaturesRepository;

		public RecomendationsForOrderDualListViewModel(
			IUnitOfWork unitOfWork,
			IGenericRepository<Nomenclature> nomenclaturesRepository,
			IEnumerable<LeftNode> leftItems,
			IEnumerable<RightNode> rightItems = null,
			Expression<Func<string, LeftNode, bool>> searchLeftPredicate = null,
			Expression<Func<string, RightNode, bool>> searchRightPredicate = null,
			Func<LeftNode, string> itemLeftDisplayFunc = null,
			Func<RightNode, string> itemRightDisplayFunc = null)
			: base(
				leftItems,
				rightItems,
				false,
				false,
				searchLeftPredicate,
				searchRightPredicate,
				itemLeftDisplayFunc,
				itemRightDisplayFunc)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_nomenclaturesRepository = nomenclaturesRepository ?? throw new ArgumentNullException(nameof(nomenclaturesRepository));
		}

		protected override void MoveSelectedItemsToRight()
		{
			var selectedItems = SelectedLeftItems.ToArray();

			var selectedItemsIds = selectedItems.Select(x => x.NomenclatureId).ToArray();

			var nomenclatures = _nomenclaturesRepository
				.Get(_unitOfWork,
					x => selectedItemsIds.Contains(x.Id))
				.ToArray();

			foreach(var item in selectedItems)
			{
				RightItems.Add(new RightNode
				{
					RecomendationId = item.RecomendationId,
					NomenclatureId = item.NomenclatureId,
					NomenclatureName = item.NomenclatureName,
					Count = 1,
					Price = nomenclatures.FirstOrDefault(x => x.Id == item.NomenclatureId).GetPrice(1)
				});
			}

			foreach(var item in selectedItems)
			{
				LeftItems.Remove(item);
			}
		}

		protected override void MoveSelectedItemsToLeft()
		{
			var selectedItems = SelectedRightItems.ToArray();

			foreach (var item in selectedItems)
			{
				LeftItems.Add(new LeftNode
				{
					RecomendationId = item.RecomendationId,
					NomenclatureId = item.NomenclatureId,
					NomenclatureName = item.NomenclatureName,
				});

				RightItems.Remove(item);
			}
		}

		public static RecomendationsForOrderDualListViewModel Create(
			IUnitOfWork unitOfWork,
			IGenericRepository<Nomenclature> nomenclatureRepository,
			IEnumerable<LeftNode> leftItems,
			IEnumerable<RightNode> rightItems = null,
			Expression<Func<string, LeftNode, bool>> searchLeftPredicate = null,
			Expression<Func<string, RightNode, bool>> searchRightPredicate = null,
			Func<LeftNode, string> itemLeftDisplayFunc = null,
			Func<RightNode, string> itemRightDisplayFunc = null)
		{
			var result = new RecomendationsForOrderDualListViewModel(
				unitOfWork,
				nomenclatureRepository,
				leftItems: leftItems,
				rightItems: rightItems,
				searchLeftPredicate: searchLeftPredicate,
				searchRightPredicate: searchRightPredicate,
				itemLeftDisplayFunc: itemLeftDisplayFunc,
				itemRightDisplayFunc: itemRightDisplayFunc);

			return result;
		}
	}
}
