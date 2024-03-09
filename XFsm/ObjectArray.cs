using System.Collections;
using System.Runtime.CompilerServices;
using SharpPluginLoader.Core;

namespace XFsm;

public unsafe class ObjectArray<T>(nint pointer, int count, Func<nint, T>? createFunc = null) 
    : IEnumerable<T> where T : MtObject, new()
{
    public int Count { get; } = count;
    public nint Address => (nint)Pointer;
    public nint* Pointer { get; } = (nint*)pointer;

    public T? this[int index]
    {
        get
        {
            var instance = Pointer[index];
            return instance == 0 ? null : createFunc?.Invoke(instance) ?? new T { Instance = instance };
        }
        set => Pointer[index] = value?.Instance ?? 0;
    }

    public void Reverse(int index, int count_)
    {
        var end = index + count_ - 1;
        while (index < end)
        {
            var temp = Pointer[index];
            Pointer[index++] = Pointer[end];
            Pointer[end--] = temp;
        }
    }

    public void Swap(int index1, int index2)
    {
        (Pointer[index1], Pointer[index2]) = (Pointer[index2], Pointer[index1]);
    }

    public IEnumerator<T> GetEnumerator() =>new Enumerator(Pointer, Count, createFunc);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator(nint* pointer, int count, Func<nint, T>? func) : IEnumerator<T>
    {
        private int _index = -1;

        public T Current
        {
            get
            {
                var instance = pointer[_index];
                return instance == 0 ? null! : func?.Invoke(instance) ?? new T { Instance = instance };
            }
        }

        object? IEnumerator.Current => Current;

        public readonly void Dispose()
        {
        }

        public bool MoveNext()
        {
            return ++_index < count;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}

public static class Extensions
{
    public static int IndexOf<T>(this PointerArray<T> array, ref T value) where T : unmanaged
    {
        for (var i = 0; i < array.Length; i++)
        {
            if (Unsafe.AreSame(ref array[i], ref value))
                return i;
        }

        return -1;
    }
}
