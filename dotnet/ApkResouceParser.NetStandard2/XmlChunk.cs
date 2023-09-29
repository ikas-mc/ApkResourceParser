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
    /// Represents an XML chunk structure.
    /// 
    /// <para>An XML chunk can contain many nodes as well as a string pool which contains all of the strings
    /// referenced by the nodes.
    /// </para>
    /// </summary>
    public sealed class XmlChunk : ChunkWithChunks
    {
        internal XmlChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
        }

        protected internal override Type getType()
        {
            return Chunk.Type.XML;
        }

        /// <summary>
        /// Returns a string at the provided (0-based) index if the index exists in the string pool.
        /// </summary>
        public string getString(int index)
        {
            foreach (Chunk chunk in getChunks().Values)
            {
                if (chunk is StringPoolChunk stringPoolChunk)
                {
                    return stringPoolChunk.getString(index);
                }
            }

            throw new InvalidOperationException("XmlChunk did not contain a string pool.");
        }
    }
}