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
    /// Provides utility methods for package names.
    /// </summary>
    public static class PackageUtils
    {
        public const int PACKAGE_NAME_SIZE = 256;

        /// <summary>
        /// Reads the package name from the buffer and repositions the buffer to point directly after
        /// the package name.
        /// </summary>
        /// <param name="buffer"> The buffer containing the package name. </param>
        /// <param name="offset"> The offset in the buffer to read from. </param>
        /// <returns> The package name. </returns>
        public static string readPackageName(ByteBuffer buffer, int offset)
        {
            byte[] data = buffer.data();
            int length = 0;
            // Look for the null terminator for the string instead of using the entire buffer.
            // It's UTF-16 so check 2 bytes at a time to see if its double 0.
            for (int i = offset; i < buffer.limit() && i < PACKAGE_NAME_SIZE + offset; i += 2)
            {
                if (data[i] == 0 && data[i + 1] == 0)
                {
                    length = i - offset;
                    break;
                }
            }

            string str = Encoding.Unicode.GetString(data, offset, length); //StandardCharsets.UTF_16LE
            buffer.position(offset + PACKAGE_NAME_SIZE);
            return str;
        }
    }
}