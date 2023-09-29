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
    /// Represents an XML resource map chunk.
    /// 
    /// <para>This chunk maps attribute ids to the resource ids of the attribute resource that defines the
    /// attribute (e.g. type, enum values, etc.).
    /// </para>
    /// </summary>
    public class XmlResourceMapChunk : Chunk
    {
        /// <summary>
        /// The size of a resource reference for {@code resources} in bytes.
        /// </summary>
        private const int RESOURCE_SIZE = 4;

        /// <summary>
        /// Contains a mapping of attributeID to resourceID. For example, the attributeID 2 refers to the
        /// resourceID returned by {@code resources.get(2)}.
        /// </summary>
        private readonly List<int> _resources = new List<int>();

        protected internal XmlResourceMapChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
        }

        protected internal override void init(ByteBuffer buffer)
        {
            base.init(buffer);
            _resources.AddRange(enumerateResources(buffer));
        }

        private IList<int> enumerateResources(ByteBuffer buffer)
        {
            int resourceCount = (getOriginalChunkSize() - getHeaderSize()) / RESOURCE_SIZE;
            var result = new List<int>(resourceCount);
            int offset1 = this.offset + getHeaderSize();
            buffer.mark();
            buffer.position(offset1);

            for (int i = 0; i < resourceCount; ++i)
            {
                result.Add(buffer.getInt());
            }

            buffer.reset();
            return result;
        }

        /// <summary>
        /// Returns the resource ID that {@code attributeId} maps to iff <seealso cref="hasResourceId"/> returns
        /// true for the given {@code attributeId}.
        /// </summary>
        public virtual ResourceIdentifier getResourceId(int attributeId)
        {
            Preconditions.checkArgument(hasResourceId(attributeId), "Attribute ID is not a valid index.");
            return ResourceIdentifier.create(_resources[attributeId]);
        }

        /// <summary>
        /// Returns true if a resource ID exists for the given {@code attributeId}.
        /// </summary>
        public virtual bool hasResourceId(int attributeId)
        {
            return attributeId >= 0 && _resources.Count > attributeId;
        }

        protected internal override Type getType()
        {
            return Chunk.Type.XML_RESOURCE_MAP;
        }
    }
}