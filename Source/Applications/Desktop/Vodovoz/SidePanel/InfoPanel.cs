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
		private VBox content;

		public const int _defaultWithRequest = 300;
		private IInfoProvider _currentInfoProvider;
		private Dictionary<IInfoProvider, List<PanelViewContainer>> _providerToViewMap;

		public InfoPanel()
		{
			Build();

			content = new VBox();
			//content.WidthRequest = _defaultWithRequest;
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
			_providerToViewMap = new Dictionary<IInfoProvider, List<PanelViewContainer>>();
		}

		public void OnCurrentObjectChanged(object sender, CurrentObjectChangedArgs args)
		{
			Application.Invoke((s, arg) =>
			{
				var provider = sender as IInfoProvider;
				var views = GetListeners(provider);
				foreach(var viewContainer in views)
				{
					(viewContainer.Widget as IPanelView)?.OnCurrentObjectChanged(args.ChangedObject);
					viewContainer.Visible = viewContainer.VisibleOnPanel;
				}

				UpdatePanelVisibility();
			});
		}

		protected IEnumerable<PanelViewContainer> GetListeners(IInfoProvider provider)
		{
			List<PanelViewContainer> views;
			if(!_providerToViewMap.TryGetValue(provider, out views))
			{
				return Enumerable.Empty<PanelViewContainer>();
			}

			return views.Where(c => !c.Pinned);
		}

		protected void OnContainerUnpinned(object sender, EventArgs args)
		{
			PanelViewContainer container = sender as PanelViewContainer;
			if(this._currentInfoProvider != container.InfoProvider)
			{
				content.Remove(container);
			}

			if(container.IsOrphan())
			{
				container.Widget.Destroy();
				container.Dispose();
			}
			else
			{
				var maybePanelView = container.Widget as IPanelView;
				if(maybePanelView != null)
				{
					maybePanelView.Refresh();
					container.Visible = container.VisibleOnPanel;
				}
			}
			UpdatePanelVisibility();
		}

		public void SetInfoProvider(IInfoProvider provider)
		{
			if(_currentInfoProvider == provider)
			{
				return;
			}

			_currentInfoProvider = provider;
			//content.WidthRequest = (_currentInfoProvider as ICustomWidthInfoProvider)?.WidthRequest ?? _defaultWithRequest;
			if(!_providerToViewMap.ContainsKey(provider))
			{
				var views = PanelViewFactory.CreateAll(provider.InfoWidgets)
					.Select(v => PanelViewContainer.Wrap(v))
					.ToList();
				views.ForEach(v => v.Unpinned += OnContainerUnpinned);
				views.ForEach(v => v.InfoProvider = provider);
				views.ForEach(v => (v.Widget as IPanelView)?.Refresh());
				_providerToViewMap.Add(provider, views);
			}

			var childrenToRemove = content.Children.Where(v => !(v as PanelViewContainer).Pinned);
			foreach(var child in childrenToRemove)
			{
				content.Remove(child);
			}

			List<PanelViewContainer> newViews;
			if(!_providerToViewMap.TryGetValue(provider, out newViews))
			{
				return;
			}

			foreach(var viewContainer in newViews)
			{
				bool alreadyOnPanel = viewContainer.Pinned;
				if(!alreadyOnPanel)
				{
					content.Add(viewContainer);
					content.SetChildPacking(viewContainer, false, false, 0, PackType.Start);
					viewContainer.Visible = viewContainer.VisibleOnPanel;
				}
			}
			UpdatePanelVisibility();
		}

		public Widget GetWidget(Type type)
		{
			var currentViews = content.Children.OfType<PanelViewContainer>();
			var panelViewContainer = currentViews.FirstOrDefault(x => x.Widget.GetType() == type);

			return panelViewContainer?.Widget;
		}

		public void OnInfoProviderDisposed(IInfoProvider provider)
		{
			List<PanelViewContainer> views;
			if(_providerToViewMap.TryGetValue(provider, out views))
			{
				foreach(var view in views)
				{
					view.InfoProvider = null;
				}
				foreach(var view in views.Where(c => !c.Pinned))
				{
					content.Remove(view);
					view.Unpinned -= OnContainerUnpinned;
					view.Widget?.Destroy();
					view.Dispose();
				}
				_providerToViewMap.Remove(provider);
			}
		}

		protected void UpdatePanelVisibility()
		{
			var currentViews = content.Children.OfType<PanelViewContainer>();
			if(!(rightsidepanel1.IsHided && rightsidepanel1.ClosedByUser))
			{
				var noViews = !currentViews.Any();
				rightsidepanel1.IsHided = noViews || currentViews.All(c => !c.VisibleOnPanel);
				WidthRequest = noViews ? 0 : (_currentInfoProvider as ICustomWidthInfoProvider)?.WidthRequest ?? _defaultWithRequest;
			}
		}
	}
}

