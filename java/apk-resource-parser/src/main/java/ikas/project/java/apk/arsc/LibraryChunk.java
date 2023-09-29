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
import java.util.ArrayList;
import java.util.List;

/**
 * Contains a list of package-id to package name mappings for any shared libraries used in this
 * {@link ResourceTableChunk}. The package-id's encoded in this resource table may be different
 * than the id's assigned at runtime
 */
public final class LibraryChunk extends Chunk {

    /**
     * The number of resources of this type at creation time.
     */
    private final int entryCount;

    /**
     * The libraries used in this chunk (package id + name).
     */
    private final List<Entry> entries = new ArrayList<>();

    LibraryChunk(ByteBuffer buffer, /*@Nullable*/ Chunk parent) {
        super(buffer, parent);
        entryCount = buffer.getInt();
    }

    @Override
    protected void init(ByteBuffer buffer) {
        super.init(buffer);
        entries.addAll(enumerateEntries(buffer));
    }

    private List<Entry> enumerateEntries(ByteBuffer buffer) {
        List<Entry> result = new ArrayList<>(entryCount);
        int offset = this.offset + getHeaderSize();
        int endOffset = offset + Entry.SIZE * entryCount;

        while (offset < endOffset) {
            result.add(Entry.create(buffer, offset));
            offset += Entry.SIZE;
        }
        return result;
    }

    @Override
    protected Type getType() {
        return Chunk.Type.TABLE_LIBRARY;
    }

    /**
     * A shared library package-id to package name entry.
     */
    protected record Entry(int packageId, String packageName) {

        /**
         * Library entries only contain a package ID (4 bytes) and a package name.
         */
        private static final int SIZE = 4 + PackageUtils.PACKAGE_NAME_SIZE;

        static Entry create(ByteBuffer buffer, int offset) {
            int packageId = buffer.getInt(offset);
            String packageName = PackageUtils.readPackageName(buffer, offset + 4);
            return new Entry(packageId, packageName);
        }

    }
}
