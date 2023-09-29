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
using System.Text;

namespace ApkResourceParser
{
    /// <summary>
    /// Represents a type chunk, which contains the resource values for a specific resource type and
    /// configuration in a <seealso cref="PackageChunk"/>. The resource values in this chunk correspond to the
    /// array of type strings in the enclosing <seealso cref="PackageChunk"/>.
    /// 
    /// <para>A <seealso cref="PackageChunk"/> can have multiple of these chunks for different (configuration,
    /// resource type) combinations.
    /// </para>
    /// </summary>
    public class TypeChunk : Chunk
    {
        /// <summary>
        /// The size of a TypeChunk's header in bytes.
        /// </summary>
        internal const int HEADER_SIZE = Chunk.METADATA_SIZE + 12 + ResourceConfiguration.SIZE;

        /// <summary>
        /// If set, the entries in this chunk are sparse and encode both the entry ID and offset into each
        /// entry. Available on platforms >= O. Note that this only changes how the <seealso cref="TypeChunk"/> is
        /// encoded / decoded.
        /// </summary>
        private const int _FLAG_SPARSE = 1 << 0;

        /// <summary>
        /// A sparse list of resource entries defined by this chunk.
        /// </summary>
        protected internal readonly IDictionary<int, Entry> entries = new SortedDictionary<int, Entry>();

        /// <summary>
        /// The offset (from {@code offset}) in the original buffer where {@code entries} start.
        /// </summary>
        private readonly int _entriesStart;

        /// <summary>
        /// The type identifier of the resource type this chunk is holding.
        /// </summary>
        private int _id;

        /// <summary>
        /// Flags for a type chunk, such as whether or not this chunk has sparse entries.
        /// </summary>
        private int _flags;

        /// <summary>
        /// The number of resources of this type.
        /// </summary>
        private int _entryCount;

        /// <summary>
        /// The resource configuration that these resource entries correspond to.
        /// </summary>
        private ResourceConfiguration _configuration;

        protected internal TypeChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            _id = /*Byte.toUnsignedInt*/buffer.get();
            _flags = /*Byte.toUnsignedInt*/buffer.get();
            buffer.position(buffer.position() + 2); // Skip 2 bytes (reserved)
            _entryCount = buffer.getInt();
            _entriesStart = buffer.getInt();
            _configuration = ResourceConfiguration.create(buffer);
        }

        protected internal override void init(ByteBuffer buffer)
        {
            int offset = this.offset + _entriesStart;
            if (hasSparseEntries())
            {
                initSparseEntries(buffer, offset);
            }
            else
            {
                initDenseEntries(buffer, offset);
            }
        }

        private void initSparseEntries(ByteBuffer buffer, int offset)
        {
            for (int i = 0; i < _entryCount; ++i)
            {
                // Offsets are stored as (offset / 4u).
                // (See android::ResTable_sparseTypeEntry)
                int index = buffer.getShort() & 0xFFFF;
                int entryOffset = (buffer.getShort() & 0xFFFF) * 4;
                Entry entry = Entry.create(buffer, offset + entryOffset, this, index);
                entries[index] = entry;
            }
        }

        private void initDenseEntries(ByteBuffer buffer, int offset)
        {
            for (int i = 0; i < _entryCount; ++i)
            {
                int entryOffset = buffer.getInt();
                if (entryOffset == Entry.NO_ENTRY)
                {
                    continue;
                }

                Entry entry = Entry.create(buffer, offset + entryOffset, this, i);
                entries[i] = entry;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("TypeChunk[id:").Append(_id).Append(", typeName:").Append(getTypeName())
                .Append(", configuration:").Append(getConfiguration()).Append(", originalEntryCount:")
                .Append(getTotalEntryCount()).Append(", entries:");

            foreach (var entry in entries)
            {
                builder.Append("<").Append(entry.Key).Append("->").Append(entry.Value).Append("> ");
            }

            builder.Append("]");
            return builder.ToString();
        }


        /// <summary>
        /// Returns the (1-based) type id of the resource types that this <seealso cref="TypeChunk"/> is holding.
        /// </summary>
        public virtual int getId()
        {
            return _id;
        }


        /// <summary>
        /// Returns true if the entries in this chunk are encoded in a sparse array.
        /// </summary>
        public virtual bool hasSparseEntries()
        {
            return (_flags & _FLAG_SPARSE) != 0;
        }


        /// <summary>
        /// Returns the name of the type this chunk represents (e.g. string, attr, id).
        /// </summary>
        public virtual string getTypeName()
        {
            PackageChunk packageChunk = getPackageChunk();
            Preconditions.checkNotNull(packageChunk, $"{this.GetType()} has no parent package.");
            return packageChunk.getTypeString(getId());
        }

        /// <summary>
        /// Returns the resource configuration that these resource entries correspond to.
        /// </summary>
        public virtual ResourceConfiguration getConfiguration()
        {
            return _configuration;
        }


        /// <summary>
        /// Returns the total number of entries for this type + configuration, including null entries.
        /// </summary>
        public virtual int getTotalEntryCount()
        {
            return _entryCount;
        }

        /// <summary>
        /// Returns a sparse list of 0-based indices to resource entries defined by this chunk.
        /// </summary>
        public virtual IDictionary<int, Entry> getEntries()
        {
            return entries;//.ToImmutableDictionary();
        }

        /// <summary>
        /// Returns true if this chunk contains an entry for {@code resourceId}.
        /// </summary>
        public virtual bool containsResource(ResourceIdentifier resourceId)
        {
            PackageChunk packageChunk = Preconditions.checkNotNull(getPackageChunk());
            int packageId = packageChunk.getId();
            int typeId = getId();
            return resourceId.packageId == packageId && resourceId.typeId == typeId &&
                   entries.ContainsKey(resourceId.entryId);
        }

        protected internal virtual string getString(int index)
        {
            ResourceTableChunk resourceTable = getResourceTableChunk();
            Preconditions.checkNotNull(resourceTable, $"{this.GetType()} has no resource table.");
            return resourceTable.getStringPool().getString(index);
        }

        protected internal virtual string getKeyName(int index)
        {
            PackageChunk packageChunk = getPackageChunk();
            Preconditions.checkNotNull(packageChunk, $"{this.GetType()} has no parent package.");
            StringPoolChunk keyPool = packageChunk.getKeyStringPool();
            Preconditions.checkNotNull(keyPool, $"{this.GetType()}'s parent package has no key pool.");
            return keyPool.getString(index);
        }

        /*@Nullable*/
        private ResourceTableChunk getResourceTableChunk()
        {
            Chunk chunk = getParent();
            while (chunk != null && !(chunk is ResourceTableChunk))
            {
                chunk = chunk.getParent();
            }

            return chunk != null && chunk is ResourceTableChunk ? (ResourceTableChunk)chunk : null;
        }

        /// <summary>
        /// Returns the package enclosing this chunk, if any. Else, returns null.
        /// </summary>
        /*@Nullable*/
        public virtual PackageChunk getPackageChunk()
        {
            Chunk chunk = getParent();
            while (chunk != null && !(chunk is PackageChunk))
            {
                chunk = chunk.getParent();
            }

            return chunk != null && chunk is PackageChunk ? (PackageChunk)chunk : null;
        }

        protected internal override Type getType()
        {
            return Chunk.Type.TABLE_TYPE;
        }

        /// <summary>
        /// Returns the number of bytes needed for offsets based on {@code entries}.
        /// </summary>
        private int getOffsetSize()
        {
            return _entryCount * 4;
        }


        /// <summary>
        /// An <seealso cref="Entry"/> in a <seealso cref="TypeChunk"/>. Contains one or more <seealso cref="ResourceValue"/>.
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// An entry offset that indicates that a given resource is not present.
            /// </summary>
            public const int NO_ENTRY = unchecked((int)0xFFFFFFFF);

            /// <summary>
            /// Size of a simple resource
            /// </summary>
            public const int SIMPLE_HEADERSIZE = 8;

            /// <summary>
            /// Size of a complex resource
            /// </summary>
            public const int COMPLEX_HEADER_SIZE = 16;

            /// <summary>
            /// Set if this is a public resource, which allows libraries to reference it.
            /// </summary>
            internal const int FLAG_PUBLIC = 0x0002;

            /// <summary>
            /// Set if this is a complex resource. Otherwise, it's a simple resource.
            /// </summary>
            internal const int FLAG_COMPLEX = 0x0001;

            /// <summary>
            /// Size of a single resource id + value mapping entry.
            /// </summary>
            internal static readonly int MAPPING_SIZE = 4 + ResourceValue.SIZE;

            /// <summary>
            /// Number of bytes in the header of the <seealso cref="Entry"/>.
            /// </summary>
            private int _headerSize;

            /// <summary>
            /// Resource entry flags.
            /// </summary>
            private int _flags;

            /// <summary>
            /// Index into <seealso cref="PackageChunk.getKeyStringPool"/> identifying this entry.
            /// </summary>
            private int _keyIndex;

            /// <summary>
            /// The value of this resource entry, if this is not a complex entry. Else, null.
            /// </summary>
            /*@Nullable*/
            private ResourceValue _value;

            /// <summary>
            /// The extra values in this resource entry if this <seealso cref="isComplex"/>.
            /// </summary>
            private IDictionary<int, ResourceValue> _values;

            /// <summary>
            /// Entry into <seealso cref="PackageChunk"/> that is the parent <seealso cref="Entry"/> to this entry.
            /// This value only makes sense when this is complex (<seealso cref="isComplex"/> returns true).
            /// </summary>
            private int _parentEntry;

            /// <summary>
            /// The <seealso cref="TypeChunk"/> that this resource entry belongs to.
            /// </summary>
            private TypeChunk _parent;

            /// <summary>
            /// The entry's index into the parent TypeChunk.
            /// </summary>
            private int _typeChunkIndex;

            public static Builder builder()
            {
                return new Builder();
            }

            /// <summary>
            /// Creates a new <seealso cref="Entry"/> whose contents start at {@code offset} in the given {@code
            /// buffer}.
            /// </summary>
            /// <param name="buffer">         The buffer to read <seealso cref="Entry"/> from. </param>
            /// <param name="offset">         Offset into the buffer where <seealso cref="Entry"/> is located. </param>
            /// <param name="parent">         The <seealso cref="TypeChunk"/> that this resource entry belongs to. </param>
            /// <param name="typeChunkIndex"> The entry's index into the parent TypeChunk. </param>
            /// <returns> New <seealso cref="Entry"/>. </returns>
            public static Entry create(ByteBuffer buffer, int offset, TypeChunk parent, int typeChunkIndex)
            {
                int position = buffer.position();
                buffer.position(offset); // Set buffer position to resource entry start
                Entry result = newInstance(buffer, parent, typeChunkIndex);
                buffer.position(position); // Restore buffer position
                return result;
            }

            internal static Entry newInstance(ByteBuffer buffer, TypeChunk parent, int typeChunkIndex)
            {
                int headerSize = buffer.getShort() & 0xFFFF;
                int flags = buffer.getShort() & 0xFFFF;
                int keyIndex = buffer.getInt();
                ResourceValue value = null;
                IDictionary<int, ResourceValue> values = new Dictionary<int, ResourceValue>();
                int parentEntry = 0;
                if ((flags & FLAG_COMPLEX) != 0)
                {
                    parentEntry = buffer.getInt();
                    int valueCount = buffer.getInt();
                    for (int i = 0; i < valueCount; ++i)
                    {
                        values[buffer.getInt()] = ResourceValue.create(buffer);
                    }
                }
                else
                {
                    value = ResourceValue.create(buffer);
                }

                return builder().headerSize(headerSize).flags(flags).keyIndex(keyIndex).value(value).values(values)
                    .parentEntry(parentEntry).parent(parent).typeChunkIndex(typeChunkIndex).build();
            }

            public virtual int headerSize()
            {
                return _headerSize;
            }

            public virtual int flags()
            {
                return _flags;
            }

            public virtual int keyIndex()
            {
                return _keyIndex;
            }

            public virtual ResourceValue value()
            {
                return _value;
            }

            public virtual IDictionary<int, ResourceValue> values()
            {
                return _values;
            }

            public virtual int parentEntry()
            {
                return _parentEntry;
            }

            public virtual TypeChunk parent()
            {
                return _parent;
            }

            public virtual int typeChunkIndex()
            {
                return _typeChunkIndex;
            }

            public virtual Builder toBuilder()
            {
                return (new Builder()).headerSize(_headerSize).flags(_flags).keyIndex(_keyIndex).value(_value)
                    .values(_values).parentEntry(_parentEntry).parent(_parent).typeChunkIndex(_typeChunkIndex);
            }

            public virtual Entry withKeyIndex(int keyIndex)
            {
                return toBuilder().keyIndex(keyIndex).build();
            }

            public virtual Entry withValue(ResourceValue value)
            {
                return toBuilder().value(value).build();
            }

            public virtual Entry withValues(IDictionary<int, ResourceValue> values)
            {
                return toBuilder().values(values).build();
            }

            /// <summary>
            /// Returns the name of the type this chunk represents (e.g. string, attr, id).
            /// </summary>
            public string typeName()
            {
                return parent().getTypeName();
            }

            /// <summary>
            /// The total number of bytes that this <seealso cref="Entry"/> takes up.
            /// </summary>
            public int size()
            {
                return headerSize() + (isComplex() ? values().Count * MAPPING_SIZE : ResourceValue.SIZE);
            }

            /// <summary>
            /// Returns the key name identifying this resource entry.
            /// </summary>
            public string key()
            {
                return parent().getKeyName(keyIndex());
            }

            /// <summary>
            /// Returns true if this is a complex resource.
            /// </summary>
            public bool isComplex()
            {
                return (flags() & FLAG_COMPLEX) != 0;
            }

            /// <summary>
            /// Returns true if this is a public resource.
            /// </summary>
            public bool isPublic()
            {
                return (flags() & FLAG_PUBLIC) != 0;
            }


            public override sealed string ToString()
            {
                return $"Entry{{key={key()},value={value()},values={values()}}}";
            }


            public class Builder
            {
                internal readonly Entry entry;

                public Builder()
                {
                    this.entry = new Entry();
                }

                public virtual Builder headerSize(int h)
                {
                    this.entry._headerSize = h;
                    return this;
                }

                public virtual Builder flags(int f)
                {
                    this.entry._flags = f;
                    return this;
                }

                public virtual Builder keyIndex(int k)
                {
                    this.entry._keyIndex = k;
                    return this;
                }

                public virtual Builder value(ResourceValue r)
                {
                    this.entry._value = r;
                    return this;
                }

                public virtual Builder values(IDictionary<int, ResourceValue> v)
                {
                    this.entry._values = v;
                    return this;
                }

                public virtual Builder parentEntry(int p)
                {
                    this.entry._parentEntry = p;
                    return this;
                }

                public virtual Builder parent(TypeChunk p)
                {
                    this.entry._parent = p;
                    return this;
                }

                public virtual Builder typeChunkIndex(int typeChunkIndex)
                {
                    this.entry._typeChunkIndex = typeChunkIndex;
                    return this;
                }

                public virtual Entry build()
                {
                    return entry;
                }
            }
        }
    }
}