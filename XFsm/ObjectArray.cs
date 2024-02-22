using System.Collections;
using System.Runtime.CompilerServices;
using SharpPluginLoader.Core;

namespace XFsm;

public unsafe class ObjectArray<T>(nint pointer, int count) : IEnumerable<T> where T : MtObject, new()
{
    private readonly nint* _pointer = (nint*)pointer;
    public int Count => count;
    public nint Pointer => (nint)_pointer;
    public T this[int index]
    {
        get => new() { Instance = _pointer[index] };
        set => _pointer[index] = value.Instance;
    }

    public IEnumerator<T> GetEnumerator() =>new Enumerator(_pointer, count);
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
