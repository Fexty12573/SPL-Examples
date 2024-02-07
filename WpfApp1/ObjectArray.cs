using System.Collections;
using SharpPluginLoader.Core;

namespace WpfApp1;

public unsafe class ObjectArray<T>(nint pointer, int count) : IEnumerable<T> where T : MtObject, new()
{
    private readonly nint* _pointer = (nint*)pointer;
    public int Count => count;
    public T this[int index] => new() { Instance = _pointer[index] };

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
