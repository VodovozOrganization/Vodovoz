using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class InfoPanel : Gtk.Bin
	{
		VBox content;

		IInfoProvider currentInfoProvider;
		Dictionary<IInfoProvider,List<PanelViewContainer>> providerToViewMap;

		public InfoPanel()
		{
			this.Build();

			content = new VBox();
			content.WidthRequest = 250;
			content.Show();
			var eventBox = new EventBox();
			eventBox.ModifyBg(StateType.Normal, new Gdk.Color(0xff, 0xff, 0xff));
			eventBox.Add(content);
			eventBox.Show();

			ScrolledWindow window = new ScrolledWindow();
			window.HscrollbarPolicy = PolicyType.Never;
			window.VscrollbarPolicy = PolicyType.Automatic;
			window.AddWithViewport(eventBox);
			window.Show();

			rightsidepanel1.Panel = window;
			providerToViewMap = new Dictionary<IInfoProvider, List<PanelViewContainer>>();
		}

		public void OnCurrentObjectChanged(object sender, CurrentObjectChangedArgs args)
		{
			var provider = sender as IInfoProvider;
			var views = GetListeners(provider);
			foreach (var viewContainer in views)
			{
				(viewContainer.Widget as IPanelView)?.OnCurrentObjectChanged(args.ChangedObject);
				viewContainer.Visible = viewContainer.VisibleOnPanel;
			}
			UpdatePanelVisibility();
		}

		protected IEnumerable<PanelViewContainer> GetListeners(IInfoProvider provider)
		{
			List<PanelViewContainer> views;
			if (!providerToViewMap.TryGetValue(provider, out views))
				return Enumerable.Empty<PanelViewContainer>();
			return views.Where(c => !c.Pinned);
		}

		protected void OnContainerUnpinned(object sender, EventArgs args)
		{
			PanelViewContainer container = sender as PanelViewContainer;
			if(this.currentInfoProvider!=container.InfoProvider)
				content.Remove(container);
			
			if (container.IsOrphan())
			{
				container.Widget.Destroy();
				container.Dispose();
			}
			else
			{
				var maybePanelView = container.Widget as IPanelView;
				if (maybePanelView != null)
				{
					maybePanelView.Refresh();
					container.Visible = container.VisibleOnPanel;
				}
			}
			UpdatePanelVisibility();
		}

		public void SetInfoProvider(IInfoProvider provider)
		{
			if (this.currentInfoProvider == provider)
				return;
			this.currentInfoProvider = provider;
			if (!providerToViewMap.ContainsKey(provider))
			{
				var views = PanelViewFactory.CreateAll(provider.InfoWidgets)
					.Select(v => PanelViewContainer.Wrap(v))
					.ToList();
				views.ForEach(v => v.Unpinned += OnContainerUnpinned);
				views.ForEach(v => v.InfoProvider = provider);
				views.ForEach(v => (v.Widget as IPanelView)?.Refresh());
				providerToViewMap.Add(provider, views);
			}

			var childrenToRemove = content.Children.Where(v => !(v as PanelViewContainer).Pinned);
			foreach (var child in childrenToRemove)
				content.Remove(child);
			
			List<PanelViewContainer> newViews;
			if (!providerToViewMap.TryGetValue(provider,out newViews))
				return;
			foreach (var viewContainer in newViews)
			{
				bool alreadyOnPanel = viewContainer.Pinned;
				if (!alreadyOnPanel)
				{
					content.Add(viewContainer);
					content.SetChildPacking(viewContainer, false, false, 0, PackType.Start);
					viewContainer.Visible = viewContainer.VisibleOnPanel;
				}
			}
			UpdatePanelVisibility();
		}

		public void OnInfoProviderDisposed(IInfoProvider provider)
		{
			List<PanelViewContainer> views;
			if (providerToViewMap.TryGetValue(provider, out views))
			{
				foreach(var view in views)
				{
					view.InfoProvider = null;
				}
				foreach (var view in views.Where(c=>!c.Pinned))
				{
					content.Remove(view);
					view.Widget?.Destroy();
					view.Dispose();
				}
				providerToViewMap.Remove(provider);
			}
		}

		protected void UpdatePanelVisibility()
		{
			var currentViews = content.Children.OfType<PanelViewContainer>();
			if(!(rightsidepanel1.IsHided && rightsidepanel1.ClosedByUser))
				rightsidepanel1.IsHided = !currentViews.Any() || currentViews.All(c => !c.VisibleOnPanel);
		}
	}
}

