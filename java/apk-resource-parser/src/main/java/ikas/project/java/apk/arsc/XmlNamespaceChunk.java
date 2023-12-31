package ikas.project.java.apk.arsc;

import java.nio.ByteBuffer;
import java.util.Locale;

/**
 * Represents the start/end of a namespace in an XML document.
 */
public abstract class XmlNamespaceChunk extends XmlNodeChunk {

    /**
     * A string reference to the namespace prefix.
     */
    private final int prefix;

    /**
     * A string reference to the namespace URI.
     */
    private final int uri;

    protected XmlNamespaceChunk(ByteBuffer buffer, /*@Nullable*/ Chunk parent) {
        super(buffer, parent);
        prefix = buffer.getInt();
        uri = buffer.getInt();
    }

    /**
     * Returns the namespace prefix.
     */
    public String getPrefix() {
        return getString(prefix);
    }

    /**
     * Returns the namespace URI.
     */
    public String getUri() {
        return getString(uri);
    }


    /**
     * Returns a brief description of this namespace chunk. The representation of this information is
     * subject to change, but below is a typical example:
     *
     * <pre>
     * "XmlNamespaceChunk{line=1234, comment=My awesome comment., prefix=foo, uri=com.google.foo}"
     * </pre>
     */
    @Override
    public String toString() {
        return String.format(
                Locale.US,
                "XmlNamespaceChunk{line=%d, comment=%s, prefix=%s, uri=%s}",
                getLineNumber(),
                getComment(),
                getPrefix(),
                getUri());
    }
}
