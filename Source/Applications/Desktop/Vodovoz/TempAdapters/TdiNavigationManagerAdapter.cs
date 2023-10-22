using QS.Dialog;
using QS.Navigation;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.ViewModels.Dialog;
using QS.Views.Resolve;
using System;
using System.Linq;
using System.Reflection;

namespace Vodovoz.TempAdapters
{
	internal class TdiNavigationManagerAdapter : TdiNavigationManager
	{
		public TdiNavigationManagerAdapter(
			TdiNotebook tdiNotebook,
			IViewModelsPageFactory viewModelsFactory,
			IInteractiveMessage interactive,
			IPageHashGenerator hashGenerator = null,
			ITdiPageFactory tdiPageFactory = null,
			AutofacViewModelsGtkPageFactory viewModelsGtkPageFactory = null,
			IGtkViewResolver viewResolver = null)
			: base(tdiNotebook, viewModelsFactory, interactive, hashGenerator, tdiPageFactory, viewModelsGtkPageFactory, viewResolver)
		{
		}

		public override IPage FindPage(DialogViewModelBase viewModel)
		{
			var page = base.FindPage(viewModel);

			if(page != null)
			{
				return page;
			}

			var lostTabInfo = tdiNotebook.Tabs.FirstOrDefault(x => x.TdiTab == viewModel || (x.TdiTab is TdiSliderTab sliderTab && sliderTab.Journal == viewModel));

			if(lostTabInfo is null)
			{
				return null;
			}

			var genericViewModelPageType = typeof(TdiPage<>).MakeGenericType(viewModel.GetType());

			ConstructorInfo constructorInfo = genericViewModelPageType.GetConstructor(new Type[] { viewModel.GetType(), typeof(ITdiTab), typeof(string) });

			page = (IPage)constructorInfo.Invoke(new object[] { viewModel, lostTabInfo.TdiTab, null});

			pages.Add(page);

			return page;
		}

		public override IPage FindPage(ITdiTab tab)
		{
			var page = base.FindPage(tab);

			if(page != null)
			{
				return page;
			}

			var lostTab = tdiNotebook.Tabs.FirstOrDefault(x => x.TdiTab == tab
				|| (x.TdiTab is TdiSliderTab sliderTab && sliderTab == tab));

			if(lostTab is null)
			{
				return null;
			}

			page = new TdiTabPage(lostTab.TdiTab, null);

			pages.Add(page);

			return page;
		}
	}
}
