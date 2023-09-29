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

package ikas.project.java.apk.arsc;

import java.nio.ByteBuffer;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.Objects;

/**
 * Represents a chunk whose payload is a list of sub-chunks.
 */
public abstract class ChunkWithChunks extends Chunk {

    protected final Map<Integer, Chunk> chunks = new LinkedHashMap<>();

    protected ChunkWithChunks(ByteBuffer buffer, /*@Nullable*/ Chunk parent) {
        super(buffer, parent);
    }

    @Override
    protected void init(ByteBuffer buffer) {
        super.init(buffer);
        chunks.clear();
        int start = this.offset + getHeaderSize();
        int offset = start;
        int end = this.offset + getOriginalChunkSize();
        int position = buffer.position();
        buffer.position(start);

        while (offset < end) {
            Chunk chunk = createChildInstance(buffer);
            chunks.put(offset, chunk);
            offset += chunk.getOriginalChunkSize();
        }

        buffer.position(position);
    }

    /**
     * Allows subclasses to decide how child instances should be instantiated, e.g., compressed chunks
     * might use a different method to extract compressed data first.
     *
     * @param buffer The buffer to read from
     * @return The child instance.
     */
    protected Chunk createChildInstance(ByteBuffer buffer) {
        return Chunk.newInstance(buffer, this);
    }

    /**
     * Retrieves the @{code chunks} contained in this chunk.
     *
     * @return map of buffer offset -> chunk contained in this chunk.
     */
    public final Map<Integer, Chunk> getChunks() {
        return chunks;
    }

    protected void add(Chunk chunk) {
        int offset = 0;
        if (!chunks.isEmpty()) {
            int oldMax = Collections.max(chunks.keySet());
            Chunk oldChunk = Objects.requireNonNull(chunks.get(oldMax));
            offset = oldMax + oldChunk.getOriginalChunkSize();
        }
        chunks.put(offset, chunk);
        if (chunk.getParent() != this) {
            throw new IllegalStateException();
        }
    }

}
