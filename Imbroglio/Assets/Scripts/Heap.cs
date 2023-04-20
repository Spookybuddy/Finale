using System;
using System.Collections;
using UnityEngine;

public class Heap<T> where T : IHeapItem<T> {
    T[] items;
    int count;

    //Set the heap
    public Heap(int maxSize) {
        items = new T[maxSize];
    }

    //add to the heap
    public void Add(T item) {
        item.HeapIndex = count;
        items[count] = item;
        SortUp(item);
        count++;
    }

    //Removes the first item
    public T RemoveFirst() {
        T first = items[0];
        count--;
        items[0] = items[count];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return first;
    }

    public void UpdateItem(T item) {
        SortUp(item);
    }

    public int Count {
        get { return count; }
    }

    public bool Contains(T item) {
        return Equals(items[item.HeapIndex], item);
    }

    //Swap children when their value < current
    private void SortDown(T item) {
        while (true) {
            int childL = item.HeapIndex * 2 + 1;
            int childR = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childL < count) {
                swapIndex = childL;
                if (childR < count) {
                    if (items[childL].CompareTo(items[childR]) < 0) {
                        swapIndex = childR;
                    }
                }
                if (item.CompareTo(items[swapIndex]) < 0) {
                    Swap(item, items[swapIndex]);
                } else return;
            } else return;
        }
    }

    //Swap with parent
    private void SortUp(T item) {
        int parentIndex = (item.HeapIndex - 1) / 2;
        while (true) {
            T parent = items[parentIndex];
            if (item.CompareTo(parent) > 0) {
                Swap(item, parent);
            } else break;
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    //Swap items A & B
    private void Swap(T a, T b) {
        items[a.HeapIndex] = b;
        items[b.HeapIndex] = a;
        int temp = a.HeapIndex;
        a.HeapIndex = b.HeapIndex;
        b.HeapIndex = temp;
    }
}

public interface IHeapItem<T> : IComparable<T> {
    int HeapIndex {
        get;
        set;
    }
}