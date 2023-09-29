namespace ApkResourceParser
{
    /// <summary>
    /// Represents the start/end of a namespace in an XML document.
    /// </summary>
    public abstract class XmlNamespaceChunk : XmlNodeChunk
    {
        /// <summary>
        /// A string reference to the namespace prefix.
        /// </summary>
        private readonly int _prefix;

        /// <summary>
        /// A string reference to the namespace URI.
        /// </summary>
        private readonly int _uri;

        protected internal XmlNamespaceChunk(ByteBuffer buffer, Chunk parent) : base(buffer, parent)
        {
            _prefix = buffer.getInt();
            _uri = buffer.getInt();
        }

        /// <summary>
        /// Returns the namespace prefix.
        /// </summary>
        public virtual string getPrefix()
        {
            return getString(_prefix);
        }

        /// <summary>
        /// Returns the namespace URI.
        /// </summary>
        public virtual string getUri()
        {
            return getString(_uri);
        }


        /// <summary>
        /// Returns a brief description of this namespace chunk. The representation of this information is
        /// subject to change, but below is a typical example:
        /// 
        /// <pre>
        /// "XmlNamespaceChunk{line=1234, comment=My awesome comment., prefix=foo, uri=com.google.foo}"
        /// </pre>
        /// </summary>
        public override string ToString()
        {
            return $"XmlNamespaceChunk{{line={getLineNumber()}, comment={getComment()}, prefix={getPrefix()}, uri={getUri()}}}";
        }
    }
}