using Gamma.Utilities;
using Gtk;
using QS.Views.Dialog;
using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Goods.Recomendations;
using Vodovoz.ViewModels.Goods;

namespace Vodovoz.Goods.Recomendations
{
	public partial class RecomendationView : DialogViewBase<RecomendationViewModel>
	{
		public RecomendationView(RecomendationViewModel viewModel) : base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			yeName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			ycbIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();

			slcbPersonType.ShowSpecialStateAll = true;
			slcbPersonType.ItemsList = Enum.GetValues(typeof(PersonType));
			slcbPersonType.SetRenderTextFunc<PersonType>(node => node.GetEnumTitle());
			slcbPersonType.Binding
				.AddBinding(ViewModel.Entity, e => e.PersonType, w => w.SelectedItem)
				.InitializeFromSource();

			slcbRoomType.ShowSpecialStateAll = true;
			slcbRoomType.ItemsList = Enum.GetValues(typeof(RoomType));
			slcbRoomType.SetRenderTextFunc<RoomType>(node => node.GetEnumTitle());
			slcbRoomType.Binding
				.AddBinding(ViewModel.Entity, e => e.RoomType, w => w.SelectedItem)
				.InitializeFromSource();

			ytwRecomendationItems.Selection.Mode = SelectionMode.Multiple;
			ytwRecomendationItems.Reorderable = true;
			ytwRecomendationItems.CreateFluentColumnsConfig<RecomendationItem>()
				.AddColumn("Приоритет")
					.AddNumericRenderer(x => x.Priority)
				.AddColumn("Код\nТМЦ")
					.AddNumericRenderer(x => x.NomenclatureId)
				.AddColumn("Номенклатура")
					.AddTextRenderer(x => ViewModel.NomenclatureCacheRepository.GetTitleById(x.NomenclatureId))
				.Finish();

			ytwRecomendationItems.ItemsDataSource = ViewModel.Entity.Items;

			ytwRecomendationItems.Binding
				.AddBinding(
					ViewModel,
					vm => vm.SelectedRecomendationItemObjects,
					w => w.SelectedRows)
				.InitializeFromSource();

			ytwRecomendationItems.DragEnd += OnItemsReordered;

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);

			ybAddNomenclature.BindCommand(ViewModel.AddNomenclatureCommand);
			ybRemoveNomenclature.BindCommand(ViewModel.RemoveNomenclatureCommand);
		}

		private void OnItemsReordered(object o, DragEndArgs args)
		{
			ViewModel.UpdatePriorityCommand.Execute();
		}

		public override void Destroy()
		{
			ytwRecomendationItems.ItemsDataSource = null;
			base.Destroy();
		}
	}
}
