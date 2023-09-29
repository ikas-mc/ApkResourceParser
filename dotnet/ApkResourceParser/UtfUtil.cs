/*
 * Copyright 2020 Google Inc. All Rights Reserved.
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
    internal sealed class UtfUtil
    {
        private UtfUtil()
        {
        }

        internal static char[] decodeUtf8OrModifiedUtf8(ByteBuffer utf8Buffer, int characterCount)
        {
            char[] charBuffer = new char[characterCount];
            int offset = 0;
            while (offset < characterCount)
            {
                offset = decodeUtf8OrModifiedUtf8CodePoint(utf8Buffer, charBuffer, offset);
            }

            return charBuffer;
        }

        // This is a Javafied version of the implementation in ART:
        // cs/android/art/libdexfile/dex/utf-inl.h?l=32&rcl=4da82e1e9f201cb0e408499ee3b38cbca575698e
        private static int decodeUtf8OrModifiedUtf8CodePoint(ByteBuffer utf8Buffer, char[] outBuffer, int offset)
        {
            byte one = utf8Buffer.get();
            if ((one & 0x80) == 0)
            {
                outBuffer[offset++] = (char)one;
                return offset;
            }

            byte two = utf8Buffer.get();
            if ((one & 0x20) == 0)
            {
                outBuffer[offset++] = (char)(((one & 0x1f) << 6) | (two & 0x3f));
                return offset;
            }

            byte three = utf8Buffer.get();
            if ((one & 0x10) == 0)
            {
                outBuffer[offset++] = (char)(((one & 0x0f) << 12) | ((two & 0x3f) << 6) | (three & 0x3f));
                return offset;
            }

            byte four = utf8Buffer.get();
            int codePoint = ((one & 0x0f) << 18) | ((two & 0x3f) << 12) | ((three & 0x3f) << 6) | (four & 0x3f);

            // Write the code point out as a surrogate pair
            outBuffer[offset++] = (char)(((codePoint >> 10) + 0xd7c0) & 0xffff);
            outBuffer[offset++] = (char)((codePoint & 0x03ff) + 0xdc00);
            return offset;
        }
    }
}