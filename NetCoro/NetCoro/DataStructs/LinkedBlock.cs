using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NetCoro.DataStructs {
	//Efficient storage for coroutines 
	public class LinkedBlock<T> {
		public readonly T Value;
		public LinkedBlock<T> Next { get; private set; }
		public LinkedBlock<T> Prev { get; private set; }

		public bool IsEmpty => Next == null;

		public LinkedBlock(T coroEnumerator) {
			Value = coroEnumerator;
		}

		public void PutAfter(LinkedBlock<T> prevContainer) {
			Next = prevContainer.Next;
			Prev = prevContainer;
			prevContainer.Next = this;
			if (Next != null)
				Next.Prev = this;
		}

		internal void DeleteFromChain() {
			if (Prev != null) {
				Prev.Next = Next;
			}
			if (Next != null) {
				Next.Prev = Prev;
			}
		}

		public static implicit operator T(LinkedBlock<T> block) => block.Value;
	}

	public class LinkedContainer<T> : IEnumerable<LinkedBlock<T>> {
		public LinkedBlock<T> First { get; private set; }
		public LinkedBlock<T> End { get; private set; }

		public int Count { get; private set; }
		public bool IsEmpty => First == null;

		public LinkedContainer() {
			First = null;
			End = null;
			Count = 0;
		}

		public void Add(T value) => Add(new LinkedBlock<T>(value));

		public void Add(LinkedBlock<T> container) {
			if (First == null) {
				First = container;
				End = container;
			} else {
				container.PutAfter(End);
				End = End.Next;
			}
			Count++;
		}

		public void Remove(LinkedBlock<T> container) {
			if (container == First)
				First = container.Next;

			if (container == End)
				End = container.Prev;

			container.DeleteFromChain();
			Count--;
		}

		public void Clear() {
			First = null;
			End = null;
			Count = 0;
		}

		public IEnumerator<LinkedBlock<T>> GetEnumerator() {
			for (var currentBlock = First; currentBlock != null; currentBlock = currentBlock.Next)
				yield return currentBlock;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	}
}
