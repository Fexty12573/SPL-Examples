using System.Collections;
using System.Runtime.CompilerServices;
using SharpPluginLoader.Core;

namespace XFsm;

public unsafe class ObjectArray<T>(nint pointer, int count) : IEnumerable<T> where T : MtObject, new()
{
    public int Count { get; } = count;
    public nint Address => (nint)Pointer;
    public nint* Pointer { get; } = (nint*)pointer;

    public T this[int index]
    {
        get => new() { Instance = Pointer[index] };
        set => Pointer[index] = value.Instance;
    }

    public void Reverse(int index, int count)
    {
        var end = index + count - 1;
        while (index < end)
        {
            var temp = Pointer[index];
            Pointer[index++] = Pointer[end];
            Pointer[end--] = temp;
        }
    }

    public IEnumerator<T> GetEnumerator() =>new Enumerator(Pointer, Count);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator(nint* pointer, int count) : IEnumerator<T>
    {
        private int _index = -1;

        public T Current => new() { Instance = pointer[_index] };

        object IEnumerator.Current => Current;

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
