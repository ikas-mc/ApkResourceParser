/*
 * Copyright 2016 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/*
 *	apk resource parser for .net
 *  port of https://github.com/google/android-arscblamer
 *
 *  ikas-mc 2023
 */
using System.Collections.Generic;

namespace ApkResourceParser
{
    /// <summary>
    /// Represents a string pool structure.
    /// </summary>
    public class StringPoolChunk : Chunk
    {
        // These are the defined flags for the "flags" field of ResourceStringPoolHeader
        private const int _SORTED_FLAG = 1 << 0;
        private const int _UTF8_FLAG = 1 << 8;

        /// <summary>
        /// The offset from the start of the header that the stylesStart field is at.
        /// </summary>
        private const int STYLE_START_OFFSET = 24;

        /// <summary>
        /// Number of strings in the original buffer. This is not necessarily the number of strings in
        /// {@code strings}.
        /// </summary>
        protected internal readonly int stringCount;

        /// <summary>
        /// Number of styles in the original buffer. This is not necessarily the number of styles in {@code
        /// styles}.
        /// </summary>
        protected internal readonly int styleCount;

        /// <summary>
        /// Flags.
        /// </summary>
        private readonly int _flags;

        /// <summary>
        /// Index from header of the string data.
        /// </summary>
        private readonly int _stringsStart;

        /// <summary>
        /// Index from header of the style data.
        /// </summary>
        private readonly int _stylesStart;

        /// <summary>
        /// The strings ordered as they appear in the arsc file. e.g. strings.get(1234) gets the 1235th
        /// string in the arsc file.
        /// </summary>
        protected internal readonly List<string> strings = new List<string>();

        /// <summary>
        /// These styles have a 1:1 relationship with the strings. For example, styles.get(3) refers to the
        /// string at location strings.get(3). There are never more styles than strings (though there may
        /// be less). Inside of that are all of the styles referenced by that string.
        /// </summary>
        protected internal readonly List<StringPoolStyle> styles = new List<StringPoolStyle>();

        protected internal bool alwaysDedup = false;

        protected internal StringPoolChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            stringCount = buffer.getInt();
            styleCount = buffer.getInt();
            _flags = buffer.getInt();
            _stringsStart = buffer.getInt();
            _stylesStart = buffer.getInt();
        }

        /// <summary>
        /// Returns a list of {@code styles} with spans containing remapped string indexes by {@code
        /// remappedIndexes}.
        /// </summary>
        protected internal static IList<StringPoolStyle> fixUpStyles(IList<StringPoolStyle> styles, int[] remappedIndexes)
        {
            var result = new List<StringPoolStyle>(styles.Count);
            foreach (var style in styles)
            {
                var newSpans = new List<StringPoolSpan>(style.spans.Count);
                foreach (StringPoolSpan span in style.spans)
                {
                    int newIndex = remappedIndexes[span.nameIndex];
                    Preconditions.checkArgument(newIndex >= 0, "error index");
                    newSpans.Add(span.withNameIndex(newIndex));
                }

                result.Add(StringPoolStyle.create(newSpans));
            }

            return result;
        }

        protected internal override void init(ByteBuffer buffer)
        {
            base.init(buffer);
            strings.AddRange(readStrings(buffer, offset + _stringsStart, stringCount));
            styles.AddRange(readStyles(buffer, offset + _stylesStart, styleCount));
        }

        /// <summary>
        /// Returns the 0-based index of the first occurrence of the given string, or -1 if the string is
        /// not in the pool. This runs in O(n) time.
        /// </summary>
        /// <param name="str"> The string to check the pool for. </param>
        /// <returns> Index of the string, or -1 if not found. </returns>
        public virtual int indexOf(string str)
        {
            return strings.IndexOf(str);
        }

        /// <summary>
        /// Returns a string at the given (0-based) index.
        /// </summary>
        /// <param name="index"> The (0-based) index of the string to return. </param>
        /// <exception cref="IndexOutOfBoundsException"> If the index is out of range (index < 0 || index >= size()). </exception>
        public virtual string getString(int index)
        {
            return strings[index];
        }

        /// <summary>
        /// Returns the number of strings in this pool.
        /// </summary>
        public virtual int getStringCount()
        {
            return strings.Count;
        }

        /// <summary>
        /// Returns a style at the given (0-based) index.
        /// </summary>
        /// <param name="index"> The (0-based) index of the style to return. </param>
        /// <exception cref="IndexOutOfBoundsException"> If the index is out of range (index < 0 || index >= size()). </exception>
        public virtual StringPoolStyle getStyle(int index)
        {
            return styles[index];
        }

        /// <summary>
        /// Returns the number of styles in this pool.
        /// </summary>
        public virtual int getStyleCount()
        {
            return styles.Count;
        }

        /// <summary>
        /// Returns the type of strings in this pool.
        /// </summary>
        public virtual ResourceString.Type getStringType()
        {
            return isUTF8() ? ResourceString.Type.UTF8 : ResourceString.Type.UTF16;
        }

        protected internal override Type getType()
        {
            return Chunk.Type.STRING_POOL;
        }

        /// <summary>
        /// Returns the number of bytes needed for offsets based on {@code strings} and {@code styles}.
        /// </summary>
        private int getOffsetSize()
        {
            return (strings.Count + styles.Count) * 4;
        }

        /// <summary>
        /// True if this string pool contains strings in UTF-8 format. Otherwise, strings are in UTF-16.
        /// </summary>
        /// <returns> true if @{code strings} are in UTF-8; false if they're in UTF-16. </returns>
        public virtual bool isUTF8()
        {
            return (_flags & _UTF8_FLAG) != 0;
        }

        /// <summary>
        /// True if this string pool contains already-sorted strings.
        /// </summary>
        /// <returns> true if @{code strings} are sorted. </returns>
        public virtual bool isSorted()
        {
            return (_flags & _SORTED_FLAG) != 0;
        }

        private IList<string> readStrings(ByteBuffer buffer, int offset, int count)
        {
            var result = new List<string>();
            int previousOffset = -1;
            // After the header, we now have an array of offsets for the strings in this pool.
            for (int i = 0; i < count; ++i)
            {
                int stringOffset = offset + buffer.getInt();
                result.Add(ResourceString.decodeString(buffer, stringOffset, getStringType()));
                if (stringOffset <= previousOffset)
                {
                    alwaysDedup = true;
                }

                previousOffset = stringOffset;
            }

            return result;
        }

        private IList<StringPoolStyle> readStyles(ByteBuffer buffer, int offset, int count)
        {
            var result = new List<StringPoolStyle>();
            // After the array of offsets for the strings in the pool, we have an offset for the styles
            // in this pool.
            for (int i = 0; i < count; ++i)
            {
                int styleOffset = offset + buffer.getInt();
                result.Add(StringPoolStyle.create(buffer, styleOffset, this));
            }

            return result;
        }

        /// <summary>
        /// Sets the flag if we should dedup strings even when shrink is set to true
        /// </summary>
        public virtual void setAlwaysDedup(bool alwaysDedup)
        {
            this.alwaysDedup = alwaysDedup;
        }

        /// <summary>
        /// Represents all of the styles for a particular string. The string is determined by its index
        /// in <seealso cref="StringPoolChunk"/>.
        /// </summary>
        public class StringPoolStyle
        {
            // Styles are a list of integers with 0xFFFFFFFF serving as a sentinel value.
            public const uint RES_STRING_POOL_SPAN_END = 0xFFFFFFFF;

            private IList<StringPoolSpan> _spans;
            private StringPoolStyle(IList<StringPoolSpan> spans)
            {
                _spans = spans;
            }

            public IList<StringPoolSpan> spans => _spans;

            public static StringPoolStyle create(ByteBuffer buffer, int offset, StringPoolChunk parent)
            {
                var spans = new List<StringPoolSpan>();
                var nameIndex = (uint)buffer.getInt(offset);
                while (nameIndex != RES_STRING_POOL_SPAN_END)
                {
                    spans.Add(StringPoolSpan.create(buffer, offset, parent));
                    offset += StringPoolSpan.SPAN_LENGTH;
                    nameIndex = (uint)buffer.getInt(offset);
                }

                return create(spans);
            }

            public static StringPoolStyle create(ByteBuffer buffer, StringPoolChunk parent)
            {
                var spans = new List<StringPoolSpan>();
                int nameIndex = buffer.getInt();
                while ((uint)nameIndex != RES_STRING_POOL_SPAN_END)
                {
                    spans.Add(StringPoolSpan.create(buffer, parent, nameIndex));
                    nameIndex = buffer.getInt();
                }

                return create((spans));
            }

            public static StringPoolStyle create(IList<StringPoolSpan> spans)
            {
                return new StringPoolStyle(spans);
            }

            /// <summary>
            /// Returns a brief description of the contents of this style. The representation of this
            /// information is subject to change, but below is a typical example:
            /// 
            /// <pre>"StringPoolStyle{spans=[StringPoolSpan{foo, start=0, stop=5}, ...]}"</pre>
            /// </summary>
            public override string ToString()
            {
                return $"StringPoolStyle{{spans={spans}}}";
            }
        }

        /// <summary>
        /// Represents a styled span associated with a specific string.
        /// </summary>
        public class StringPoolSpan
        {
            public const int SPAN_LENGTH = 12;

            private int _nameIndex;
            private int _start;
            private int _stop;
            private StringPoolChunk _parent;

            private StringPoolSpan(int nameIndex, int start, int stop, StringPoolChunk parent)
            {
                _nameIndex = nameIndex;
                _start = start;
                _stop = stop;
                _parent = parent;
            }

            public int nameIndex => _nameIndex;

            public int start => _start;

            public int stop => _stop;

            public StringPoolChunk parent => _parent;

            public static StringPoolSpan create(ByteBuffer buffer, StringPoolChunk parent, int nameIndex)
            {
                int start = buffer.getInt();
                int stop = buffer.getInt();
                return new StringPoolSpan(nameIndex, start, stop, parent);
            }

            public static StringPoolSpan create(ByteBuffer buffer, int offset, StringPoolChunk parent)
            {
                int nameIndex = buffer.getInt(offset);
                int start = buffer.getInt(offset + 4);
                int stop = buffer.getInt(offset + 8);
                return new StringPoolSpan(nameIndex, start, stop, parent);
            }


            public StringPoolSpan withNameIndex(int nameIndex)
            {
                return new StringPoolSpan(nameIndex, start, stop, parent);
            }

            /// <summary>
            /// Returns a brief description of this span. The representation of this information is subject
            /// to change, but below is a typical example:
            /// 
            /// <pre>"StringPoolSpan{foo, start=0, stop=5}"</pre>
            /// </summary>
            public override string ToString()
            {
                return $"StringPoolSpan{{{parent.getString(nameIndex)}, start={start}, stop={stop}}}";
            }
        }
    }
}