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
    /// Represents a generic chunk.
    /// </summary>
    public abstract class Chunk
    {
        /// <summary>
        /// The byte boundary to pad chunks on.
        /// </summary>
        public const int PAD_BOUNDARY = 4;

        /// <summary>
        /// The number of bytes in every chunk that describes chunk type, header size, and chunk size.
        /// </summary>
        public const int METADATA_SIZE = 8;

        /// <summary>
        /// The number of bytes in every chunk that describes header size and chunk size, but not the type.
        /// </summary>
        public const int METADATA_SIZE_NO_TYPE = METADATA_SIZE - 2;

        /// <summary>
        /// The offset in bytes, from the start of the chunk, where the chunk size can be found.
        /// </summary>
        private const int CHUNK_SIZE_OFFSET = 4;

        /// <summary>
        /// Size of the chunk header in bytes.
        /// </summary>
        protected internal readonly int headerSize;

        /// <summary>
        /// headerSize + dataSize. The total size of this chunk.
        /// </summary>
        protected internal readonly int chunkSize;

        /// <summary>
        /// Offset of this chunk from the start of the file.
        /// </summary>
        protected internal readonly int offset;

        /// <summary>
        /// The parent to this chunk, if any.
        /// </summary>
        /*@Nullable*/
        private readonly Chunk _parent;

        protected internal Chunk(ByteBuffer buffer, Chunk parent)
        {
            this._parent = parent;
            offset = buffer.position() - 2;
            headerSize = buffer.getShort() & 0xFFFF;
            chunkSize = buffer.getInt();
        }

        /// <summary>
        /// Creates a new chunk whose contents start at {@code buffer}'s current position.
        /// </summary>
        /// <param name="buffer"> A buffer positioned at the start of a chunk. </param>
        /// <returns> new chunk </returns>
        public static Chunk newInstance(ByteBuffer buffer)
        {
            return newInstance(buffer, null);
        }

        /// <summary>
        /// Creates a new chunk whose contents start at {@code buffer}'s current position.
        /// </summary>
        /// <param name="buffer"> A buffer positioned at the start of a chunk. </param>
        /// <param name="parent"> The parent to this chunk (or null if there's no parent). </param>
        /// <returns> new chunk </returns>
        public static Chunk newInstance(ByteBuffer buffer, Chunk parent)
        {
            var tyep = (Chunk.Type)buffer.getShort();
            Chunk result = tyep switch
            {
                Chunk.Type.STRING_POOL => new StringPoolChunk(buffer, parent),
                Chunk.Type.TABLE => new ResourceTableChunk(buffer, parent),
                Chunk.Type.XML => new XmlChunk(buffer, parent),
                Chunk.Type.XML_START_NAMESPACE => new XmlNamespaceStartChunk(buffer, parent),
                Chunk.Type.XML_END_NAMESPACE => new XmlNamespaceEndChunk(buffer, parent),
                Chunk.Type.XML_START_ELEMENT => new XmlStartElementChunk(buffer, parent),
                Chunk.Type.XML_END_ELEMENT => new XmlEndElementChunk(buffer, parent),
                Chunk.Type.XML_CDATA => new XmlCdataChunk(buffer, parent),
                Chunk.Type.XML_RESOURCE_MAP => new XmlResourceMapChunk(buffer, parent),
                Chunk.Type.TABLE_PACKAGE => new PackageChunk(buffer, parent),
                Chunk.Type.TABLE_TYPE => new TypeChunk(buffer, parent),
                Chunk.Type.TABLE_TYPE_SPEC => new TypeSpecChunk(buffer, parent),
                Chunk.Type.TABLE_LIBRARY => new LibraryChunk(buffer, parent),
                _ => new UnknownChunk(buffer, parent),
            };
            result.init(buffer);
            result.seekToEndOfChunk(buffer);
            return result;
        }

        /// <summary>
        /// Finishes initialization of a chunk. This should be called immediately after the constructor.
        /// This is separate from the constructor so that the header of a chunk can be fully initialized
        /// before the payload of that chunk is initialized for chunks that require such behavior.
        /// </summary>
        /// <param name="buffer"> The buffer that the payload will be initialized from. </param>
        protected internal virtual void init(ByteBuffer buffer)
        {
        }

        /// <summary>
        /// Returns the parent to this chunk, if any. A parent is a chunk whose payload contains this
        /// chunk. If there's no parent, null is returned.
        /// </summary>
        ///*@Nullable*/
        public virtual Chunk getParent()
        {
            return _parent;
        }

        protected internal abstract Type getType();

        /// <summary>
        /// Returns the size of this chunk's header.
        /// </summary>
        public int getHeaderSize()
        {
            return headerSize;
        }

        /// <summary>
        /// Returns the size of this chunk when it was first read from a buffer. A chunk's size can deviate
        /// from this value when its data is modified (e.g. adding an entry, changing a string).
        /// 
        /// <para>A chunk's current size can be determined from the length of the byte array returned from
        /// </para>
        /// </summary>
        public int getOriginalChunkSize()
        {
            return chunkSize;
        }

        /// <summary>
        /// Reposition the buffer after this chunk. Use this at the end of a Chunk constructor.
        /// </summary>
        /// <param name="buffer"> The buffer to be repositioned. </param>
        protected internal void seekToEndOfChunk(ByteBuffer buffer)
        {
            buffer.position(offset + chunkSize);
        }


        /// <summary>
        /// Allows overwriting what value gets written as the type of a chunk. Subclasses may use a
        /// specialized type value schema.
        /// </summary>
        /// <returns> The type value for this chunk. </returns>
        protected internal virtual short getTypeValue()
        {
            return (short)getType();
        }

        /// <summary>
        /// Types of chunks that can exist.
        /// </summary>
        // See
        // https://android.googlesource.com/platform/frameworks/base/+/refs/heads/master/libs/androidfw/include/androidfw/ResourceTypes.h
        public enum Type : short
        {
            NULL = 0x0000,
            STRING_POOL = 0x0001,
            TABLE = 0x0002,
            XML = 0x0003,
            XML_START_NAMESPACE = 0x0100,
            XML_END_NAMESPACE = 0x0101,
            XML_START_ELEMENT = 0x0102,
            XML_END_ELEMENT = 0x0103,
            XML_CDATA = 0x0104,
            XML_RESOURCE_MAP = 0x0180,
            TABLE_PACKAGE = 0x0200,
            TABLE_TYPE = 0x0201,
            TABLE_TYPE_SPEC = 0x0202,
            TABLE_LIBRARY = 0x0203,
            TABLE_OVERLAYABLE = 0x204,
            TABLE_OVERLAYABLE_POLICY = 0x205
        }
    }
}