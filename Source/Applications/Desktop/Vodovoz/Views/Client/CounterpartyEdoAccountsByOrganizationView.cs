using System.ComponentModel;
using System.Linq;
using Gtk;
using QS.Views;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.Views.Client
{
	[ToolboxItem(true)]
	public partial class CounterpartyEdoAccountsByOrganizationView : ViewBase<CounterpartyEdoAccountsByOrganizationViewModel>
	{
		private RadioButton _groupHolderRadioBtn;
		
		public CounterpartyEdoAccountsByOrganizationView(
			CounterpartyEdoAccountsByOrganizationViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			InitializeCurrentEdoAccountsViews();
			edoLightsMatrixView.ViewModel = ViewModel.EdoLightsMatrixViewModel;
			
			ViewModel.EdoAccountViewModelAdded += OnEdoAccountViewModelAdded;
			ViewModel.EdoAccountViewModelRemoved += OnEdoAccountViewModelRemoved;
		}

		private void InitializeCurrentEdoAccountsViews()
		{
			foreach(var edoAccountViewModel in ViewModel.EdoAccountsViewModels)
			{
				AddEdoAccountView(edoAccountViewModel);
			}

			var edoAccountViews = vboxEdoAccountsByOrganization.Children.OfType<EdoAccountView>().ToArray();

			_groupHolderRadioBtn = edoAccountViews
				.Where(x => x.ViewModel.Entity.IsDefault)
				.Select(x => x.IsDefaultAccountRbtn)
				.SingleOrDefault();

			foreach(var edoAccountView in edoAccountViews)
			{
				SetGroupRadioButton(edoAccountView);
			}

			var lightMatrixBox = (Box.BoxChild)vboxEdoAccountsByOrganization[hboxEdoLightsMatrixByOrganization];
			lightMatrixBox.PackType = PackType.End;
			lightMatrixBox.Position = 0;
		}

		private void OnEdoAccountViewModelAdded(EdoAccountViewModel edoAccountViewModel)
		{
			AddEdoAccountView(edoAccountViewModel, true);
		}

		private void AddEdoAccountView(EdoAccountViewModel edoAccountViewModel, bool manual = false)
		{
			var edoAccountView = new EdoAccountView(edoAccountViewModel);

			if(manual)
			{
				SetGroupRadioButton(edoAccountView);
			}
			
			vboxEdoAccountsByOrganization.Add(edoAccountView);
			edoAccountView.Show();
		}
		
		private void SetGroupRadioButton(EdoAccountView edoAccountView)
		{
			if(_groupHolderRadioBtn is null)
			{
				return;
			}
			
			if(edoAccountView.IsDefaultAccountRbtn != _groupHolderRadioBtn)
			{
				edoAccountView.IsDefaultAccountRbtn.Group = _groupHolderRadioBtn.Group;
			}
		}
		
		private void OnEdoAccountViewModelRemoved(EdoAccountViewModel edoAccountViewModel, int index)
		{
			var edoAccountView = vboxEdoAccountsByOrganization.Children[index];
			edoAccountView.Destroy();
		}

		protected override void OnDestroyed()
		{
			ViewModel.EdoAccountViewModelAdded -= OnEdoAccountViewModelAdded;
			base.OnDestroyed();
		}
	}
}
