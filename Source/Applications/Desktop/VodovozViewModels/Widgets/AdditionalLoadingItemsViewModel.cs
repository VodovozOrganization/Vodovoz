using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Widgets
{
	public class AdditionalLoadingItemsViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly ITdiCompatibilityNavigation _navigationManager;
		private readonly IInteractiveService _interactiveService;
		private IUnitOfWork _uow;
		private ITdiTab _master;
		private AdditionalLoadingDocument _additionalLoadingDocument;

		public AdditionalLoadingItemsViewModel(
			IUnitOfWork uow,
			ITdiTab master,
			ITdiCompatibilityNavigation navigationManager,
			IInteractiveService interactiveService)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_master = master ?? throw new ArgumentNullException(nameof(master));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		private PropertyInfo _targetProperyInfo;
		private bool _canEdit;

		public void BindWithSource<T>(T source, Expression<Func<T, AdditionalLoadingDocument>> additionaLoadingDocumentFunc)
			where T : INotifyPropertyChanged
		{
			_targetProperyInfo = PropertyUtil.GetPropertyInfo(additionaLoadingDocumentFunc);
			source.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == _targetProperyInfo.Name)
				{
					AdditionalLoadingDocument = (AdditionalLoadingDocument)_targetProperyInfo.GetValue(source);
				}
			};
			AdditionalLoadingDocument = (AdditionalLoadingDocument)_targetProperyInfo.GetValue(source);
		}

		public AdditionalLoadingDocument AdditionalLoadingDocument
		{
			get => _additionalLoadingDocument;
			set => SetField(ref _additionalLoadingDocument, value);
		}

		public bool CanEdit
		{
			get => _canEdit;
			set => SetField(ref _canEdit, value);
		}

		public void RemoveItems(IEnumerable<AdditionalLoadingDocumentItem> items)
		{
			foreach(var item in items)
			{
				AdditionalLoadingDocument.ObservableItems.Remove(item);
			}
		}

		public void AddItem()
		{
			var journal = _navigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel>(
				_master,
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.SelectionMode = JournalSelectionMode.Multiple;
				}
			).ViewModel;
			
			journal.OnSelectResult += OnNomenclaturesSelected;
		}

		private void OnNomenclaturesSelected(object sender, JournalSelectedEventArgs args)
		{
			if(AdditionalLoadingDocument == null)
			{
				return;
			}
			var selectedNodes = args.SelectedObjects.Cast<NomenclatureJournalNode>();
			var selectedNomenclatures = _uow.GetById<Nomenclature>(selectedNodes.Select(x => x.Id));
			var notValidWeightOrVolume = new List<Nomenclature>();
			var notValidCategory = new List<Nomenclature>();

			foreach(var nomenclature in selectedNomenclatures)
			{
				if(AdditionalLoadingDocument.ObservableItems.Any(x => x.Nomenclature.Id == nomenclature.Id))
				{
					continue;
				}
				if(nomenclature.Weight == 0 || nomenclature.Volume == 0)
				{
					notValidWeightOrVolume.Add(nomenclature);
					continue;
				}
				if(!Nomenclature.CategoriesWithWeightAndVolume.Contains(nomenclature.Category))
				{
					notValidCategory.Add(nomenclature);
					continue;
				}

				{
					AdditionalLoadingDocument.ObservableItems.Add(new AdditionalLoadingDocumentItem
					{
						Nomenclature = nomenclature,
						AdditionalLoadingDocument = AdditionalLoadingDocument,
						Amount = 1
					});
				}
			}

			if(notValidWeightOrVolume.Any())
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning,
					"Можно добавлять только номенклатуры с заполненным объёмом и весом.\n" +
					"Номенклатуры, которые не были добавлены:\n\n" +
					$"{string.Join("\n", notValidWeightOrVolume.Select(x => $"{x.Id} {x.Name}"))}");
			}
			else if(notValidCategory.Any())
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning,
					"Можно добавлять только номенклатуры с категорией из списка: " +
					$"{string.Join(", ", Nomenclature.CategoriesWithWeightAndVolume.Select(x => $"<b>{x.GetEnumTitle()}</b>"))}.\n" +
					"Номенклатуры, которые не были добавлены:\n\n" +
					$"{string.Join("\n", notValidCategory.Select(x => $"{x.Id} {x.Category.GetEnumTitle()} {x.Name}"))}");
			}
		}

		public void Dispose()
		{
			_uow = null;
			_master = null;
		}
	}
}
