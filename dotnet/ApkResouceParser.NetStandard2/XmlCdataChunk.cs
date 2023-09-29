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
    /// Represents an XML cdata node.
    /// </summary>
    public sealed class XmlCdataChunk : XmlNodeChunk
    {
        /// <summary>
        /// A string reference to a string containing the raw character data.
        /// </summary>
        private readonly int _rawValue;

        /// <summary>
        /// A <seealso cref="ResourceValue"/> instance containing the parsed value.
        /// </summary>
        private readonly ResourceValue _resourceValue;

        internal XmlCdataChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            _rawValue = buffer.getInt();
            _resourceValue = ResourceValue.create(buffer);
        }

        /// <summary>
        /// Returns a string containing the raw character data of this chunk.
        /// </summary>
        public string getRawValue()
        {
            return getString(_rawValue);
        }

        /// <summary>
        /// Returns a <seealso cref="ResourceValue"/> instance containing the parsed cdata value.
        /// </summary>
        public ResourceValue getResourceValue()
        {
            return _resourceValue;
        }

        protected internal override Type getType()
        {
            return Chunk.Type.XML_CDATA;
        }

        /// <summary>
        /// Returns a brief description of this XML node. The representation of this information is
        /// subject to change, but below is a typical example:
        /// 
        /// <pre>"XmlCdataChunk{line=1234, comment=My awesome comment., value=1234}"</pre>
        /// </summary>
        public override string ToString()
        {
            return $"XmlCdataChunk\\{{line={getLineNumber()}, comment={getComment()}, value={getRawValue()}\\}}";
        }
    }
}