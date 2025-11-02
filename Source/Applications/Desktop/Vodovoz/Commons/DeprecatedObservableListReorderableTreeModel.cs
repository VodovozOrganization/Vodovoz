using System;
using System.Data.Bindings;
using Gtk;

namespace Vodovoz
{
	[Obsolete("Добавлен для совместимости со старым кодом. Вместо него использовать модели из Gamma.Binding")]
	public class DeprecatedObservableListReorderableTreeModel : DeprecatedObservableListTreeModel, TreeDragSourceImplementor, TreeDragDestImplementor
	{
		public DeprecatedObservableListReorderableTreeModel(IObservableList list) : base(list)
		{
			
		}

		public bool RowDraggable (TreePath path)
		{
			return true;
		}

		public bool RowDropPossible(TreePath path, SelectionData sel)
		{
			Console.WriteLine("RowDropPossible path={0} depth= {1}", path, path.Depth);
			return path.Depth == 1;
		}

		public bool DragDataGet(TreePath path, SelectionData sel)
		{
			Console.WriteLine("DragDataGet path={0}", path);
			return Tree.SetRowDragData(sel, Adapter, path);
		}

		public bool DragDataReceived(TreePath path, SelectionData data)
		{
			Console.WriteLine("DragDataReceived dstPath={0}", path);
			TreeModel srcModel;
			TreePath srcPath;
			if(Tree.GetRowDragData(data, out srcModel, out srcPath))
			{
				Console.WriteLine("DragDataReceived srcPath={0}", srcPath);
				object row = NodeAtPath (srcPath);
				SourceList.RemoveAt (srcPath.Indices[0]);
				if (srcPath.Indices [0] < path.Indices [0])
					path.Prev ();
					
				if (path.Indices [0] == SourceList.Count)
					SourceList.Add (row);
				else
					SourceList.Insert (path.Indices [0], row);
				return true;
			}
			return false;
		}

		public bool DragDataDelete(TreePath path)
		{
			TreeIter iter;
			this.GetIter(out iter, path);
			//this.Remove(ref iter);
			return true;
		}
	}
}

