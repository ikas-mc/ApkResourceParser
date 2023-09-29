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
    /// A chunk whose contents are unknown. This is a placeholder until we add a proper chunk for the
    /// unknown type.
    /// </summary>
    public sealed class UnknownChunk : Chunk
    {
        private readonly Type _type;

        private readonly byte[] _header;

        private readonly byte[] _payload;

        internal UnknownChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            _type = (Type)buffer.getShort(offset);
            _header = new byte[headerSize - Chunk.METADATA_SIZE];
            _payload = new byte[chunkSize - headerSize];
            buffer.get(_header);
            buffer.get(_payload);
        }

        protected internal override Type getType()
        {
            return _type;
        }

        public override string ToString()
        {
            return $"unknown chunk,type={_type}";
        }
    }
}