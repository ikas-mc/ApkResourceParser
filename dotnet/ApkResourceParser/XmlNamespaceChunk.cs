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
    /// Represents the start/end of a namespace in an XML document.
    /// </summary>
    public abstract class XmlNamespaceChunk : XmlNodeChunk
    {
        /// <summary>
        /// A string reference to the namespace prefix.
        /// </summary>
        private readonly int _prefix;

        /// <summary>
        /// A string reference to the namespace URI.
        /// </summary>
        private readonly int _uri;

        protected internal XmlNamespaceChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            _prefix = buffer.getInt();
            _uri = buffer.getInt();
        }

        /// <summary>
        /// Returns the namespace prefix.
        /// </summary>
        public virtual string getPrefix()
        {
            return getString(_prefix);
        }

        /// <summary>
        /// Returns the namespace URI.
        /// </summary>
        public virtual string getUri()
        {
            return getString(_uri);
        }


        /// <summary>
        /// Returns a brief description of this namespace chunk. The representation of this information is
        /// subject to change, but below is a typical example:
        /// 
        /// <pre>
        /// "XmlNamespaceChunk{line=1234, comment=My awesome comment., prefix=foo, uri=com.google.foo}"
        /// </pre>
        /// </summary>
        public override string ToString()
        {
            return $"XmlNamespaceChunk{{line={getLineNumber()}, comment={getComment()}, prefix={getPrefix()}, uri={getUri()}}}";
        }
    }
}