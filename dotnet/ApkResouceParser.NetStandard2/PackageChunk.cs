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
using System.Collections.ObjectModel;

namespace ApkResourceParser
{
    /// <summary>
    /// A package chunk is a collection of resource data types within a package.
    /// </summary>
    public class PackageChunk : ChunkWithChunks
    {
        /// <summary>
        /// Offset in bytes, from the start of the chunk, where {@code typeStringsOffset} can be found.
        /// </summary>
        protected internal const int TYPE_OFFSET_OFFSET = 268;

        /// <summary>
        /// Offset in bytes, from the start of the chunk, where {@code keyStringsOffset} can be found.
        /// </summary>
        protected internal const int KEY_OFFSET_OFFSET = 276;

        protected internal const int HEADER_SIZE = KEY_OFFSET_OFFSET + 12;

        /// <summary>
        /// The offset (from {@code offset}) in the original buffer where type strings start.
        /// </summary>
        protected internal readonly int typeStringsOffset;

        /// <summary>
        /// An offset to the string pool that contains the key strings for this package.
        /// </summary>
        protected internal readonly int keyStringsOffset;

        /// <summary>
        /// The index into the type string pool of the last public type.
        /// </summary>
        private readonly int _lastPublicType;

        /// <summary>
        /// The index into the key string pool of the last public key.
        /// </summary>
        private readonly int _lastPublicKey;

        /// <summary>
        /// An offset to the type ID(s). This is undocumented in the original code.
        /// </summary>
        private readonly int _typeIdOffset;

        /// <summary>
        /// Contains a mapping of a type index to its <seealso cref="TypeSpecChunk"/>.
        /// </summary>
        private readonly IDictionary<int, TypeSpecChunk> _typeSpecs = new Dictionary<int, TypeSpecChunk>();

        /// <summary>
        /// Contains a mapping of a type index to all of the <seealso cref="TypeChunk"/> with that index.
        /// </summary>
        private readonly IDictionary<int, ISet<TypeChunk>> _types = new Dictionary<int, ISet<TypeChunk>>();

        /// <summary>
        /// The package id if this is a base package, or 0 if not a base package.
        /// </summary>
        private int _id;

        /// <summary>
        /// The name of the package.
        /// </summary>
        private string _packageName;

        /// <summary>
        /// May contain a library chunk for mapping dynamic references to resolved references.
        /// </summary>
        private LibraryChunk _libraryChunk = null;

        protected internal PackageChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            _id = buffer.getInt();
            _packageName = PackageUtils.readPackageName(buffer, buffer.position());
            typeStringsOffset = buffer.getInt();
            _lastPublicType = buffer.getInt();
            keyStringsOffset = buffer.getInt();
            _lastPublicKey = buffer.getInt();
            _typeIdOffset = buffer.getInt();
        }

        protected internal override void init(ByteBuffer buffer)
        {
            base.init(buffer);
            initializeChildMappings();
        }

        protected internal virtual void reinitializeChildMappings()
        {
            _types.Clear();
            _typeSpecs.Clear();
            _libraryChunk = null;
            initializeChildMappings();
        }

        private void initializeChildMappings()
        {
            foreach (Chunk chunk in getChunks().Values)
            {
                if (chunk is TypeChunk typeChunk)
                {
                    putIntoTypes(typeChunk);
                }
                else if (chunk is TypeSpecChunk typeSpecChunk)
                {
                    _typeSpecs[typeSpecChunk.getId()] = typeSpecChunk;
                }
                else if (chunk is LibraryChunk)
                {
                    if (_libraryChunk != null)
                    {
                        throw new InvalidOperationException("Multiple library chunks present in package chunk.");
                    }

                    // NB: this is currently unused except for the above assertion that there's <=1 chunk.
                    _libraryChunk = (LibraryChunk)chunk;
                }
                else if (!(chunk is StringPoolChunk) && !(chunk is UnknownChunk))
                {
                    throw new InvalidOperationException(string.Format("PackageChunk contains an unexpected chunk: {0}",
                        chunk.GetType()));
                }
            }
        }

        /// <summary>
        /// Returns the package id if this is a base package, or 0 if not a base package.
        /// </summary>
        public virtual int getId()
        {
            return _id;
        }

        /// <summary>
        /// Sets the package id
        /// </summary>
        public virtual void setId(int id)
        {
            this._id = id;
        }

        /// <summary>
        /// Returns the string pool that contains the names of the resources in this package.
        /// </summary>
        public virtual StringPoolChunk getKeyStringPool()
        {
            getChunks().TryGetValue(keyStringsOffset + offset, out var chunk);
            Preconditions.checkNotNull(chunk);

            if (chunk is StringPoolChunk stringPoolChunk)
            {
                return stringPoolChunk;
            }

            throw new InvalidOperationException("Key string pool not found.");
        }

        /// <summary>
        /// Get the type string for a specific id, e.g., (e.g. string, attr, id).
        /// </summary>
        /// <param name="id"> The id to get the type for. </param>
        /// <returns> The type string. </returns>
        public virtual string getTypeString(int id)
        {
            StringPoolChunk typePool = getTypeStringPool();
            Preconditions.checkNotNull(typePool, "Package has no type pool.");
            Preconditions.checkState(typePool.strings.Count >= id, "No type for id: " + id);
            return typePool.getString(id - 1); // - 1 here to convert to 0-based index
        }

        /// <summary>
        /// Returns the string pool that contains the type strings for this package, such as "layout",
        /// "string", "color".
        /// </summary>
        public virtual StringPoolChunk getTypeStringPool()
        {
            getChunks().TryGetValue(typeStringsOffset + offset, out var chunk);
            Preconditions.checkNotNull(chunk);
            Preconditions.checkState(chunk is StringPoolChunk, "Type string pool not found.");
            return (StringPoolChunk)chunk;
        }

        /// <summary>
        /// Returns all <seealso cref="TypeChunk"/> in this package.
        /// </summary>
        public virtual ICollection<TypeChunk> getTypeChunks()
        {
            ISet<TypeChunk> typeChunks = new HashSet<TypeChunk>();
            foreach (ICollection<TypeChunk> values in _types.Values)
            {
                foreach (var typeChunk in values)
                {
                    typeChunks.Add(typeChunk);
                }
            }

            return typeChunks;
        }

        /// <summary>
        /// For a given type id, returns the <seealso cref="TypeChunk"/> objects that match that id. The type id is
        /// the 1-based index of the type in the type string pool (returned by <seealso cref="getTypeStringPool"/>).
        /// </summary>
        /// <param name="id"> The 1-based type id to return <seealso cref="TypeChunk"/> objects for. </param>
        /// <returns> The matching <seealso cref="TypeChunk"/> objects, or an empty collection if there are none. </returns>
        public virtual ICollection<TypeChunk> getTypeChunks(int id)
        {
            _types.TryGetValue(id, out var chunks);
            return chunks != null ? chunks :  new HashSet<TypeChunk>();//TODO
        }

        /// <summary>
        /// For a given type, returns the <seealso cref="TypeChunk"/> objects that match that type
        /// (e.g. "attr", "id", "string", ...).
        /// </summary>
        /// <param name="type"> The type to return <seealso cref="TypeChunk"/> objects for. </param>
        /// <returns> The matching <seealso cref="TypeChunk"/> objects, or an empty collection if there are none. </returns>
        public virtual ICollection<TypeChunk> getTypeChunks(string type)
        {
            StringPoolChunk typeStringPool = Preconditions.checkNotNull(getTypeStringPool());
            return getTypeChunks(typeStringPool.indexOf(type) + 1); // Convert 0-based index to 1-based
        }

        /// <summary>
        /// Returns all <seealso cref="TypeSpecChunk"/> in this package.
        /// </summary>
        public virtual ICollection<TypeSpecChunk> getTypeSpecChunks()
        {
            return _typeSpecs.Values;
        }

        /// <summary>
        /// For a given (1-based) type id, returns the <seealso cref="TypeSpecChunk"/> matching it.
        /// </summary>
        public virtual TypeSpecChunk getTypeSpecChunk(int id)
        {
            _typeSpecs.TryGetValue(id, out var result);
            Preconditions.checkNotNull(result);
            return result;
        }

        /// <summary>
        /// For a given {@code type}, returns the <seealso cref="TypeSpecChunk"/> that matches it
        /// (e.g. "attr", "id", "string", ...).
        /// </summary>
        public virtual TypeSpecChunk getTypeSpecChunk(string type)
        {
            StringPoolChunk typeStringPool = Preconditions.checkNotNull(getTypeStringPool());
            return getTypeSpecChunk(typeStringPool.indexOf(type) + 1); // Convert 0-based index to 1-based
        }

        /// <summary>
        /// Returns the name of this package.
        /// </summary>
        public virtual string getPackageName()
        {
            return _packageName;
        }

        /// <summary>
        /// Set the package name
        /// </summary>
        public virtual void setPackageName(string packageName)
        {
            this._packageName = packageName;
        }


        protected internal override Type getType()
        {
            return Chunk.Type.TABLE_PACKAGE;
        }

        /// <summary>
        /// Using <seealso cref="types"/> as a {@code Multimap}, put a <seealso cref="TypeChunk"/> into it. The key is the id
        /// of the {@code typeChunk}.
        /// </summary>
        private void putIntoTypes(TypeChunk typeChunk)
        {
            _types.TryGetValue(typeChunk.getId(), out var chunks);
            if (chunks == null)
            {
                // Some tools require that the default TypeChunk is first in the list. Use a LinkedHashSet
                // to make sure that when we return the chunks they are in original order (in cases
                // where we copy and edit them this is important).
                chunks = new HashSet<TypeChunk>();
                _types[typeChunk.getId()] = chunks;
            }

            chunks.Add(typeChunk);
        }
    }
}