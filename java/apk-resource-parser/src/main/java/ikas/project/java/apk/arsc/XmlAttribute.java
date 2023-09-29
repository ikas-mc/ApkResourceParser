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

/**
 * Represents an XML attribute and value.
 */
public record XmlAttribute(int namespaceIndex, int nameIndex, int rawValueIndex, ResourceValue typedValue, XmlNodeChunk parent) {

    /**
     * The serialized size in bytes of an {@link ikas.project.java.apk.arsc.XmlAttribute}.
     */
    public static final int SIZE = 12 + ResourceValue.SIZE;

    /**
     * Creates a new {@link ikas.project.java.apk.arsc.XmlAttribute} based on the bytes at the current {@code buffer} position.
     *
     * @param buffer A buffer whose position is at the start of a {@link ikas.project.java.apk.arsc.XmlAttribute}.
     * @param parent The parent chunk that contains this attribute; used for string lookups.
     */
    public static XmlAttribute create(ByteBuffer buffer, XmlNodeChunk parent) {
        int namespace = buffer.getInt();
        int name = buffer.getInt();
        int rawValue = buffer.getInt();
        ResourceValue typedValue = ResourceValue.create(buffer);
        return create(namespace, name, rawValue, typedValue, parent);
    }

    public static XmlAttribute create(
            int namespace, int name, int rawValue, ResourceValue typedValue, XmlNodeChunk parent) {
        return new XmlAttribute(namespace, name, rawValue, typedValue, parent);
    }

    /**
     * The namespace URI, or the empty string if not present.
     */
    public String namespace() {
        return getString(namespaceIndex());
    }

    /**
     * The attribute name, or the empty string if not present.
     */
    public String name() {
        return getString(nameIndex());
    }

    /**
     * The raw character value.
     */
    public String rawValue() {
        return getString(rawValueIndex());
    }

    private String getString(int index) {
        return parent().getString(index);
    }

    /**
     * Returns a brief description of this XML attribute. The representation of this information is
     * subject to change, but below is a typical example:
     *
     * <pre>"XmlAttribute{namespace=foo, name=bar, value=1234}"</pre>
     */
    @Override
    public String toString() {
        return String.format("XmlAttribute{namespace=%s, name=%s, value=%s}",
                namespace(), name(), rawValue());
    }
}
