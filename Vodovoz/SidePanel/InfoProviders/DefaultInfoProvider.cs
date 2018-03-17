using System;

namespace Vodovoz.SidePanel.InfoProviders
{
	public class DefaultInfoProvider : IInfoProvider
	{
		#region IInfoProvider implementation

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		PanelViewType[] infoWidgets;
		public PanelViewType[] InfoWidgets
		{
			get
			{
				return infoWidgets;
			}
		}

		public QSOrmProject.IUnitOfWork UoW
		{
			get
			{
				return null;
			}
		}

		#endregion

		private static DefaultInfoProvider instance;
		public static DefaultInfoProvider Instance{
			get{
				if (instance == null)
					instance = new DefaultInfoProvider();
				return instance;
			}
		}

		private DefaultInfoProvider()
		{
			infoWidgets = new PanelViewType[0];
		}

	}
}

