using System;
using System.Linq;
using Gamma.GtkWidgets;
using Mango.Client;
using QS.Views.Dialog;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Dialogs.Mango;

namespace Vodovoz.Views.Mango
{
	public partial class SubscriberSelectionView : DialogViewBase<SubscriberSelectionViewModel>
	{
		private string _number;
		private Gdk.Pixbuf _userIcon = Gdk.Pixbuf.LoadFromResource("Vodovoz.icons.buttons.user22.png");
		private Gdk.Pixbuf _groupIcon = Gdk.Pixbuf.LoadFromResource("Vodovoz.icons.menu.users.png");

		public SubscriberSelectionView(SubscriberSelectionViewModel model) : base(model)
		{
			Build();
			Configure();
		}

		void Configure()
		{
			if(ViewModel.DialogType == DialogType.Transfer)
			{
				ForwardingToConsultationButton.Visible = true;
				ForwardingToConsultationButton.Clicked += Clicked_ForwardingToConsultationButton;
				ForwardingButton.Clicked += Clicked_ForwardingButton;
			}
			else if(ViewModel.DialogType == DialogType.Telephone)
			{
				ForwardingToConsultationButton.Visible = false;
				ForwardingButton.Label = "Позвонить";
				ForwardingButton.Clicked += Clicked_MakeCall;
			}

			ySearchTable.ColumnsConfig = ColumnsConfigFactory.Create<SearchTableEntity>()
				.AddColumn("Имя")
				.AddPixbufRenderer(x => x.IsGroup ? _groupIcon : _userIcon)
				.AddTextRenderer(entity => entity.Name).SearchHighlight()
				.AddColumn("Статус")
				.AddTextRenderer(entity => entity.IsReady ? $"<span foreground=\"{GdkColors.SuccessText.ToHtmlColor()}\">☎ Свободен</span>" : $"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">☎ Занят</span>", useMarkup: true)
				.AddColumn("Номер")
				.AddTextRenderer(entity => entity.Extension).SearchHighlight()
				.AddColumn("Отдел")
				.AddTextRenderer(entity => entity.Department).SearchHighlight()
				.Finish();
			ySearchTable.SetItemsSource<SearchTableEntity>(ViewModel.SearchTableEntities);
			ySearchTable.RowActivated += SelectCursorRow_OrderYTreeView;
			ySearchTable.Selection.Changed += Selection_Changed;
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			CheckSensetive();
		}

		void CheckSensetive()
		{
			_number = string.Copy(FilterEntry.Text);
			var row = ySearchTable.GetSelectedObject<SearchTableEntity>();
			ForwardingButton.Sensitive = ForwardingToConsultationButton.Sensitive = row?.IsReady == true
				|| IsNumber(ref _number);
		}

		bool IsNumber(ref string s)
		{
			if(string.IsNullOrWhiteSpace(s))
			{
				return false;
			}

			s = s.Replace("+7", "").Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");

			if(s.Length > 11)
			{
				return false;
			}
			else if(s.Length == 11 && s.First() == '8')
			{
				s = s.Remove(0, 1);
				s = "7" + s;
			}
			else if(s.Length == 10)
			{
				s = "7" + s;
			}
			else if(s.Length < 10 && s.Length > 3)
			{
				return false;
			}
			else if(s.Length < 3)
			{
				return false;
			}

			for(int i = 0; i < s.Length; i++)
			{
				if(s[i] < '0' || s[i] > '9')
				{
					return false;
				}
			}

			return true;
		}

		private void SelectCursorRow_OrderYTreeView(object sender, EventArgs e)
		{
			ForwardingToConsultationButton.Click();
		}

		protected void Clicked_MakeCall(object sender, EventArgs e)
		{
			var row = ySearchTable.GetSelectedObject<SearchTableEntity>();
			if(row != null)
			{
				ViewModel.MakeCall(row);
			}
			else
			{
				ViewModel.MakeCall(_number);
			}
		}
		protected void Clicked_ForwardingButton(object sender, EventArgs e)
		{
			var row = ySearchTable.GetSelectedObject<SearchTableEntity>();
			if(row != null) //Перевод реализован только на внутрение
			{
				ViewModel.ForwardCall(row, ForwardingMethod.blind);
			}
		}

		protected void Clicked_ForwardingToConsultationButton(object sender, EventArgs e)
		{
			var row = ySearchTable.GetSelectedObject<SearchTableEntity>();
			if(row != null) //Перевод реализован только на внутрение
			{
				ViewModel.ForwardCall(row, ForwardingMethod.hold);
			}
		}

		protected void Changed_FilterEntry(object sender, EventArgs args)
		{
			ySearchTable.SearchHighlightText = FilterEntry.Text;
			if(string.IsNullOrWhiteSpace(FilterEntry.Text))
			{
				ySearchTable.SetItemsSource(ViewModel.SearchTableEntities);
			}
			else
			{
				string input = FilterEntry.Text.ToLower();
				ySearchTable.SetItemsSource(ViewModel.SearchTableEntities
					.Where(x => (x.Extension?.Contains(input) ?? false)
					 	|| (x.Name?.ToLower().Contains(input) ?? false)
						|| (x.Department?.ToLower().Contains(input) ?? false)
				).ToList());
			}
			CheckSensetive();
		}

		protected void OnFilterEntryActivated(object sender, EventArgs e)
		{
			ySearchTable.Selection.SelectPath(new Gtk.TreePath("0"));
			if(ViewModel.DialogType == DialogType.Transfer)
			{
				ForwardingToConsultationButton.Click();
			}
			else
			{
				ForwardingButton.Click();
			}
		}
	}
}
