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
    /// Given an arsc file, maps the contents of the file.
    /// </summary>
    public sealed class ResourceFile
    {
        /// <summary>
        /// The chunks contained in this resource file.
        /// </summary>
        private readonly List<Chunk> _chunks = new();

        public ResourceFile(ByteBuffer buf)
        {
            //buf.order(ByteOrder.LITTLE_ENDIAN);
            while (buf.remaining() > 0)
            {
                _chunks.Add(Chunk.newInstance(buf));
            }
        }

        /// <summary>
        /// Returns the chunks in this resource file.
        /// </summary>
        public IList<Chunk> getChunks()
        {
            return _chunks;
        }
    }
}