using System;
using System.Collections;
using System.Data.Bindings;
using System.Runtime.InteropServices;
using Gamma.Binding;
using Gtk;

namespace Vodovoz
{
	[Obsolete("Добавлен для совместимости со старым кодом. Вместо него использовать модели из Gamma.Binding")]
	public class DeprecatedObservableListTreeModel : GLib.Object, TreeModelImplementor, IyTreeModel
	{
		TreeModel adapter;
		IObservableList sourceList;
		IEnumerator cachedEnumerator;

		public event EventHandler RenewAdapter;

		public DeprecatedObservableListTreeModel(IObservableList list)
		{
			adapter = new TreeModelAdapter (this);
			sourceList = list;
			sourceList.ElementChanged += SourceList_ElementChanged;
			sourceList.ElementAdded += SourceList_ElementAdded;
			sourceList.ElementRemoved += SourceList_ElementRemoved;
			sourceList.ListChanged += SourceList_ListChanged;
		}

		void SourceList_ListChanged (object aList)
		{
			adapter = new TreeModelAdapter (this);
			OnRenewAdapter ();
		}

		public void EmitModelChanged()
		{
			OnRenewAdapter();
		}

		void OnRenewAdapter()
		{
			if (RenewAdapter != null)
				RenewAdapter (this, EventArgs.Empty);
		}

		void SourceList_ElementRemoved (object aList, int[] aIdx, object aObject)
		{
			TreePath path = new TreePath(aIdx);

			Adapter.EmitRowDeleted (path);
		}

		void SourceList_ElementAdded (object aList, int[] aIdx)
		{
			object value = (aList as IList) [aIdx[0]];
			TreeIter iter = IterFromNode(value);
			TreePath path = PathFromNode (value);

			Adapter.EmitRowInserted (path, iter);
		}

		void SourceList_ElementChanged (object aList, int[] aIdx)
		{
			object value = (aList as IList) [aIdx[0]];
			TreeIter iter = IterFromNode(value);
			TreePath path = PathFromNode (value);

			Adapter.EmitRowChanged (path, iter);
		}
			
		protected IObservableList SourceList {
			get {
				return sourceList;
			}
		}

		#region IyTreeModel implementation

		public TreeModel Adapter {
			get { return adapter;
			}
		}

		#endregion

		#region TreeModelImplementor implementation

		public GLib.GType GetColumnType (int index_)
		{
			return GLib.GType.Object;
		}

		public bool GetIter (out TreeIter iter, TreePath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			iter = TreeIter.Zero;

			object node = NodeAtPath (path); //FIXME Will be optimized
			if (node == null)
				return (false);

			iter = IterFromNode (node);
			return (true);
		}

		public TreePath GetPath (TreeIter iter)
		{
			object node = NodeFromIter (iter);
			if (node == null) 
				throw new ArgumentException (nameof(iter));

			return (PathFromNode (node));
		}

		public void GetValue (TreeIter iter, int column, ref GLib.Value value)
		{
			value = new GLib.Value(NodeFromIter(iter));
		}

		public bool IterNext(ref TreeIter iter)
		{
			if((sourceList == null) || (sourceList.Count == 0) || iter.UserData == IntPtr.Zero)
				return (false);

			object node = NodeFromIter(iter);
			if(node == null)
				return (false);
			object lastNode;
			//Check for "Collection was modified" Exception
			try {
				lastNode = cachedEnumerator != null ? cachedEnumerator.Current : null;
			} catch(InvalidOperationException ex) {
				lastNode = null;
			}
			if(lastNode == node){
				try {
					return GetCacheNext(ref iter);
				}
				catch(InvalidOperationException ex)
				{
					Console.WriteLine("Collection was changed");
				}
			}

			cachedEnumerator = sourceList.GetEnumerator ();
			while (cachedEnumerator.MoveNext ()) {
				if (node == cachedEnumerator.Current)
					return GetCacheNext (ref iter);
			}
			cachedEnumerator = null;
			return false;
		}

		public bool IterChildren (out TreeIter iter, TreeIter parent)
		{
			iter = TreeIter.Zero;
			return false;
		}

		public bool IterHasChild (TreeIter iter)
		{
			return false;
		}

		public int IterNChildren (TreeIter iter)
		{
			if (iter.Equals(TreeIter.Zero as object))
				return SourceList.Count;
			else
				return 0;
		}

		public bool IterNthChild (out TreeIter iter, TreeIter parent, int n)
		{
			iter = TreeIter.Zero;
			if (sourceList == null || sourceList.Count == 0)
				return false;

			if (parent.UserData == IntPtr.Zero) {
				if (sourceList.Count <= n)
					return (false);
				iter = IterFromNode (sourceList [n]);
				return (true);
			}
			return false;
		}

		public bool IterParent (out TreeIter iter, TreeIter child)
		{
			iter = TreeIter.Zero;
			return false;
		}

		public void RefNode (TreeIter iter)
		{
			
		}

		public void UnrefNode (TreeIter iter)
		{
			
		}

		public TreeModelFlags Flags {
			get {
				return TreeModelFlags.ListOnly;
			}
		}

		public int NColumns {
			get { return 1;
			}
		}

		#endregion

		public object NodeAtPath (TreePath aPath)
		{
			if (sourceList == null)
				return (null);
			if (aPath.Indices.Length == 0)
				return (null);
			if (aPath.Indices [0] < 0 || aPath.Indices [0] >= sourceList.Count)
				return null;
			return (sourceList [aPath.Indices [0]]);
		}

		Hashtable node_hash = new Hashtable ();

		public TreeIter IterFromNode (object node)
		{
			GCHandle gch;
			if (node_hash [node] != null) {
				gch = (GCHandle) node_hash [node];
			} else {
				gch = GCHandle.Alloc (node);
				node_hash [node] = gch;
			}
			var result = TreeIter.Zero;
			result.UserData = GCHandle.ToIntPtr(gch);
			return result;
		}

		public object NodeFromIter (TreeIter iter)
		{
			var gch = GCHandle.FromIntPtr(iter.UserData);
			return gch.Target;
		}

		public TreePath PathFromNode (object aNode)
		{
			TreePath tp = new TreePath ();
			if ((aNode == null) || (sourceList == null) || (sourceList.Count == 0))
				return (tp);

			int i = sourceList.IndexOf (aNode);
			if (i > -1)
				tp.AppendIndex (i);
			return (tp);
		}

		private bool GetCacheNext (ref TreeIter iter)
		{
			if (cachedEnumerator.MoveNext ()) {
				iter = IterFromNode (cachedEnumerator.Current);
				return true;
			} else {
				cachedEnumerator = null;
				return false;
			}
		}

		public override void Dispose() {
			sourceList.ElementChanged -= SourceList_ElementChanged;
			sourceList.ElementAdded -= SourceList_ElementAdded;
			sourceList.ElementRemoved -= SourceList_ElementRemoved;
			sourceList.ListChanged -= SourceList_ListChanged;

			foreach(GCHandle item in node_hash.Values) {
				item.Free();
			}
		}
	}
}

