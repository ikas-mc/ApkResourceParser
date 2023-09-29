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
namespace ApkResourceParser
{
    /// <summary>
    /// Represents an XML attribute and value.
    /// </summary>
    public class XmlAttribute
    {
        /// <summary>
        /// The serialized size in bytes of an <seealso cref="XmlAttribute"/>.
        /// </summary>
        public const int SIZE = 12 + ResourceValue.SIZE;

        private int _namespaceIndex;
        private int _nameIndex;
        private int _rawValueIndex;
        private ResourceValue _typedValue;
        private XmlNodeChunk _parent;

        public int namespaceIndex => _namespaceIndex;

        public int nameIndex => _nameIndex;

        public int rawValueIndex => _rawValueIndex;
        public ResourceValue typedValue => _typedValue;
        public XmlNodeChunk parent => _parent;

        private XmlAttribute(int namespaceIndex, int nameIndex, int rawValueIndex, ResourceValue typedValue, XmlNodeChunk parent)
        {
            _namespaceIndex = namespaceIndex;
            _nameIndex = nameIndex;
            _rawValueIndex = rawValueIndex;
            _typedValue = typedValue;
            _parent = parent;
        }

        /// <summary>
        /// Creates a new <seealso cref="XmlAttribute"/> based on the bytes at the current {@code buffer} position.
        /// </summary>
        /// <param name="buffer"> A buffer whose position is at the start of a <seealso cref="XmlAttribute"/>. </param>
        /// <param name="parent"> The parent chunk that contains this attribute; used for string lookups. </param>
        public static XmlAttribute create(ByteBuffer buffer, XmlNodeChunk parent)
        {
            int namespaceIndex = buffer.getInt();
            int nameIndex = buffer.getInt();
            int rawValueIndex = buffer.getInt();
            ResourceValue typedValue = ResourceValue.create(buffer);
            return create(namespaceIndex, nameIndex, rawValueIndex, typedValue, parent);
        }

        public static XmlAttribute create(int namespaceIndex, int nameIndex, int rawValueIndex, ResourceValue typedValue, XmlNodeChunk parent)
        {
            return new XmlAttribute(namespaceIndex, nameIndex, rawValueIndex, typedValue, parent);
        }

        /// <summary>
        /// The namespace URI, or the empty string if not present.
        /// </summary>
        public string xmlNamespace()
        {
            return getString(_namespaceIndex);
        }

        /// <summary>
        /// The attribute name, or the empty string if not present.
        /// </summary>
        public string name()
        {
            return getString(_nameIndex);
        }

        /// <summary>
        /// The raw character value.
        /// </summary>
        public string rawValue()
        {
            return getString(_rawValueIndex);
        }

        private string getString(int index)
        {
            return _parent.getString(index);
        }

        /// <summary>
        /// Returns a brief description of this XML attribute. The representation of this information is
        /// subject to change, but below is a typical example:
        /// 
        /// <pre>"XmlAttribute{namespace=foo, name=bar, value=1234}"</pre>
        /// </summary>
        public override string ToString()
        {
            return $"XmlAttribute{{namespace={xmlNamespace()}, name={name()}, value={rawValue()}}}";
        }
    }
}