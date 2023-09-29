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
import java.util.Collection;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;

/**
 * Represents a resource table structure. Its sub-chunks contain:
 *
 * <ul>
 *   <li>A {@link StringPoolChunk} containing all string values in the entire resource table. It
 *       does not, however, contain the names of entries or type identifiers.
 *   <li>One or more {@link PackageChunk}.
 * </ul>
 */
public class ResourceTableChunk extends ChunkWithChunks {

    protected static final int HEADER_SIZE = Chunk.METADATA_SIZE + 4; // +4 = package count
    /**
     * The packages contained in this resource table.
     */
    private final Map<String, PackageChunk> packages = new HashMap<>();
    /**
     * A string pool containing all string resource values in the entire resource table.
     */
    private StringPoolChunk stringPool;

    protected ResourceTableChunk(ByteBuffer buffer, /*@Nullable*/ Chunk parent) {
        super(buffer, parent);
        // packageCount. We ignore this, because we already know how many chunks we have.
        Preconditions.checkState(buffer.getInt() >= 1, "ResourceTableChunk package count was < 1.");
    }

    private static boolean isString(ResourceValue value) {
        return value.type() == ResourceValue.Type.STRING;
    }

    @Override
    protected void init(ByteBuffer buffer) {
        super.init(buffer);
        setChildChunks();
    }

    protected void setChildChunks() {
        packages.clear();
        for (Chunk chunk : getChunks().values()) {
            if (chunk instanceof PackageChunk packageChunk) {
                packages.put(packageChunk.getPackageName(), packageChunk);
            } else if (chunk instanceof StringPoolChunk) {
                stringPool = (StringPoolChunk) chunk;
            }
        }
        Preconditions.checkNotNull(stringPool, "ResourceTableChunk must have a string pool.");
    }

    /**
     * Returns the string pool containing all string resource values in the resource table.
     */
    public StringPoolChunk getStringPool() {
        return stringPool;
    }

    /**
     * Adds the {@link PackageChunk} to this table.
     */
    public void addPackageChunk(PackageChunk packageChunk) {
        super.add(packageChunk);
        this.packages.put(packageChunk.getPackageName(), packageChunk);
    }

    /**
     * Returns the package with the given {@code packageName}. Else, returns null.
     */
    /*@Nullable*/
    public PackageChunk getPackage(String packageName) {
        return packages.get(packageName);
    }

    /**
     * Returns the package with the given {@code packageId}. Else, returns null
     */
    /*@Nullable*/
    public PackageChunk getPackage(int packageId) {
        for (PackageChunk chunk : packages.values()) {
            if (chunk.getId() == packageId) {
                return chunk;
            }
        }
        return null;
    }

    /**
     * Returns the packages contained in this resource table.
     */
    public Collection<PackageChunk> getPackages() {
        return Collections.unmodifiableCollection(packages.values());
    }

    @Override
    protected Type getType() {
        return Chunk.Type.TABLE;
    }

}
