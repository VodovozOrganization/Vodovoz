using Gamma.Utilities;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.ViewModels.Goods;

namespace Vodovoz.Goods.Recomendations
{
	[ToolboxItem(true)]
	public partial class RecomendationFilterView : FilterViewBase<RecomendationsJournalFilterViewModel>
	{
		public RecomendationFilterView(RecomendationsJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			slcbPersonType.ShowSpecialStateAll = true;
			slcbPersonType.ItemsList = Enum.GetValues(typeof(PersonType));
			slcbPersonType.SetRenderTextFunc<PersonType>(node => node.GetEnumTitle());
			slcbPersonType.Binding
				.AddBinding(ViewModel, vm => vm.PersonType, w => w.SelectedItem)
				.InitializeFromSource();

			slcbRoomType.ShowSpecialStateAll = true;
			slcbRoomType.ItemsList = Enum.GetValues(typeof(RoomType));
			slcbRoomType.SetRenderTextFunc<RoomType>(node => node.GetEnumTitle());
			slcbRoomType.Binding
				.AddBinding(ViewModel, vm => vm.RoomType, w => w.SelectedItem)
				.InitializeFromSource();
		}
	}
}
