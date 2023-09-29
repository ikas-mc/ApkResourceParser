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
    /// Represents a resource table structure. Its sub-chunks contain:
    /// 
    /// <ul>
    ///   <li>A <seealso cref="StringPoolChunk"/> containing all string values in the entire resource table. It
    ///       does not, however, contain the names of entries or type identifiers.</li>
    ///   <li>One or more <seealso cref="PackageChunk"/>.</li>
    /// </ul>
    /// </summary>
    public class ResourceTableChunk : ChunkWithChunks
    {
        protected internal const int HEADER_SIZE = Chunk.METADATA_SIZE + 4; // +4 = package count

        /// <summary>
        /// The packages contained in this resource table.
        /// </summary>
        private readonly IDictionary<string, PackageChunk> _packages = new Dictionary<string, PackageChunk>();

        /// <summary>
        /// A string pool containing all string resource values in the entire resource table.
        /// </summary>
        private StringPoolChunk _stringPool;

        protected internal ResourceTableChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            // packageCount. We ignore this, because we already know how many chunks we have.
            Preconditions.checkState(buffer.getInt() >= 1, "ResourceTableChunk package count was < 1.");
        }

        private static bool isString(ResourceValue value)
        {
            return value.type() == ResourceValue.Type.STRING;
        }

        protected internal override void init(ByteBuffer buffer)
        {
            base.init(buffer);
            setChildChunks();
        }

        protected internal virtual void setChildChunks()
        {
            _packages.Clear();
            foreach (Chunk chunk in getChunks().Values)
            {
                if (chunk is PackageChunk packageChunk)
                {
                    _packages[packageChunk.getPackageName()] = packageChunk;
                }
                else if (chunk is StringPoolChunk stringPoolChunk)
                {
                    _stringPool = stringPoolChunk;
                }
            }

            Preconditions.checkNotNull(_stringPool, "ResourceTableChunk must have a string pool.");
        }

        /// <summary>
        /// Returns the string pool containing all string resource values in the resource table.
        /// </summary>
        public virtual StringPoolChunk getStringPool()
        {
            return _stringPool;
        }

        /// <summary>
        /// Adds the <seealso cref="PackageChunk"/> to this table.
        /// </summary>
        public virtual void addPackageChunk(PackageChunk packageChunk)
        {
            base.add(packageChunk);
            this._packages[packageChunk.getPackageName()] = packageChunk;
        }

        /// <summary>
        /// Returns the package with the given {@code packageName}. Else, returns null.
        /// </summary>
        /*@Nullable*/
        public virtual PackageChunk getPackage(string packageName)
        {
            _packages.TryGetValue(packageName, out var result);
            return result;
        }

        /// <summary>
        /// Returns the package with the given {@code packageId}. Else, returns null
        /// </summary>
        /*@Nullable*/
        public virtual PackageChunk getPackage(int packageId)
        {
            foreach (PackageChunk chunk in _packages.Values)
            {
                if (chunk.getId() == packageId)
                {
                    return chunk;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the packages contained in this resource table.
        /// </summary>
        public virtual ICollection<PackageChunk> getPackages()
        {
            return _packages.Values;
        }

        protected internal override Type getType()
        {
            return Chunk.Type.TABLE;
        }
    }
}