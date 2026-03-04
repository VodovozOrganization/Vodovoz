using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using NHibernate.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.DataLoader.Hierarchy;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.WageCalculation;
using Vodovoz.ViewModels.Journals.JournalNodes.WageCalculation;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.WageCalculation
{
	public class CallCenterMotivationCoefficientJournalViewModel : JournalViewModelBase
	{
		private readonly Type _productGroupType = typeof(ProductGroup);
		private readonly Type _nomenclatureType = typeof(Nomenclature);
		private IEnumerable<CallCenterMotivationCoefficientJournalNode> _groupNodes = new List<CallCenterMotivationCoefficientJournalNode>();
		private IEnumerable<CallCenterMotivationCoefficientJournalNode> _nomenclatureNodes = new List<CallCenterMotivationCoefficientJournalNode>();
		private readonly HierarchicalChunkLinqLoader<ProductGroup, CallCenterMotivationCoefficientJournalNode> _hierarchicalChunkLinqLoader;
		private readonly HashSet<CallCenterMotivationCoefficientJournalNode> _modifiedNodes = new HashSet<CallCenterMotivationCoefficientJournalNode>();
		private readonly IInteractiveService _interactiveService;
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;
		private readonly CallCenterMotivationCoefficientJournalFilterViewModel _filter;

		public CallCenterMotivationCoefficientJournalViewModel(
			CallCenterMotivationCoefficientJournalFilterViewModel filter,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IGenericRepository<Nomenclature> nomenclatureRepository)
			: base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));

			Title = "Коэффициенты мотивации КЦ";

			_filter.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filter;
			SearchEnabled = false;

			_hierarchicalChunkLinqLoader = new HierarchicalChunkLinqLoader<ProductGroup, CallCenterMotivationCoefficientJournalNode>(UnitOfWorkFactory);
			_hierarchicalChunkLinqLoader.SetRecursiveModel(GetChunk);

			var threadDataLoader = new ThreadDataLoader<CallCenterMotivationCoefficientJournalNode>(unitOfWorkFactory);
			threadDataLoader.QueryLoaders.Add(_hierarchicalChunkLinqLoader);
			DataLoader = threadDataLoader;
			DataLoader.DynamicLoadingEnabled = false;
			DataLoader.PostLoadProcessingFunc = PostLoadProcessingFunc;

			MotivationUnitTypeList = Enum.GetValues(typeof(NomenclatureMotivationUnitType)).Cast<NomenclatureMotivationUnitType?>().ToList();

			CreateActions();
		}
		
		public RecursiveTreeModel<CallCenterMotivationCoefficientJournalNode> TreeModel { get; private set; }

		public IList<NomenclatureMotivationUnitType?> MotivationUnitTypeList { get; }

		private void PostLoadProcessingFunc(IList items, uint addedSince)
		{
			_modifiedNodes.Clear();
		}

		private void CreateActions()
		{
			NodeActionsList.Clear();

			var saveAction = new JournalAction("Сохранить",
				(selected) => true,
				(selected) => true,
				(selected) => OnSave()
			);

			NodeActionsList.Add(saveAction);

			var closeAction = new JournalAction("Отмена",
				(selected) => true,
				(selected) => true,
				(selected) => Close(true, CloseSource.ClosePage)
			);

			NodeActionsList.Add(closeAction);
		}

		private void UpdateGroupAndNomenclatureNodes(IUnitOfWork unitOfWork)
		{
			var searchString = _filter.SqlSearchString;

			_groupNodes =
				(from productGroup in unitOfWork.GetAll<ProductGroup>()
					where
						(string.IsNullOrWhiteSpace(searchString)
						 || productGroup.Name.ToLower().Like(searchString)
						 || productGroup.Id.ToString().Like(searchString))
						&& (!_filter.IsHideArchived || !productGroup.IsArchive)
					orderby productGroup.Id
					select new CallCenterMotivationCoefficientJournalNode()
					{
						Id = productGroup.Id,
						Name = productGroup.Name,
						ParentId = productGroup.Parent.Id,
						IsArchive = productGroup.IsArchive,
						JournalNodeType = _productGroupType
					})
				.ToList();

			_nomenclatureNodes =
				(from nomenclature in unitOfWork.GetAll<Nomenclature>()
					where
						(string.IsNullOrWhiteSpace(searchString)
						 || nomenclature.Name.ToLower().Like(searchString)
						 || nomenclature.Id.ToString().Like(searchString))
						&& (!_filter.IsHideArchived || !nomenclature.IsArchive)
					orderby nomenclature.Id
					select new CallCenterMotivationCoefficientJournalNode()
					{
						Id = nomenclature.Id,
						Name = nomenclature.Name,
						ParentId = nomenclature.ProductGroup.Id,
						IsArchive = nomenclature.IsArchive,
						JournalNodeType = _nomenclatureType,
						MotivationUnitType = nomenclature.MotivationUnitType,
						MotivationCoefficientText = nomenclature.MotivationCoefficient.ToString()
					})
				.ToList()
				.Select(node =>
				{
					if(!string.IsNullOrEmpty(node.MotivationCoefficientText))
					{
						node.MotivationCoefficientText = node.MotivationCoefficientText.Replace('.', ',');
					}

					return node;
				})
				.ToList();
		}

		private IQueryable<CallCenterMotivationCoefficientJournalNode> GetChunk(IUnitOfWork unitOfWork, int? parentId)
		{
			UpdateGroupAndNomenclatureNodes(unitOfWork);

			return GetSubNodes(parentId);
		}

		private IQueryable<CallCenterMotivationCoefficientJournalNode> GetSubNodes(int? parentId)
		{
			if(!_filter.IsSearchStringEmpty && parentId != null)
			{
				return Enumerable.Empty<CallCenterMotivationCoefficientJournalNode>().AsQueryable();
			}

			var nodes = GetGroups(parentId).Concat(GetNomenclatures(parentId));

			return nodes.AsQueryable();
		}

		private IEnumerable<CallCenterMotivationCoefficientJournalNode> GetGroups(int? parentId)
		{
			var groups =
				from productGroup in _groupNodes
				where
					(_filter.IsSearchStringEmpty && productGroup.ParentId == parentId)
					|| !_filter.IsSearchStringEmpty
				let children = GetSubNodes(productGroup.Id)
				orderby productGroup.Id
				select new CallCenterMotivationCoefficientJournalNode()
				{
					Id = productGroup.Id,
					Name = productGroup.Name,
					ParentId = productGroup.ParentId,
					IsArchive = productGroup.IsArchive,
					JournalNodeType = _productGroupType,
					Children = children.ToList()
				};

			return groups;
		}

		private IEnumerable<CallCenterMotivationCoefficientJournalNode> GetNomenclatures(int? parentId)
		{
			if(!_filter.IsSearchStringEmpty && parentId != null)
			{
				return Enumerable.Empty<CallCenterMotivationCoefficientJournalNode>();
			}

			var nomenclatures =
				from nomenclature in _nomenclatureNodes
				where
					(_filter.IsSearchStringEmpty && nomenclature.ParentId == parentId)
					|| !_filter.IsSearchStringEmpty
				orderby nomenclature.Id
				select new CallCenterMotivationCoefficientJournalNode()
				{
					Id = nomenclature.Id,
					Name = nomenclature.Name,
					ParentId = nomenclature.ParentId,
					IsArchive = nomenclature.IsArchive,
					JournalNodeType = _nomenclatureType,
					MotivationUnitType = nomenclature.MotivationUnitType,
					MotivationCoefficientText = nomenclature.MotivationCoefficientText
				};

			return nomenclatures;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		private static List<CallCenterMotivationCoefficientJournalNode> GetNodeWithChilds(
			CallCenterMotivationCoefficientJournalNode node,
			Type targetType = null)
		{
			var result = new List<CallCenterMotivationCoefficientJournalNode>();

			if(targetType == null || node.JournalNodeType == targetType)
			{
				result.Add(node);
			}

			foreach(var child in node.Children)
			{
				result.AddRange(GetNodeWithChilds(child, targetType));
			}

			return result;
		}

		private void OnSave()
		{
			if(!_modifiedNodes.Any())
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Нет изменений, нечего сохранять.");

				return;
			}

			if(!_modifiedNodes.All(IsValidNode))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Есть неверно заполненные коэффиценты мотивации, сохранение невозможно.");

				return;
			}

			var modifiedNomenclatureIds = _modifiedNodes
				.Where(x => x.JournalNodeType == _nomenclatureType)
				.Select(node => node.Id)
				.ToArray();

			if(!_interactiveService.Question($"Сохранить изменения в {modifiedNomenclatureIds.Length} номенклатурах?"))
			{
				return;
			}

			var nomenclatures = _nomenclatureRepository.Get(UoW, n => modifiedNomenclatureIds.Contains(n.Id));

			foreach(var nomenclature in nomenclatures)
			{
				var modifiedNomenclatureNode = _modifiedNodes.Single(n => n.JournalNodeType == _nomenclatureType && n.Id == nomenclature.Id);

				nomenclature.MotivationUnitType = modifiedNomenclatureNode.MotivationUnitType;
				nomenclature.MotivationCoefficient = modifiedNomenclatureNode.MotivationCoefficient;

				UoW.Save(nomenclature);
			}

			UoW.Commit();

			_modifiedNodes.Clear();

			_interactiveService.ShowMessage(ImportanceLevel.Info, "Сохранено");
		}
		
		public RecursiveTreeModel<CallCenterMotivationCoefficientJournalNode> CreateAndSaveTreeModel()
		{
			TreeModel = new RecursiveTreeModel<CallCenterMotivationCoefficientJournalNode>(
				Items.Cast<CallCenterMotivationCoefficientJournalNode>(),
				_hierarchicalChunkLinqLoader.TreeConfig);

			return TreeModel;
		}

		public void OnMotivationUnitTypeEdited(CallCenterMotivationCoefficientJournalNode sourceNode)
		{
			var childNodes = GetNodeWithChilds(sourceNode);

			foreach(var childNode in childNodes)
			{
				childNode.MotivationUnitType = sourceNode.MotivationUnitType;
				childNode.MotivationCoefficientText = null;

				_modifiedNodes.Add(childNode);
			}
		}

		public void OnMotivationCoefficientEdited(CallCenterMotivationCoefficientJournalNode sourceNode)
		{
			if(!IsValidNode(sourceNode))
			{
				return;
			}

			var childNodes = GetNodeWithChilds(sourceNode);

			foreach(var childNode in childNodes)
			{
				childNode.MotivationCoefficientText = sourceNode.MotivationCoefficientText;
				childNode.MotivationUnitType = sourceNode.MotivationUnitType;

				_modifiedNodes.Add(childNode);
			}
		}

		public bool IsValidNode(CallCenterMotivationCoefficientJournalNode node)
		{
			var isEmptyMotivationTypeForCoefficient = node.MotivationUnitType is null && !string.IsNullOrEmpty(node.MotivationCoefficientText);

			if(isEmptyMotivationTypeForCoefficient)
			{
				return false;
			}

			var isNotNumeric = !string.IsNullOrEmpty(node.MotivationCoefficientText) && !decimal.TryParse(node.MotivationCoefficientText, out _);

			if(isNotNumeric)
			{
				return false;
			}
			
			if(node.MotivationCoefficient is null)
			{
				return true;
			}

			var isPercentValid = node.MotivationUnitType != NomenclatureMotivationUnitType.Percent
			                     || (node.MotivationCoefficient <= 100 && node.MotivationCoefficient >= 0);

			var isItemValid = node.MotivationUnitType != NomenclatureMotivationUnitType.Item
			                  || node.MotivationCoefficient >= 0;

			return isPercentValid && isItemValid;
		}

		public override void Dispose()
		{
			_filter.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
