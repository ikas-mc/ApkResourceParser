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
    /// Represents the end of an XML node.
    /// </summary>
    public sealed class XmlEndElementChunk : XmlNodeChunk
    {
        /// <summary>
        /// A string reference to the namespace URI, or -1 if not present.
        /// </summary>
        private readonly int _namespace;

        /// <summary>
        /// A string reference to the attribute name.
        /// </summary>
        private readonly int _name;

        internal XmlEndElementChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            _namespace = buffer.getInt();
            _name = buffer.getInt();
        }

        /// <summary>
        /// Returns the namespace URI, or the empty string if no namespace is present.
        /// </summary>
        public string getNamespace()
        {
            return getString(_namespace);
        }

        /// <summary>
        /// Returns the attribute name.
        /// </summary>
        public string getName()
        {
            return getString(_name);
        }

        protected internal override Type getType()
        {
            return Chunk.Type.XML_END_ELEMENT;
        }


        /// <summary>
        /// Returns a brief description of this XML node. The representation of this information is
        /// subject to change, but below is a typical example:
        /// 
        /// <pre>
        /// "XmlEndElementChunk{line=1234, comment=My awesome comment., namespace=foo, name=bar}"
        /// </pre>
        /// </summary>
        public override string ToString()
        {
            return $"XmlEndElementChunk{{line={getLineNumber()}, comment={getComment()}, namespace={getNamespace()}, name={getName()}}}";
        }
    }
}