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
    /// Represents a single typed resource value.
    /// </summary>
    public class ResourceValue
    {
        /// <summary>
        /// The serialized size in bytes of a <seealso cref="ResourceValue"/>.
        /// </summary>
        public const int SIZE = 8;

        /// <summary>
        /// The length in bytes of this value.
        /// </summary>
        private int _size;

        /// <summary>
        /// The raw data type of this value.
        /// </summary>
        private Type _type;

        /// <summary>
        /// The actual 4-byte value; interpretation of the value depends on {@code dataType}.
        /// </summary>
        private int _data;

        /// <summary>
        /// Returns a new, empty builder for <seealso cref="ResourceValue"/> instances.
        /// </summary>
        public static Builder builder()
        {
            return new ResourceValue.Builder();
        }

        internal virtual Builder toBuilder()
        {
            return new ResourceValue.Builder().size(_size).type(_type).data(_data);
        }

        public static ResourceValue create(ByteBuffer buffer)
        {
            int size = buffer.getShort() & 0xFFFF;
            buffer.get(); // Unused
            var type = (ResourceValue.Type)buffer.get();
            int data = buffer.getInt();
            return builder().size(size).type(type).data(data).build();
        }

        internal virtual ResourceValue withData(int d)
        {
            return toBuilder().data(d).build();
        }

        public virtual int size()
        {
            return _size;
        }

        public virtual Type type()
        {
            return _type;
        }

        public virtual int data()
        {
            return _data;
        }

        private string dataHexString()
        {
            return $"0x{data():x8}";
        }

        public override string ToString()
        {
            var valueType = (ResourceValue.Type)type();
            return valueType switch
            {
                ResourceValue.Type.NULL => data() == 0 ? "null" : "empty",
                ResourceValue.Type.REFERENCE => "ref(" + dataHexString() + ")",
                ResourceValue.Type.ATTRIBUTE => "attr(" + dataHexString() + ")",
                ResourceValue.Type.STRING => "string(" + dataHexString() + ")",
                ResourceValue.Type.FLOAT => "float(" + data() + ")",
                ResourceValue.Type.DIMENSION => "dimen(" + data() + ")",
                ResourceValue.Type.FRACTION => "frac(" + data() + ")",
                ResourceValue.Type.DYNAMIC_REFERENCE => "dynref(" + dataHexString() + ")",
                ResourceValue.Type.DYNAMIC_ATTRIBUTE => "dynattr(" + dataHexString() + ")",
                ResourceValue.Type.INT_DEC => "dec(" + data() + ")",
                ResourceValue.Type.INT_HEX => "hex(" + dataHexString() + ")",
                ResourceValue.Type.INT_BOOLEAN => "bool(" + data() + ")",
                ResourceValue.Type.INT_COLOR_ARGB8 => "argb8(" + dataHexString() + ")",
                ResourceValue.Type.INT_COLOR_RGB8 => "rgb8(" + dataHexString() + ")",
                ResourceValue.Type.INT_COLOR_ARGB4 => "argb4(" + dataHexString() + ")",
                ResourceValue.Type.INT_COLOR_RGB4 => "rgb4(" + dataHexString() + ")",
                _ => "<invalid value>",
            };
        }


        /// <summary>
        /// Resource type codes.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// data is either 0 (undefined) or 1 (empty).
            /// </summary>
            NULL = 0x00,

            /// <summary>
            /// data holds a {@link ResourceTableChunk} entry reference.
            /// </summary>
            REFERENCE = 0x01,

            /// <summary>
            /// data holds an attribute resource identifier.
            /// </summary>
            ATTRIBUTE = 0x02,

            /// <summary>
            /// data holds an index into the containing resource table's string pool.
            /// </summary>
            STRING = 0x03,

            /// <summary>
            /// data holds a single-precision floating point number.
            /// </summary>
            FLOAT = 0x04,

            /// <summary>
            /// data holds a complex number encoding a dimension value, such as "100in".
            /// </summary>
            DIMENSION = 0x05,

            /// <summary>
            /// data holds a complex number encoding a fraction of a container.
            /// </summary>
            FRACTION = 0x06,

            /// <summary>
            /// data holds a dynamic {@link ResourceTableChunk} entry reference.
            /// </summary>
            DYNAMIC_REFERENCE = 0x07,

            /// <summary>
            /// data holds a dynamic attribute resource identifier.
            /// </summary>
            DYNAMIC_ATTRIBUTE = 0x08,

            /// <summary>
            /// data is a raw integer value of the form n..n.
            /// </summary>
            INT_DEC = 0x10,

            /// <summary>
            /// data is a raw integer value of the form 0xn..n.
            /// </summary>
            INT_HEX = 0x11,

            /// <summary>
            /// data is either 0 (false) or 1 (true).
            /// </summary>
            INT_BOOLEAN = 0x12,

            /// <summary>
            /// data is a raw integer value of the form #aarrggbb.
            /// </summary>
            INT_COLOR_ARGB8 = 0x1c,

            /// <summary>
            /// data is a raw integer value of the form #rrggbb.
            /// </summary>
            INT_COLOR_RGB8 = 0x1d,

            /// <summary>
            /// data is a raw integer value of the form #argb.
            /// </summary>
            INT_COLOR_ARGB4 = 0x1e,

            ///
            /// data is a raw integer value of the form #rgb.
            ///
            INT_COLOR_RGB4 = 0x1f
        }

        /// <summary>
        /// A builder for <seealso cref="ResourceValue"/> instances.
        /// </summary>
        public class Builder
        {
            internal readonly ResourceValue value;

            public Builder()
            {
                this.value = new ResourceValue();
            }

            public virtual Builder size(int s)
            {
                value._size = s;
                return this;
            }

            public virtual Builder type(Type t)
            {
                value._type = t;
                return this;
            }

            public virtual Builder data(int d)
            {
                value._data = d;
                return this;
            }

            public virtual ResourceValue build()
            {
                return value;
            }
        }
    }
}