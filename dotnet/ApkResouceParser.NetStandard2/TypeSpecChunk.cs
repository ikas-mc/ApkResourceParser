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
    /// A chunk that contains a collection of resource entries for a particular resource data type.
    /// </summary>
    public class TypeSpecChunk : Chunk
    {
        /// <summary>
        /// Flag indicating that a resource entry is public.
        /// </summary>
        private const int SPEC_PUBLIC = 0x40000000;

        /// <summary>
        /// The id of the resource type that this type spec refers to.
        /// </summary>
        private int _id;

        /// <summary>
        /// Flags for entries at a given index.
        /// </summary>
        private int[] _resources;

        protected internal TypeSpecChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            _id = /*Byte.toUnsignedInt*/buffer.get();
            buffer.position(buffer.position() + 3); // Skip 3 bytes for packing
            int resourceCount = buffer.getInt();
            _resources = new int[resourceCount];
            for (int i = 0; i < resourceCount; ++i)
            {
                _resources[i] = buffer.getInt();
            }
        }

        /// <summary>
        /// Returns the (1-based) type id of the resources that this <seealso cref="TypeSpecChunk"/> has
        /// configuration masks for.
        /// </summary>
        public virtual int getId()
        {
            return _id;
        }

        /// <summary>
        /// Sets the id of this chunk.
        /// </summary>
        /// <param name="newId"> The new id to use. </param>
        public virtual void setId(int newId)
        {
            // Ids are 1-based.
            Preconditions.checkState(newId >= 1);
            _id = newId;
        }

        /// <summary>
        /// Returns the number of resource entries that this chunk has configuration masks for.
        /// </summary>
        public virtual int getResourceCount()
        {
            return getResources().Length;
        }

        protected internal override Type getType()
        {
            return Chunk.Type.TABLE_TYPE_SPEC;
        }


        /// <summary>
        /// Resource configuration masks.
        /// </summary>
        public virtual int[] getResources()
        {
            return _resources;
        }
    }
}