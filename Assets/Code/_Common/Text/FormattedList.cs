using System.Text;
using System.Collections.Generic;


namespace PQ.Common.Text
{
    /*
    Provides a simple mechanism for maintaining a pretty-formatted growing list of items.

    Since string concatenation can cause a lot of gc pressure, this relieves it by providing
    a streamlined way to create append-only formatted lists - which is a very common use case for logging
    in this game.
    */
    public sealed class FormattedList<T>
    {
        private readonly string _start;
        private readonly string _end;
        private readonly string _seperator;

        private string _text;
        private StringBuilder _builder;

        public const string DefaultStart     = "[";
        public const string DefaultEnd       = "]";
        public const string DefaultSeperator = ",";
        public static readonly T[] EmptyItems = System.Array.Empty<T>();

        public string Format => _start;
        public string Seperator => _seperator;
        public override string ToString() => _text;


        public FormattedList() :
            this(DefaultStart, DefaultEnd, DefaultSeperator, EmptyItems) { }

        public FormattedList(string start, string end, string sep) :
            this(start, end, sep, EmptyItems) { }

        public FormattedList(string start, string end, string sep, in IEnumerable<T> items)
        {
            string empty = start + end;
            _start     = start;
            _end       = end;
            _seperator = sep;
            _text      = empty;
            _builder   = new StringBuilder(empty);

            AppendAll(items);
        }
        
        public void Reset()
        {
            _builder.Clear();
            _builder.Append(_start);
            _builder.Append(_end);
        }

        public void Append(T item)
        {
            _builder.Remove(_text.Length - _end.Length, _end.Length);
            _builder.Append(_seperator);
            _builder.Append(item);
            _builder.Append(_end);
            _text = _builder.ToString();
        }

        public void AppendAll(in IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Append(item);
            }
        }
    }
}
