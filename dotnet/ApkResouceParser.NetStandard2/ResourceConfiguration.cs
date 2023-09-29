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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ApkResourceParser
{
    /// <summary>
    /// Describes a particular resource configuration.
    /// </summary>
    public class ResourceConfiguration
    {
        public enum Type
        {
            MCC,
            MNC,
            LANGUAGE_STRING,
            LOCALE_SCRIPT_STRING,
            REGION_STRING,
            LOCALE_VARIANT_STRING,
            SCREEN_LAYOUT_DIRECTION,
            SMALLEST_SCREEN_WIDTH_DP,
            SCREEN_WIDTH_DP,
            SCREEN_HEIGHT_DP,
            SCREEN_LAYOUT_SIZE,
            SCREEN_LAYOUT_LONG,
            SCREEN_LAYOUT_ROUND,
            COLOR_MODE_WIDE_COLOR_GAMUT, // NB: COLOR_GAMUT takes priority over HDR in #isBetterThan.
            COLOR_MODE_HDR,
            ORIENTATION,
            UI_MODE_TYPE,
            UI_MODE_NIGHT,
            DENSITY_DPI,
            TOUCHSCREEN,
            KEYBOARD_HIDDEN,
            KEYBOARD,
            NAVIGATION_HIDDEN,
            NAVIGATION,
            SCREEN_SIZE,
            SDK_VERSION
        }


        /// <summary>
        /// The below constants are from android.content.res.Configuration.
        /// </summary>
        internal const int COLOR_MODE_WIDE_COLOR_GAMUT_MASK = 0x03;

        internal const int COLOR_MODE_WIDE_COLOR_GAMUT_UNDEFINED = 0;
        internal const int COLOR_MODE_WIDE_COLOR_GAMUT_NO = 0x01;
        internal const int COLOR_MODE_WIDE_COLOR_GAMUT_YES = 0x02;
        internal const int COLOR_MODE_HDR_MASK = 0x0C;
        internal const int COLOR_MODE_HDR_UNDEFINED = 0;
        internal const int COLOR_MODE_HDR_NO = 0x04;
        internal const int COLOR_MODE_HDR_YES = 0x08;
        internal const int DENSITY_DPI_UNDEFINED = 0;
        internal const int DENSITY_DPI_LDPI = 120;
        internal const int DENSITY_DPI_MDPI = 160;
        internal const int DENSITY_DPI_TVDPI = 213;
        internal const int DENSITY_DPI_HDPI = 240;
        internal const int DENSITY_DPI_XHDPI = 320;
        internal const int DENSITY_DPI_XXHDPI = 480;
        internal const int DENSITY_DPI_XXXHDPI = 640;
        internal const int DENSITY_DPI_ANY = 0xFFFE;
        internal const int DENSITY_DPI_NONE = 0xFFFF;
        internal const int KEYBOARD_NOKEYS = 1;
        internal const int KEYBOARD_QWERTY = 2;
        internal const int KEYBOARD_12KEY = 3;
        internal const int KEYBOARDHIDDEN_MASK = 0x03;
        internal const int KEYBOARDHIDDEN_NO = 1;
        internal const int KEYBOARDHIDDEN_YES = 2;
        internal const int KEYBOARDHIDDEN_SOFT = 3;
        internal const int NAVIGATION_NONAV = 1;
        internal const int NAVIGATION_DPAD = 2;
        internal const int NAVIGATION_TRACKBALL = 3;
        internal const int NAVIGATION_WHEEL = 4;
        internal const int NAVIGATIONHIDDEN_MASK = 0x0C;
        internal const int NAVIGATIONHIDDEN_NO = 0x04;
        internal const int NAVIGATIONHIDDEN_YES = 0x08;
        internal const int ORIENTATION_PORTRAIT = 0x01;
        internal const int ORIENTATION_LANDSCAPE = 0x02;
        internal const int SCREENLAYOUT_LAYOUTDIR_MASK = 0xC0;
        internal const int SCREENLAYOUT_LAYOUTDIR_LTR = 0x40;
        internal const int SCREENLAYOUT_LAYOUTDIR_RTL = 0x80;
        internal const int SCREENLAYOUT_LONG_MASK = 0x30;
        internal const int SCREENLAYOUT_LONG_NO = 0x10;
        internal const int SCREENLAYOUT_LONG_YES = 0x20;
        internal const int SCREENLAYOUT_ROUND_MASK = 0x03;
        internal const int SCREENLAYOUT_ROUND_NO = 0x01;
        internal const int SCREENLAYOUT_ROUND_YES = 0x02;
        internal const int SCREENLAYOUT_SIZE_MASK = 0x0F;
        internal const int SCREENLAYOUT_SIZE_SMALL = 0x01;
        internal const int SCREENLAYOUT_SIZE_NORMAL = 0x02;
        internal const int SCREENLAYOUT_SIZE_LARGE = 0x03;
        internal const int SCREENLAYOUT_SIZE_XLARGE = 0x04;
        internal const int TOUCHSCREEN_NOTOUCH = 1;
        internal const int TOUCHSCREEN_FINGER = 3;
        internal const int UI_MODE_NIGHT_MASK = 0x30;
        internal const int UI_MODE_NIGHT_NO = 0x10;
        internal const int UI_MODE_NIGHT_YES = 0x20;
        internal const int UI_MODE_TYPE_MASK = 0x0F;
        internal const int UI_MODE_TYPE_DESK = 0x02;
        internal const int UI_MODE_TYPE_CAR = 0x03;
        internal const int UI_MODE_TYPE_TELEVISION = 0x04;
        internal const int UI_MODE_TYPE_APPLIANCE = 0x05;
        internal const int UI_MODE_TYPE_WATCH = 0x06;
        internal const int UI_MODE_TYPE_VR_HEADSET = 0x07;
        private static readonly IDictionary<int, string> _COLOR_MODE_WIDE_COLOR_GAMUT_VALUES;
        private static readonly IDictionary<int, string> _COLOR_MODE_HDR_VALUES;
        private static readonly IDictionary<int, string> _DENSITY_DPI_VALUES;
        private static readonly IDictionary<int, string> _KEYBOARD_VALUES;
        private static readonly IDictionary<int, string> _KEYBOARDHIDDEN_VALUES;
        private static readonly IDictionary<int, string> _NAVIGATION_VALUES;
        private static readonly IDictionary<int, string> _NAVIGATIONHIDDEN_VALUES;
        private static readonly IDictionary<int, string> _ORIENTATION_VALUES;
        private static readonly IDictionary<int, string> _SCREENLAYOUT_LAYOUTDIR_VALUES;
        private static readonly IDictionary<int, string> _SCREENLAYOUT_LONG_VALUES;
        private static readonly IDictionary<int, string> _SCREENLAYOUT_ROUND_VALUES;
        private static readonly IDictionary<int, string> _SCREENLAYOUT_SIZE_VALUES;
        private static readonly IDictionary<int, string> _TOUCHSCREEN_VALUES;
        private static readonly IDictionary<int, string> _UI_MODE_NIGHT_VALUES;
        private static readonly IDictionary<int, string> _UI_MODE_TYPE_VALUES;

        /// <summary>
        /// The minimum size in bytes that a <seealso cref="ResourceConfiguration"/> can be.
        /// </summary>
        private const int MIN_SIZE = 28;

        /// <summary>
        /// The minimum size in bytes that this configuration must be to contain screen config info.
        /// </summary>
        private const int SCREEN_CONFIG_MIN_SIZE = 32;

        /// <summary>
        /// The minimum size in bytes that this configuration must be to contain screen dp info.
        /// </summary>
        private const int SCREEN_DP_MIN_SIZE = 36;

        /// <summary>
        /// The minimum size in bytes that this configuration must be to contain locale info.
        /// </summary>
        private const int LOCALE_MIN_SIZE = 48;

        /// <summary>
        /// The minimum size in bytes that this config must be to contain the screenConfig extension.
        /// </summary>
        private const int SCREEN_CONFIG_EXTENSION_MIN_SIZE = 52;

        /// <summary>
        /// The size of resource configurations in bytes for the latest version of Android resources.
        /// </summary>
        public const int SIZE = SCREEN_CONFIG_EXTENSION_MIN_SIZE;

        private static readonly Builder _DEFAULT_BUILDER = builder();

        /// <summary>
        /// The default configuration. This configuration acts as a "catch-all" for looking up resources
        /// when no better configuration can be found.
        /// </summary>
        public static readonly ResourceConfiguration DEFAULT = _DEFAULT_BUILDER.build();

        static ResourceConfiguration()
        {
            IDictionary<int, string> map = new Dictionary<int, string>();
            map[COLOR_MODE_WIDE_COLOR_GAMUT_UNDEFINED] = "";
            map[COLOR_MODE_WIDE_COLOR_GAMUT_NO] = "nowidecg";
            map[COLOR_MODE_WIDE_COLOR_GAMUT_YES] = "widecg";
            _COLOR_MODE_WIDE_COLOR_GAMUT_VALUES = new ReadOnlyDictionary<int, string>(map);
            IDictionary<int, string> map2 = new Dictionary<int, string>();
            map[COLOR_MODE_HDR_UNDEFINED] = "";
            map[COLOR_MODE_HDR_NO] = "lowdr";
            map[COLOR_MODE_HDR_YES] = "highdr";
            _COLOR_MODE_HDR_VALUES = new ReadOnlyDictionary<int, string>(map2);
            IDictionary<int, string> map3 = new Dictionary<int, string>();
            map[DENSITY_DPI_UNDEFINED] = "";
            map[DENSITY_DPI_LDPI] = "ldpi";
            map[DENSITY_DPI_MDPI] = "mdpi";
            map[DENSITY_DPI_TVDPI] = "tvdpi";
            map[DENSITY_DPI_HDPI] = "hdpi";
            map[DENSITY_DPI_XHDPI] = "xhdpi";
            map[DENSITY_DPI_XXHDPI] = "xxhdpi";
            map[DENSITY_DPI_XXXHDPI] = "xxxhdpi";
            map[DENSITY_DPI_ANY] = "anydpi";
            map[DENSITY_DPI_NONE] = "nodpi";
            _DENSITY_DPI_VALUES = new ReadOnlyDictionary<int, string>(map3);
            IDictionary<int, string> map4 = new Dictionary<int, string>();
            map[KEYBOARD_NOKEYS] = "nokeys";
            map[KEYBOARD_QWERTY] = "qwerty";
            map[KEYBOARD_12KEY] = "12key";
            _KEYBOARD_VALUES = new ReadOnlyDictionary<int, string>(map4);
            IDictionary<int, string> map5 = new Dictionary<int, string>();
            map[KEYBOARDHIDDEN_NO] = "keysexposed";
            map[KEYBOARDHIDDEN_YES] = "keyshidden";
            map[KEYBOARDHIDDEN_SOFT] = "keyssoft";
            _KEYBOARDHIDDEN_VALUES = new ReadOnlyDictionary<int, string>(map5);
            IDictionary<int, string> map6 = new Dictionary<int, string>();
            map[NAVIGATION_NONAV] = "nonav";
            map[NAVIGATION_DPAD] = "dpad";
            map[NAVIGATION_TRACKBALL] = "trackball";
            map[NAVIGATION_WHEEL] = "wheel";
            _NAVIGATION_VALUES = new ReadOnlyDictionary<int, string>(map6);
            IDictionary<int, string> map7 = new Dictionary<int, string>();
            map[NAVIGATIONHIDDEN_NO] = "navexposed";
            map[NAVIGATIONHIDDEN_YES] = "navhidden";
            _NAVIGATIONHIDDEN_VALUES = new ReadOnlyDictionary<int, string>(map7);
            IDictionary<int, string> map8 = new Dictionary<int, string>();
            map[ORIENTATION_PORTRAIT] = "port";
            map[ORIENTATION_LANDSCAPE] = "land";
            _ORIENTATION_VALUES = new ReadOnlyDictionary<int, string>(map8);
            IDictionary<int, string> map9 = new Dictionary<int, string>();
            map[SCREENLAYOUT_LAYOUTDIR_LTR] = "ldltr";
            map[SCREENLAYOUT_LAYOUTDIR_RTL] = "ldrtl";
            _SCREENLAYOUT_LAYOUTDIR_VALUES = new ReadOnlyDictionary<int, string>(map9);
            IDictionary<int, string> map10 = new Dictionary<int, string>();
            map[SCREENLAYOUT_LONG_NO] = "notlong";
            map[SCREENLAYOUT_LONG_YES] = "long";
            _SCREENLAYOUT_LONG_VALUES = new ReadOnlyDictionary<int, string>(map10);
            IDictionary<int, string> map11 = new Dictionary<int, string>();
            map[SCREENLAYOUT_ROUND_NO] = "notround";
            map[SCREENLAYOUT_ROUND_YES] = "round";
            _SCREENLAYOUT_ROUND_VALUES = new ReadOnlyDictionary<int, string>(map11);
            IDictionary<int, string> map12 = new Dictionary<int, string>();
            map[SCREENLAYOUT_SIZE_SMALL] = "small";
            map[SCREENLAYOUT_SIZE_NORMAL] = "normal";
            map[SCREENLAYOUT_SIZE_LARGE] = "large";
            map[SCREENLAYOUT_SIZE_XLARGE] = "xlarge";
            _SCREENLAYOUT_SIZE_VALUES = new ReadOnlyDictionary<int, string>(map12);
            IDictionary<int, string> map13 = new Dictionary<int, string>();
            map[TOUCHSCREEN_NOTOUCH] = "notouch";
            map[TOUCHSCREEN_FINGER] = "finger";
            _TOUCHSCREEN_VALUES = new ReadOnlyDictionary<int, string>(map13);
            IDictionary<int, string> map14 = new Dictionary<int, string>();
            map[UI_MODE_NIGHT_NO] = "notnight";
            map[UI_MODE_NIGHT_YES] = "night";
            _UI_MODE_NIGHT_VALUES = new ReadOnlyDictionary<int, string>(map14);
            IDictionary<int, string> map15 = new Dictionary<int, string>();
            map[UI_MODE_TYPE_DESK] = "desk";
            map[UI_MODE_TYPE_CAR] = "car";
            map[UI_MODE_TYPE_TELEVISION] = "television";
            map[UI_MODE_TYPE_APPLIANCE] = "appliance";
            map[UI_MODE_TYPE_WATCH] = "watch";
            map[UI_MODE_TYPE_VR_HEADSET] = "vrheadset";
            _UI_MODE_TYPE_VALUES = new ReadOnlyDictionary<int, string>(map15);
        }

        private int _size;

        private int _mcc;

        private int _mnc;

        private byte[] _language;

        private byte[] _region;

        private int _orientation;

        private int _touchscreen;

        private int _density;

        private int _keyboard;

        private int _navigation;

        private int _inputFlags;

        private int _screenWidth;

        private int _screenHeight;

        private int _sdkVersion;

        private int _minorVersion;

        private int _screenLayout;

        private int _uiMode;

        private int _smallestScreenWidthDp;

        private int _screenWidthDp;

        private int _screenHeightDp;

        private byte[] _localeScript;

        private byte[] _localeVariant;

        private int _screenLayout2;

        private int _colorMode;

        private byte[] _unknown;

        public int size()
        {
            return _size;
        }

        public int mcc()
        {
            return _mcc;
        }

        public int mnc()
        {
            return _mnc;
        }

        public byte[] language()
        {
            return _language;
        }

        public byte[] region()
        {
            return _region;
        }

        public int orientation()
        {
            return _orientation;
        }

        public int touchscreen()
        {
            return _touchscreen;
        }

        public int density()
        {
            return _density;
        }

        public int keyboard()
        {
            return _keyboard;
        }

        public int navigation()
        {
            return _navigation;
        }

        public int inputFlags()
        {
            return _inputFlags;
        }

        public int screenWidth()
        {
            return _screenWidth;
        }

        public int screenHeight()
        {
            return _screenHeight;
        }

        public int sdkVersion()
        {
            return _sdkVersion;
        }

        public int minorVersion()
        {
            return _minorVersion;
        }

        public int screenLayout()
        {
            return _screenLayout;
        }

        public int uiMode()
        {
            return _uiMode;
        }

        public int smallestScreenWidthDp()
        {
            return _smallestScreenWidthDp;
        }

        public int screenWidthDp()
        {
            return _screenWidthDp;
        }

        public int screenHeightDp()
        {
            return _screenHeightDp;
        }

        public byte[] localeScript()
        {
            return _localeScript;
        }

        public byte[] localeVariant()
        {
            return _localeVariant;
        }

        public int screenLayout2()
        {
            return _screenLayout2;
        }

        public int colorMode()
        {
            return _colorMode;
        }

        public byte[] unknown()
        {
            return _unknown;
        }

        /// <summary>
        /// Returns a <seealso cref="Builder"/> with sane default properties.
        /// </summary>
        public static ResourceConfiguration.Builder builder()
        {
            return new ResourceConfiguration.Builder()
                .size(SIZE)
                .mcc(0)
                .mnc(0)
                .language(new byte[2])
                .region(new byte[2])
                .orientation(0)
                .touchscreen(0)
                .density(0)
                .keyboard(0)
                .navigation(0)
                .inputFlags(0)
                .screenWidth(0)
                .screenHeight(0)
                .sdkVersion(0)
                .minorVersion(0)
                .screenLayout(0)
                .uiMode(0)
                .smallestScreenWidthDp(0)
                .screenWidthDp(0)
                .screenHeightDp(0)
                .localeScript(new byte[4])
                .localeVariant(new byte[8])
                .screenLayout2(0)
                .colorMode(0)
                .unknown(new byte[0]);
        }

        internal static ResourceConfiguration create(ByteBuffer buffer)
        {
            int startPosition = buffer.position(); // The starting buffer position to calculate bytes read.
            int size = buffer.getInt();
            Preconditions.checkArgument(size >= MIN_SIZE, $"Expected minimum ResourceConfiguration size of {MIN_SIZE}, got {size}");
            // Builder order is important here. It's the same order as the data stored in the buffer.
            // The order of the builder's method calls, such as #mcc and #mnc, should not be changed.
            Builder configurationBuilder = builder().size(size).mcc(buffer.getShort() & 0xFFFF).mnc(buffer.getShort() & 0xFFFF);
            byte[] language = new byte[2];
            buffer.get(language);
            byte[] region = new byte[2];
            buffer.get(region);
            configurationBuilder.language(language) //
                .region(region) //
                .orientation( /*Byte.toUnsignedInt*/(buffer.get())) //
                .touchscreen( /*Byte.toUnsignedInt*/(buffer.get())) //
                .density(buffer.getShort() & 0xFFFF) //
                .keyboard( /*Byte.toUnsignedInt*/(buffer.get())) //
                .navigation( /*Byte.toUnsignedInt*/(buffer.get())) //
                .inputFlags( /*Byte.toUnsignedInt*/(buffer.get()));
            buffer.get(); // 1 byte of padding
            configurationBuilder.screenWidth(buffer.getShort() & 0xFFFF) //
                .screenHeight(buffer.getShort() & 0xFFFF) //
                .sdkVersion(buffer.getShort() & 0xFFFF) //
                .minorVersion(buffer.getShort() & 0xFFFF); //

            // At this point, the configuration's size needs to be taken into account as not all
            // configurations have all values.
            if (size >= SCREEN_CONFIG_MIN_SIZE)
            {
                configurationBuilder
                    .screenLayout( /*Byte.toUnsignedInt*/(buffer.get()))
                    .uiMode( /*Byte.toUnsignedInt*/(buffer.get()))
                    .smallestScreenWidthDp(buffer.getShort() & 0xFFFF);
            }

            if (size >= SCREEN_DP_MIN_SIZE)
            {
                configurationBuilder.screenWidthDp(buffer.getShort() & 0xFFFF).screenHeightDp(buffer.getShort() & 0xFFFF);
            }

            if (size >= LOCALE_MIN_SIZE)
            {
                byte[] localeScript = new byte[4];
                buffer.get(localeScript);
                byte[] localeVariant = new byte[8];
                buffer.get(localeVariant);
                configurationBuilder.localeScript(localeScript).localeVariant(localeVariant);
            }

            if (size >= SCREEN_CONFIG_EXTENSION_MIN_SIZE)
            {
                configurationBuilder.screenLayout2( /*Byte.toUnsignedInt*/(buffer.get()));
                configurationBuilder.colorMode( /*Byte.toUnsignedInt*/(buffer.get()));
                buffer.getShort(); // More reserved padding
            }

            // After parsing everything that's known, account for anything that's unknown.
            int bytesRead = buffer.position() - startPosition;
            byte[] unknown = new byte[size - bytesRead];
            buffer.get(unknown);
            configurationBuilder.unknown(unknown);

            return configurationBuilder.build();
        }

        public class Builder
        {
            internal readonly ResourceConfiguration res;

            public Builder()
            {
                res = new ResourceConfiguration();
            }


            public Builder size(int size)
            {
                res._size = size;
                return this;
            }


            public Builder mcc(int mcc)
            {
                res._mcc = mcc;
                return this;
            }


            public Builder mnc(int mnc)
            {
                res._mnc = mnc;
                return this;
            }


            public Builder language(byte[] language)
            {
                res._language = language;
                return this;
            }


            public Builder region(byte[] region)
            {
                res._region = region;
                return this;
            }


            public Builder orientation(int orientation)
            {
                res._orientation = orientation;
                return this;
            }


            public Builder touchscreen(int touchscreen)
            {
                res._touchscreen = touchscreen;
                return this;
            }


            public Builder density(int density)
            {
                res._density = density;
                return this;
            }


            public Builder keyboard(int keyboard)
            {
                res._keyboard = keyboard;
                return this;
            }


            public Builder navigation(int navigation)
            {
                res._navigation = navigation;
                return this;
            }


            public Builder inputFlags(int inputFlags)
            {
                res._inputFlags = inputFlags;
                return this;
            }


            public Builder screenWidth(int screenWidth)
            {
                res._screenWidth = screenWidth;
                return this;
            }


            public Builder screenHeight(int screenHeight)
            {
                res._screenHeight = screenHeight;
                return this;
            }


            public Builder sdkVersion(int sdkVersion)
            {
                res._sdkVersion = sdkVersion;
                return this;
            }


            public Builder minorVersion(int minorVersion)
            {
                res._minorVersion = minorVersion;
                return this;
            }


            public Builder screenLayout(int screenLayout)
            {
                res._screenLayout = screenLayout;
                return this;
            }


            public Builder uiMode(int uiMode)
            {
                res._uiMode = uiMode;
                return this;
            }


            public Builder smallestScreenWidthDp(int smallestScreenWidthDp)
            {
                res._smallestScreenWidthDp = smallestScreenWidthDp;
                return this;
            }


            public Builder screenWidthDp(int screenWidthDp)
            {
                res._screenWidthDp = screenWidthDp;
                return this;
            }


            public Builder screenHeightDp(int screenHeightDp)
            {
                res._screenHeightDp = screenHeightDp;
                return this;
            }


            public Builder localeScript(byte[] localeScript)
            {
                res._localeScript = localeScript;
                return this;
            }


            public Builder localeVariant(byte[] localeVariant)
            {
                res._localeVariant = localeVariant;
                return this;
            }


            public Builder screenLayout2(int screenLayout2)
            {
                res._screenLayout2 = screenLayout2;
                return this;
            }


            public Builder colorMode(int colorMode)
            {
                res._colorMode = colorMode;
                return this;
            }


            public Builder unknown(byte[] unknown)
            {
                res._unknown = unknown;
                return this;
            }

            public ResourceConfiguration build()
            {
                return res;
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}