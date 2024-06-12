using System;
using System.Collections.Generic;

namespace SotnKhaosTools.Utils
{
	internal sealed class PriorityQueue<TPriority, TItem> where TPriority : IComparable<TPriority>
	{
		private readonly IComparer<TPriority>? comparer;
		private (TItem Item, TPriority Priority)[] items;
		private int capacity;
		private int arity;

		public PriorityQueue(int capacity, int arity = 2, Comparer<TPriority>? comparer = null)
		{
			this.items = new (TItem Item, TPriority Priority)[capacity];
			this.capacity = capacity;
			this.Count = 0;
			this.arity = arity;
			if (comparer is null)
			{
				this.comparer = Comparer<TPriority>.Default;
			}
		}

		public int Count { get; private set; }

		public void Add(TItem item, TPriority priority)
		{
			Count++;
			items[Count - 1] = (item, priority);
			if (this.Count > 1)
			{
				HeapifyUp();
			}
		}

		public void Clear()
		{
			Count = 0;
		}

		public TItem Peek()
		{
			return items[0].Item;
		}

		public TItem PopMin()
		{
			if (this.Count == 0)
			{
				return items[0].Item;
			}
			TItem min = items[0].Item;
			items[0] = items[Count - 1];
			Count--;
			if (Count > 1)
			{
				HeapifyDown();
			}
			return min;
		}

		private int GetParent(int index)
		{
			return (index - 1) / arity;
		}

		private int GetNthChild(int index, int n)
		{
			return (index * arity) + 1 + n;
		}

		private int GetMinChild(int index)
		{
			int minChild = GetNthChild(index, 0);

			for (int i = 1; i < arity; i++)
			{
				int currentChild = GetNthChild(index, i);
				if (currentChild >= Count)
				{
					break;
				}
				if (comparer.Compare(items[currentChild].Priority, items[minChild].Priority) < 0)
				{
					minChild = currentChild;
				}
			}
			return minChild;
		}

		private void Swap(int a, int b)
		{
			(TItem, TPriority) tmp = items[a];
			items[a] = items[b];
			items[b] = tmp;
		}

		private void AssureCapacity()
		{
			if (this.Count < this.items.Length)
			{
				return;
			}
			capacity *= 2;
			(TItem Item, TPriority Priority)[] newItems = new (TItem Item, TPriority Priority)[capacity];

			items.CopyTo(newItems, 0);
			items = newItems;
		}

		private void HeapifyUp(int index = -1)
		{
			if (index == -1)
			{
				index = Count - 1;
			}

			while (index >= 0)
			{
				int parent = GetParent(index);
				if (comparer.Compare(items[index].Priority, items[parent].Priority) < 0)
				{
					Swap(index, parent);
					index = parent;
				}
				else
				{
					break;
				}
			}
		}

		private void HeapifyDown(int index = -1)
		{
			if (index == -1)
			{
				index = 0;
			}

			while (index < Count)
			{
				int minChild = GetMinChild(index);
				if (comparer.Compare(items[minChild].Priority, items[index].Priority) < 0)
				{
					Swap(index, minChild);
					index = minChild;
				}
				else
				{
					break;
				}
			}
		}
	}
}
