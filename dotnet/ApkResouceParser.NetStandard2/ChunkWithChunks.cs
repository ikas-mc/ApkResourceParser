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
using System.Collections.Generic;
using System.Linq;

namespace ApkResourceParser
{
    /// <summary>
    /// Represents a chunk whose payload is a list of sub-chunks.
    /// </summary>
    public abstract class ChunkWithChunks : Chunk
    {
        protected internal readonly Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>();

        protected internal ChunkWithChunks(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
        }

        protected internal override void init(ByteBuffer buffer)
        {
            base.init(buffer);
            chunks.Clear();
            int start = this.offset + getHeaderSize();
            int offset1 = start;
            int end = this.offset + getOriginalChunkSize();
            int position = buffer.position();
            buffer.position(start);

            while (offset1 < end)
            {
                Chunk chunk = createChildInstance(buffer);
                chunks[offset1] = chunk;
                offset1 += chunk.getOriginalChunkSize();
            }

            buffer.position(position);
        }

        /// <summary>
        /// Allows subclasses to decide how child instances should be instantiated, e.g., compressed chunks
        /// might use a different method to extract compressed data first.
        /// </summary>
        /// <param name="buffer"> The buffer to read from </param>
        /// <returns> The child instance. </returns>
        protected internal virtual Chunk createChildInstance(ByteBuffer buffer)
        {
            return Chunk.newInstance(buffer, this);
        }

        /// <summary>
        /// Retrieves the @{code chunks} contained in this chunk.
        /// </summary>
        /// <returns> map of buffer offset -> chunk contained in this chunk. </returns>
        public IDictionary<int, Chunk> getChunks()
        {
            return chunks;
        }

        protected internal virtual void add(Chunk chunk)
        {
            int offset1 = 0;
            if (chunks.Count > 0)
            {
                int oldMax = chunks.Keys.Max();
                if (!chunks.TryGetValue(oldMax, out var oldChunk))
                {
                    throw new InvalidOperationException("oldChunk is null");
                }

                offset1 = oldMax + oldChunk.getOriginalChunkSize();
            }

            chunks[offset1] = chunk;

            if (chunk.getParent() != this)
            {
                throw new InvalidOperationException();
            }
        }
    }
}