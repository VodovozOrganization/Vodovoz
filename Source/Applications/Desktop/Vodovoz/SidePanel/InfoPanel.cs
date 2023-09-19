using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class InfoPanel : Bin
	{
		public const int _defaultWithRequest = 250;
		private readonly Dictionary<IInfoProvider, List<PanelViewContainer>> _providerToViewMap;
		private IInfoProvider _currentInfoProvider;
		private VBox _content;

		public InfoPanel()
		{
			Build();

			_content = new VBox();
			_content.Show();
			_content.WidthRequest = _defaultWithRequest;

			var eventBox = new EventBox();
			eventBox.ModifyBg(StateType.Normal, new Gdk.Color(0xff, 0xff, 0xff));
			eventBox.Add(_content);
			eventBox.Show();

			ScrolledWindow window = new ScrolledWindow
			{
				HscrollbarPolicy = PolicyType.Never,
				VscrollbarPolicy = PolicyType.Automatic
			};

			window.AddWithViewport(eventBox);
			window.Show();

			rightsidepanel1.Panel = window;

			_providerToViewMap = new Dictionary<IInfoProvider, List<PanelViewContainer>>();
		}

		public void OnCurrentObjectChanged(object sender, CurrentObjectChangedArgs args)
		{
			Gtk.Application.Invoke((s, arg) =>
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
			if(!_providerToViewMap.TryGetValue(
				provider,
				out List<PanelViewContainer> views))
			{
				return Enumerable.Empty<PanelViewContainer>();
			}

			return views.Where(c => !c.Pinned);
		}

		protected void OnContainerUnpinned(object sender, EventArgs args)
		{
			PanelViewContainer container = sender as PanelViewContainer;

			if(_currentInfoProvider != container.InfoProvider)
			{
				_content.Remove(container);
			}

			if(container.IsOrphan())
			{
				container.Widget.Destroy();
				container.Dispose();
			}
			else
			{
				if(container.Widget is IPanelView panelView)
				{
					panelView.Refresh();
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

			if(!_providerToViewMap.ContainsKey(provider))
			{
				var views = PanelViewFactory
					.CreateAll(provider.InfoWidgets)
					.Select(v => PanelViewContainer.Wrap(v))
					.ToList();

				views.ForEach(v => v.Unpinned += OnContainerUnpinned);
				views.ForEach(v => v.InfoProvider = provider);
				views.ForEach(v => (v.Widget as IPanelView)?.Refresh());

				_providerToViewMap.Add(provider, views);
			}

			var childrenToRemove = _content.Children.Where(v => !(v as PanelViewContainer).Pinned);

			foreach(var child in childrenToRemove)
			{
				_content.Remove(child);
			}

			if(!_providerToViewMap.TryGetValue(
				provider,
				out List<PanelViewContainer> newViews))
			{
				return;
			}

			foreach(var viewContainer in newViews)
			{
				bool alreadyOnPanel = viewContainer.Pinned;

				if(!alreadyOnPanel)
				{
					_content.Add(viewContainer);
					_content.SetChildPacking(viewContainer, false, false, 0, PackType.Start);

					viewContainer.Visible = viewContainer.VisibleOnPanel;
				}
			}
			UpdatePanelVisibility();
		}

		public Widget GetWidget(Type type)
		{
			var currentViews = _content.Children.OfType<PanelViewContainer>();
			var panelViewContainer = currentViews.FirstOrDefault(x => x.Widget.GetType() == type);

			return panelViewContainer?.Widget;
		}

		public void OnInfoProviderDisposed(IInfoProvider provider)
		{
			if(_providerToViewMap.TryGetValue(
				provider,
				out List<PanelViewContainer> views))
			{
				foreach(var view in views)
				{
					view.InfoProvider = null;
				}

				foreach(var view in views.Where(c => !c.Pinned))
				{
					_content.Remove(view);

					view.Unpinned -= OnContainerUnpinned;
					view.Widget?.Destroy();
					view.Dispose();
				}

				_providerToViewMap.Remove(provider);
			}
		}

		protected void UpdatePanelVisibility()
		{
			var currentViews = _content.Children.OfType<PanelViewContainer>();

			if(!(rightsidepanel1.IsHided && rightsidepanel1.ClosedByUser))
			{
				rightsidepanel1.IsHided = true;
				var noViews = !currentViews.Any();
				rightsidepanel1.IsHided = noViews || currentViews.All(c => !c.VisibleOnPanel);
				_content.WidthRequest = (_currentInfoProvider as ICustomWidthInfoProvider)?.WidthRequest ?? _defaultWithRequest;
			}
		}
	}
}
