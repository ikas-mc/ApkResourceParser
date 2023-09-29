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
using System;

namespace ApkResourceParser
{
    /// <summary>
    /// The common superclass for the various types of XML nodes.
    /// </summary>
    public abstract class XmlNodeChunk : Chunk
    {
        /// <summary>
        /// The line number in the original source at which this node appeared.
        /// </summary>
        private readonly int _lineNumber;

        /// <summary>
        /// A string reference of this node's comment. If this is -1, then there is no comment.
        /// </summary>
        private readonly int _comment;

        protected internal XmlNodeChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            _lineNumber = buffer.getInt();
            _comment = buffer.getInt();
        }

        /// <summary>
        /// Returns true if this XML node contains a comment. Else, returns false.
        /// </summary>
        public virtual bool hasComment()
        {
            return _comment != -1;
        }

        /// <summary>
        /// Returns the line number in the original source at which this node appeared.
        /// </summary>
        public virtual int getLineNumber()
        {
            return _lineNumber;
        }

        /// <summary>
        /// Returns the comment associated with this node, if any. Else, returns the empty string.
        /// </summary>
        public virtual string getComment()
        {
            return getString(_comment);
        }

        /// <summary>
        /// An <seealso cref="XmlNodeChunk"/> does not know by itself what strings its indices reference. In order
        /// to get the actual string, the first <seealso cref="XmlChunk"/> ancestor is found. The
        /// <seealso cref="XmlChunk"/> ancestor should have a string pool which {@code index} references.
        /// </summary>
        /// <param name="index"> The index of the string. </param>
        /// <returns> String that the given {@code index} references, or empty string if {@code index} is -1. </returns>
        protected internal virtual string getString(int index)
        {
            if (index == -1)
            {
                // Special case. Packed XML files use -1 for "no string entry"
                return "";
            }

            Chunk parent = getParent();
            while (parent != null)
            {
                if (parent is XmlChunk)
                {
                    return ((XmlChunk)parent).getString(index);
                }

                parent = parent.getParent();
            }

            throw new InvalidOperationException("XmlNodeChunk did not have an XmlChunk parent.");
        }

        /// <summary>
        /// Returns a brief description of this XML node. The representation of this information is
        /// subject to change, but below is a typical example:
        /// 
        /// <pre>"XmlNodeChunk{line=1234, comment=My awesome comment.}"</pre>
        /// </summary>
        public override string ToString()
        {
            return $"XmlNodeChunk{{line={getLineNumber()}, comment={getComment()}}}";
        }
    }
}