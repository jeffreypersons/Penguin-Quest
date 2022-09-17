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
    public sealed class FormattedList
    {
        private readonly string _start;
        private readonly string _end;
        private readonly string _seperator;

        private string _text;
        private StringBuilder _builder;

        public const string DefaultStart     = "[";
        public const string DefaultEnd       = "]";
        public const string DefaultSeperator = ",";
        public static readonly string[] EmptyItems = System.Array.Empty<string>();

        public string Format => _start;
        public string Seperator => _seperator;
        public override string ToString() => _text;


        public FormattedList() :
            this(DefaultStart, DefaultEnd, DefaultSeperator, EmptyItems) { }

        public FormattedList(string start, string end, string sep) :
            this(start, end, sep, EmptyItems) { }

        public FormattedList(string start, string end, string sep, in IEnumerable<string> items)
        {
            string empty = start + end;
            _start     = start;
            _end       = end;
            _seperator = sep;
            _text      = empty;
            _builder   = new StringBuilder(empty);
            foreach (string item in items)
            {
                Append(item);
            }
        }

        public void Append(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                return;
            }

            _builder.Remove(_text.Length - _end.Length, _end.Length);
            _builder.Append(_seperator);
            _builder.Append(item);
            _builder.Append(_end);
            _text = _builder.ToString();
        }
    }
}
