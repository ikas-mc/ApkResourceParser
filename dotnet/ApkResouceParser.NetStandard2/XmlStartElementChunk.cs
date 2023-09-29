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
    /// Represents the beginning of an XML node.
    /// </summary>
    public sealed class XmlStartElementChunk : XmlNodeChunk
    {
        /// <summary>
        /// A string reference to the namespace URI, or -1 if not present.
        /// </summary>
        private readonly int _namespace;

        /// <summary>
        /// A string reference to the element name that this chunk represents.
        /// </summary>
        private readonly int _name;

        /// <summary>
        /// The offset to the start of the attributes payload.
        /// </summary>
        private readonly int _attributeStart;

        /// <summary>
        /// The number of attributes in the original buffer.
        /// </summary>
        private readonly int _attributeCount;

        /// <summary>
        /// The (0-based) index of the id attribute, or -1 if not present.
        /// </summary>
        private readonly int _idIndex;

        /// <summary>
        /// The (0-based) index of the class attribute, or -1 if not present.
        /// </summary>
        private readonly int _classIndex;

        /// <summary>
        /// The (0-based) index of the style attribute, or -1 if not present.
        /// </summary>
        private readonly int _styleIndex;

        /// <summary>
        /// The XML attributes associated with this element.
        /// </summary>
        private readonly List<XmlAttribute> _attributes = new List<XmlAttribute>();

        internal XmlStartElementChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            _namespace = buffer.getInt();
            _name = buffer.getInt();
            _attributeStart = (buffer.getShort() & 0xFFFF);
            int attributeSize = (buffer.getShort() & 0xFFFF);
            Preconditions.checkState(attributeSize == XmlAttribute.SIZE,
                $"attributeSize is wrong size. Got {attributeSize}, want {XmlAttribute.SIZE}");
            _attributeCount = (buffer.getShort() & 0xFFFF);

            // The following indices are 1-based and need to be adjusted.
            _idIndex = (buffer.getShort() & 0xFFFF) - 1;
            _classIndex = (buffer.getShort() & 0xFFFF) - 1;
            _styleIndex = (buffer.getShort() & 0xFFFF) - 1;
        }

        protected internal override void init(ByteBuffer buffer)
        {
            base.init(buffer);
            _attributes.AddRange(enumerateAttributes(buffer));
        }

        private IList<XmlAttribute> enumerateAttributes(ByteBuffer buffer)
        {
            var result = new List<XmlAttribute>(_attributeCount);
            int offset1 = this.offset + getHeaderSize() + _attributeStart;
            int endOffset = offset1 + XmlAttribute.SIZE * _attributeCount;
            buffer.mark();
            buffer.position(offset1);

            while (offset1 < endOffset)
            {
                result.Add(XmlAttribute.create(buffer, this));
                offset1 += XmlAttribute.SIZE;
            }

            buffer.reset();
            return result;
        }

        /// <summary>
        /// Remaps all the attribute references using the supplied remapping. If an attribute has a
        /// reference to a resourceid that is in the remapping keys, it will be updated with the
        /// corresponding value from the remapping. All attributes that do not have reference to
        /// a value in the remapping are left as is.
        /// </summary>
        /// <param name="remapping"> The original and new resource ids. </param>
        public void remapReferences(IDictionary<int, int> remapping)
        {
            var newEntries = new Dictionary<int, XmlAttribute>();
            int count = 0;
            foreach (XmlAttribute attribute in _attributes)
            {
                ResourceValue value = attribute.typedValue;
                if (value.type() == ResourceValue.Type.REFERENCE)
                {
                    int valueData = value.data();
                    if (ResourceIdentifier.create(valueData).packageId != 0x1)
                    {
                        if (remapping.ContainsKey(valueData))
                        {
                            remapping.TryGetValue(valueData, out var data);
                            Preconditions.checkNotNull(data);
                            var newAttribute = XmlAttribute.create(attribute.namespaceIndex, attribute.nameIndex, attribute.rawValueIndex, attribute.typedValue.withData(data), attribute.parent);
                            newEntries[count] = newAttribute;
                        }
                    }
                }

                count++;
            }

            foreach (KeyValuePair<int, XmlAttribute> entry in newEntries)
            {
                _attributes[entry.Key] = entry.Value;
            }
        }

        /// <summary>
        /// Returns the namespace URI, or the empty string if not present.
        /// </summary>
        public string getNamespace()
        {
            return getString(_namespace);
        }

        /// <summary>
        /// Returns the element name that this chunk represents.
        /// </summary>
        public string getName()
        {
            return getString(_name);
        }

        /// <summary>
        /// Returns an unmodifiable list of this XML element's attributes.
        /// </summary>
        public IList<XmlAttribute> getAttributes()
        {
            return _attributes; //.ToImmutableList();
        }

        protected internal override Type getType()
        {
            return Chunk.Type.XML_START_ELEMENT;
        }

        /// <summary>
        /// Returns a brief description of this XML node. The representation of this information is
        /// subject to change, but below is a typical example:
        /// 
        /// <pre>
        /// "XmlStartElementChunk{line=1234, comment=My awesome comment., namespace=foo, name=bar, ...}"
        /// </pre>
        /// </summary>
        public override string ToString()
        {
            return $"XmlStartElementChunk\\{{line={getLineNumber()}, comment={getComment()},namespace={getNamespace()}, name={getName()}, attributes={_attributes.ToString()}\\}}";
        }
    }
}