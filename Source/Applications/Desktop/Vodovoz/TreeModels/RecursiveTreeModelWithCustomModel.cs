using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Gtk;
using Vodovoz.TreeModels;

namespace Gamma.Binding
{
	public class RecursiveTreeModelWithCustomModel<TNode> : GLib.Object, TreeModelImplementor, IyTreeModel
	{
		TreeModel adapter;
		IList<TNode> sourceList;
		IEnumerator cachedEnumerator;
		
		private IList<IModelConfig> _modelConfig;

		public event EventHandler RenewAdapter;

		public RecursiveTreeModelWithCustomModel(IList<TNode> list, IList<IModelConfig> config)
		{
			_modelConfig = config;

			adapter = new TreeModelAdapter (this);
			sourceList = list;
		}

		protected IList<TNode> SourceList => sourceList;

		#region IyTreeModel implementation

		public void EmitModelChanged()
		{
			OnRenewAdapter();
		}

		void OnRenewAdapter()
		{
			if (RenewAdapter != null)
				RenewAdapter (this, EventArgs.Empty);
		}

		public TreeModel Adapter => adapter;

		#endregion

		#region TreeModelImplementor implementation

		public GLib.GType GetColumnType(int index)
		{
			return GLib.GType.Object;
		}

		public bool GetIter(out TreeIter iter, TreePath path)
		{
			if(path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			iter = TreeIter.Zero;

			var node = NodeAtPath(path); //FIXME Will be optimized
			if(node == null)
			{
				return false;
			}

			iter = IterFromNode(node);
			return true;
		}

		public TreePath GetPath(TreeIter iter)
		{
			var node = NodeFromIter(iter);
			if(node == null)
			{
				throw new ArgumentException("iter");
			}

			return PathFromNode(node);
		}

		public void GetValue(TreeIter iter, int column, ref GLib.Value value)
		{
			value = new GLib.Value(NodeFromIter(iter));
		}

		public bool IterNext(ref TreeIter iter)
		{
			var node = NodeFromIter(iter);
			if(node == null || sourceList == null || sourceList.Count == 0)
			{
				return false;
			}

			object lastNode;
			//Check for "Collection was modified" Exception
			try
			{ 
				lastNode = cachedEnumerator?.Current;
			}
			catch (InvalidOperationException ex)
			{
				lastNode = null;
			}
			if(lastNode == node)
			{
				try
				{
					return GetCacheNext(ref iter);
				}
				catch(InvalidOperationException)
				{ //for "Collection was modified" Exception
					cachedEnumerator = null;
				}
			}

			var config = GetConfig(node);
			var parent = config.ParentProperty.GetValue(node, null);
			
			cachedEnumerator = parent == null
				? sourceList.GetEnumerator()
				: GetConfig(parent).GetChilds(parent).GetEnumerator();

			while (cachedEnumerator.MoveNext ())
			{
				if (node == cachedEnumerator.Current)
				{
					return GetCacheNext (ref iter);
				}
			}
			cachedEnumerator = null;
			return false;
		}

		public bool IterChildren(out TreeIter iter, TreeIter parent)
		{
			iter = TreeIter.Zero;
			if(parent.UserData == IntPtr.Zero)
			{
				return false;
			}
			var list = GetChildsList(parent);
			if(list == null || list.Count == 0)
			{
				return false;
			}

			iter = IterFromNode(list[0]);
			return true;
		}

		public bool IterHasChild(TreeIter iter)
		{
			var list = GetChildsList(iter);
			return list != null && list.Count > 0;
		}

		public int IterNChildren(TreeIter iter)
		{
			if(iter.Equals(TreeIter.Zero as object))
			{
				return SourceList.Count;
			}

			var list = GetChildsList(iter);
			return list?.Count ?? 0;
		}

		public bool IterNthChild(out TreeIter iter, TreeIter parent, int n)
		{
			iter = TreeIter.Zero;
			if(sourceList == null || sourceList.Count == 0)
			{
				return false;
			}

			var list = parent.UserData == IntPtr.Zero ? (IList)sourceList : GetChildsList(parent);

			if(list == null || list.Count <= n)
			{
				return false;
			}

			iter = IterFromNode(list [n]);
			return true;
		}

		public bool IterParent(out TreeIter iter, TreeIter child)
		{
			iter = TreeIter.Zero;
			if(child.Equals(TreeIter.Zero as object))
			{
				return false;
			}

			var node = NodeFromIter(child);
			var parent = GetConfig(node).ParentProperty.GetValue(node, null);
			if(parent == null)
			{
				return false;
			}

			iter = IterFromNode(parent);
			return true;
		}

		public void RefNode(TreeIter iter)
		{
			
		}

		public void UnrefNode(TreeIter iter)
		{
			
		}

		public TreeModelFlags Flags => TreeModelFlags.ItersPersist;

		public int NColumns => 1;

		#endregion

		public object NodeAtPath(TreePath aPath)
		{
			if(sourceList == null)
			{
				return (null);
			}

			if(aPath.Indices.Length == 0)
			{
				return (null);
			}

			if(aPath.Indices [0] < 0 || aPath.Indices [0] >= sourceList.Count)
			{
				return null;
			}

			var item = sourceList[aPath.Indices[0]];

			if(aPath.Depth == 1)
			{
				return item;
			}

			return GetLevelNode(item, aPath, 1);
		}

		Hashtable node_hash = new Hashtable ();

		public TreeIter IterFromNode(object node)
		{
			GCHandle gch;
			if(node_hash[node] != null)
			{
				gch = (GCHandle) node_hash[node];
			}
			else
			{
				gch = GCHandle.Alloc(node);
				node_hash[node] = gch;
			}
			var result = TreeIter.Zero;
			result.UserData = GCHandle.ToIntPtr(gch);
			return result;
		}

		public object NodeFromIter(TreeIter iter)
		{
			var gch = GCHandle.FromIntPtr(iter.UserData);
			return gch.Target;
		}

		public TreePath PathFromNode(object aNode)
		{
			var tp = new TreePath ();
			if(aNode == null || sourceList == null || sourceList.Count == 0)
			{
				return tp;
			}

			var curNode = aNode;
			var indicesList = new List<int>();
			
			do
			{
				var parent = GetConfig(curNode).ParentProperty.GetValue(curNode, null);
				if(parent == null)
				{
					var i = sourceList.IndexOf((TNode)curNode);
					if(i == -1)
					{
						return tp;
					}

					indicesList.Add(i);
					indicesList.Reverse();
					indicesList.ForEach(tp.AppendIndex);
					return tp;
				}

				var curList = GetConfig(parent).GetChilds(parent);
				var ix = curList.IndexOf(curNode);
				if(ix == -1)
				{
					return tp;
				}

				indicesList.Add(ix);
				curNode = parent;
			} while (true);
		}
		
		#region Privates

		private bool GetCacheNext(ref TreeIter iter)
		{
			if (cachedEnumerator.MoveNext())
			{
				iter = IterFromNode(cachedEnumerator.Current);
				return true;
			}
			
			cachedEnumerator = null;
			return false;
		}

		private IList GetChildsList(TreeIter iter)
		{
			var node = NodeFromIter(iter);
			return GetConfig(node)?.GetChilds(node);
		}

		private object GetLevelNode(object parentNode, TreePath aPath, int level)
		{
			var childs = GetConfig(parentNode).GetChilds(parentNode);

			if(aPath.Indices [level] < 0 || aPath.Indices [level] >= childs.Count)
			{
				return null;
			}

			if(aPath.Depth > level + 1)
			{
				return GetLevelNode(childs[aPath.Indices[level]], aPath, level + 1);
			}
			
			return childs[aPath.Indices[level]];
		}

		private IModelConfig GetConfig(object node) {
			return 	_modelConfig.SingleOrDefault(x => x.Type == node?.GetType());
		}

		#endregion

		public override void Dispose()
		{
			foreach(GCHandle item in node_hash.Values)
			{
				item.Free();
			}
		}
	}
}
