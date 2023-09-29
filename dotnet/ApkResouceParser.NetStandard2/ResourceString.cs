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
using System.Text;

namespace ApkResourceParser
{
    /// <summary>
    /// Provides utilities to decode/encode a String packed in an arsc resource file.
    /// </summary>
    public sealed class ResourceString
    {
        private ResourceString()
        {
        } // Private constructor

        /// <summary>
        /// Given a buffer and an offset into the buffer, returns a String. The {@code offset} is the
        /// 0-based byte offset from the start of the buffer where the string resides. This should be the
        /// location in memory where the string's character count, followed by its byte count, and then
        /// followed by the actual string is located.
        /// 
        /// <para>Here's an example UTF-8-encoded string of ab©:
        /// <pre>
        /// 03 04 61 62 C2 A9 00
        /// ^ Offset should be here
        /// </pre>
        /// 
        /// </para>
        /// </summary>
        /// <param name="buffer"> The buffer containing the string to decode. </param>
        /// <param name="offset"> Offset into the buffer where the string resides. </param>
        /// <param name="type">   The encoding type that the <seealso cref="ResourceString"/> is encoded in. </param>
        /// <returns> The decoded string. </returns>
        public static string decodeString(ByteBuffer buffer, int offset, Type type)
        {
            int characterCount = decodeLength(buffer, offset, type);
            offset += computeLengthOffset(characterCount, type);
            // UTF-8 strings have 2 lengths: the number of characters, and then the encoding length.
            // UTF-16 strings, however, only have 1 length: the number of characters.
            if (type == Type.UTF8)
            {
                int length = decodeLength(buffer, offset, type);
                offset += computeLengthOffset(length, type);

                int origPosition = buffer.position();
                buffer.position(offset);
                try
                {
                    char[] chars = UtfUtil.decodeUtf8OrModifiedUtf8(buffer, characterCount);
                    return new string(chars);
                }
                finally
                {
                    buffer.position(origPosition);
                }
            }
            else
            {
                int length = characterCount * 2;
                var data = buffer.data();
                return Encoding.Unicode.GetString(data, offset, length);
            }
        }

        private static int computeLengthOffset(int length, Type type)
        {
            return (type == Type.UTF8 ? 1 : 2) * (length >= (type == Type.UTF8 ? 0x80 : 0x8000) ? 2 : 1);
        }

        private static int decodeLength(ByteBuffer buffer, int offset, Type type)
        {
            return type == Type.UTF8 ? decodeLengthUTF8(buffer, offset) : decodeLengthUTF16(buffer, offset);
        }

        private static int decodeLengthUTF8(ByteBuffer buffer, int offset)
        {
            // UTF-8 strings use a clever variant of the 7-bit integer for packing the string length.
            // If the first byte is >= 0x80, then a second byte follows. For these values, the length
            // is WORD-length in big-endian & 0x7FFF.
            int length = /*Byte.toUnsignedInt*/(buffer.get(offset));
            if ((length & 0x80) != 0)
            {
                length = ((length & 0x7F) << 8) | /*Byte.toUnsignedInt*/(buffer.get(offset + 1));
            }

            return length;
        }

        private static int decodeLengthUTF16(ByteBuffer buffer, int offset)
        {
            // UTF-16 strings use a clever variant of the 7-bit integer for packing the string length.
            // If the first word is >= 0x8000, then a second word follows. For these values, the length
            // is DWORD-length in big-endian & 0x7FFFFFFF.
            int length = (buffer.getShort(offset) & 0xFFFF);
            if ((length & 0x8000) != 0)
            {
                length = ((length & 0x7FFF) << 16) | (buffer.getShort(offset + 2) & 0xFFFF);
            }

            return length;
        }

        /// <summary>
        /// Type of <seealso cref="ResourceString"/> to encode / decode.
        /// </summary>
        public enum Type
        {
            UTF8,
            UTF16
        }
    }
}