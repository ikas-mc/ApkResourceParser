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
import java.nio.charset.StandardCharsets;
import java.util.Arrays;
import java.util.Collections;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.Map;

/**
 * Describes a particular resource configuration.
 */
public class ResourceConfiguration {

    public enum Type {
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


    /**
     * The below constants are from android.content.res.Configuration.
     */
    static final int COLOR_MODE_WIDE_COLOR_GAMUT_MASK = 0x03;
    static final int COLOR_MODE_WIDE_COLOR_GAMUT_UNDEFINED = 0;
    static final int COLOR_MODE_WIDE_COLOR_GAMUT_NO = 0x01;
    static final int COLOR_MODE_WIDE_COLOR_GAMUT_YES = 0x02;
    static final int COLOR_MODE_HDR_MASK = 0x0C;
    static final int COLOR_MODE_HDR_UNDEFINED = 0;
    static final int COLOR_MODE_HDR_NO = 0x04;
    static final int COLOR_MODE_HDR_YES = 0x08;
    static final int DENSITY_DPI_UNDEFINED = 0;
    static final int DENSITY_DPI_LDPI = 120;
    static final int DENSITY_DPI_MDPI = 160;
    static final int DENSITY_DPI_TVDPI = 213;
    static final int DENSITY_DPI_HDPI = 240;
    static final int DENSITY_DPI_XHDPI = 320;
    static final int DENSITY_DPI_XXHDPI = 480;
    static final int DENSITY_DPI_XXXHDPI = 640;
    static final int DENSITY_DPI_ANY = 0xFFFE;
    static final int DENSITY_DPI_NONE = 0xFFFF;
    static final int KEYBOARD_NOKEYS = 1;
    static final int KEYBOARD_QWERTY = 2;
    static final int KEYBOARD_12KEY = 3;
    static final int KEYBOARDHIDDEN_MASK = 0x03;
    static final int KEYBOARDHIDDEN_NO = 1;
    static final int KEYBOARDHIDDEN_YES = 2;
    static final int KEYBOARDHIDDEN_SOFT = 3;
    static final int NAVIGATION_NONAV = 1;
    static final int NAVIGATION_DPAD = 2;
    static final int NAVIGATION_TRACKBALL = 3;
    static final int NAVIGATION_WHEEL = 4;
    static final int NAVIGATIONHIDDEN_MASK = 0x0C;
    static final int NAVIGATIONHIDDEN_NO = 0x04;
    static final int NAVIGATIONHIDDEN_YES = 0x08;
    static final int ORIENTATION_PORTRAIT = 0x01;
    static final int ORIENTATION_LANDSCAPE = 0x02;
    static final int SCREENLAYOUT_LAYOUTDIR_MASK = 0xC0;
    static final int SCREENLAYOUT_LAYOUTDIR_LTR = 0x40;
    static final int SCREENLAYOUT_LAYOUTDIR_RTL = 0x80;
    static final int SCREENLAYOUT_LONG_MASK = 0x30;
    static final int SCREENLAYOUT_LONG_NO = 0x10;
    static final int SCREENLAYOUT_LONG_YES = 0x20;
    static final int SCREENLAYOUT_ROUND_MASK = 0x03;
    static final int SCREENLAYOUT_ROUND_NO = 0x01;
    static final int SCREENLAYOUT_ROUND_YES = 0x02;
    static final int SCREENLAYOUT_SIZE_MASK = 0x0F;
    static final int SCREENLAYOUT_SIZE_SMALL = 0x01;
    static final int SCREENLAYOUT_SIZE_NORMAL = 0x02;
    static final int SCREENLAYOUT_SIZE_LARGE = 0x03;
    static final int SCREENLAYOUT_SIZE_XLARGE = 0x04;
    static final int TOUCHSCREEN_NOTOUCH = 1;
    static final int TOUCHSCREEN_FINGER = 3;
    static final int UI_MODE_NIGHT_MASK = 0x30;
    static final int UI_MODE_NIGHT_NO = 0x10;
    static final int UI_MODE_NIGHT_YES = 0x20;
    static final int UI_MODE_TYPE_MASK = 0x0F;
    static final int UI_MODE_TYPE_DESK = 0x02;
    static final int UI_MODE_TYPE_CAR = 0x03;
    static final int UI_MODE_TYPE_TELEVISION = 0x04;
    static final int UI_MODE_TYPE_APPLIANCE = 0x05;
    static final int UI_MODE_TYPE_WATCH = 0x06;
    static final int UI_MODE_TYPE_VR_HEADSET = 0x07;
    private static final Map<Integer, String> COLOR_MODE_WIDE_COLOR_GAMUT_VALUES;
    private static final Map<Integer, String> COLOR_MODE_HDR_VALUES;
    private static final Map<Integer, String> DENSITY_DPI_VALUES;
    private static final Map<Integer, String> KEYBOARD_VALUES;
    private static final Map<Integer, String> KEYBOARDHIDDEN_VALUES;
    private static final Map<Integer, String> NAVIGATION_VALUES;
    private static final Map<Integer, String> NAVIGATIONHIDDEN_VALUES;
    private static final Map<Integer, String> ORIENTATION_VALUES;
    private static final Map<Integer, String> SCREENLAYOUT_LAYOUTDIR_VALUES;
    private static final Map<Integer, String> SCREENLAYOUT_LONG_VALUES;
    private static final Map<Integer, String> SCREENLAYOUT_ROUND_VALUES;
    private static final Map<Integer, String> SCREENLAYOUT_SIZE_VALUES;
    private static final Map<Integer, String> TOUCHSCREEN_VALUES;
    private static final Map<Integer, String> UI_MODE_NIGHT_VALUES;
    private static final Map<Integer, String> UI_MODE_TYPE_VALUES;
    /**
     * The minimum size in bytes that a {@link ikas.project.java.apk.arsc.ResourceConfiguration} can be.
     */
    private static final int MIN_SIZE = 28;
    /**
     * The minimum size in bytes that this configuration must be to contain screen config info.
     */
    private static final int SCREEN_CONFIG_MIN_SIZE = 32;
    /**
     * The minimum size in bytes that this configuration must be to contain screen dp info.
     */
    private static final int SCREEN_DP_MIN_SIZE = 36;
    /**
     * The minimum size in bytes that this configuration must be to contain locale info.
     */
    private static final int LOCALE_MIN_SIZE = 48;
    /**
     * The minimum size in bytes that this config must be to contain the screenConfig extension.
     */
    private static final int SCREEN_CONFIG_EXTENSION_MIN_SIZE = 52;
    /**
     * The size of resource configurations in bytes for the latest version of Android resources.
     */
    public static final int SIZE = SCREEN_CONFIG_EXTENSION_MIN_SIZE;
    private static final Builder DEFAULT_BUILDER = builder();
    /**
     * The default configuration. This configuration acts as a "catch-all" for looking up resources
     * when no better configuration can be found.
     */
    public static final ResourceConfiguration DEFAULT = DEFAULT_BUILDER.build();

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(COLOR_MODE_WIDE_COLOR_GAMUT_UNDEFINED, "");
        map.put(COLOR_MODE_WIDE_COLOR_GAMUT_NO, "nowidecg");
        map.put(COLOR_MODE_WIDE_COLOR_GAMUT_YES, "widecg");
        COLOR_MODE_WIDE_COLOR_GAMUT_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(COLOR_MODE_HDR_UNDEFINED, "");
        map.put(COLOR_MODE_HDR_NO, "lowdr");
        map.put(COLOR_MODE_HDR_YES, "highdr");
        COLOR_MODE_HDR_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(DENSITY_DPI_UNDEFINED, "");
        map.put(DENSITY_DPI_LDPI, "ldpi");
        map.put(DENSITY_DPI_MDPI, "mdpi");
        map.put(DENSITY_DPI_TVDPI, "tvdpi");
        map.put(DENSITY_DPI_HDPI, "hdpi");
        map.put(DENSITY_DPI_XHDPI, "xhdpi");
        map.put(DENSITY_DPI_XXHDPI, "xxhdpi");
        map.put(DENSITY_DPI_XXXHDPI, "xxxhdpi");
        map.put(DENSITY_DPI_ANY, "anydpi");
        map.put(DENSITY_DPI_NONE, "nodpi");
        DENSITY_DPI_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(KEYBOARD_NOKEYS, "nokeys");
        map.put(KEYBOARD_QWERTY, "qwerty");
        map.put(KEYBOARD_12KEY, "12key");
        KEYBOARD_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(KEYBOARDHIDDEN_NO, "keysexposed");
        map.put(KEYBOARDHIDDEN_YES, "keyshidden");
        map.put(KEYBOARDHIDDEN_SOFT, "keyssoft");
        KEYBOARDHIDDEN_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(NAVIGATION_NONAV, "nonav");
        map.put(NAVIGATION_DPAD, "dpad");
        map.put(NAVIGATION_TRACKBALL, "trackball");
        map.put(NAVIGATION_WHEEL, "wheel");
        NAVIGATION_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(NAVIGATIONHIDDEN_NO, "navexposed");
        map.put(NAVIGATIONHIDDEN_YES, "navhidden");
        NAVIGATIONHIDDEN_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(ORIENTATION_PORTRAIT, "port");
        map.put(ORIENTATION_LANDSCAPE, "land");
        ORIENTATION_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(SCREENLAYOUT_LAYOUTDIR_LTR, "ldltr");
        map.put(SCREENLAYOUT_LAYOUTDIR_RTL, "ldrtl");
        SCREENLAYOUT_LAYOUTDIR_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(SCREENLAYOUT_LONG_NO, "notlong");
        map.put(SCREENLAYOUT_LONG_YES, "long");
        SCREENLAYOUT_LONG_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(SCREENLAYOUT_ROUND_NO, "notround");
        map.put(SCREENLAYOUT_ROUND_YES, "round");
        SCREENLAYOUT_ROUND_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(SCREENLAYOUT_SIZE_SMALL, "small");
        map.put(SCREENLAYOUT_SIZE_NORMAL, "normal");
        map.put(SCREENLAYOUT_SIZE_LARGE, "large");
        map.put(SCREENLAYOUT_SIZE_XLARGE, "xlarge");
        SCREENLAYOUT_SIZE_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(TOUCHSCREEN_NOTOUCH, "notouch");
        map.put(TOUCHSCREEN_FINGER, "finger");
        TOUCHSCREEN_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(UI_MODE_NIGHT_NO, "notnight");
        map.put(UI_MODE_NIGHT_YES, "night");
        UI_MODE_NIGHT_VALUES = Collections.unmodifiableMap(map);
    }

    static {
        Map<Integer, String> map = new HashMap<>();
        map.put(UI_MODE_TYPE_DESK, "desk");
        map.put(UI_MODE_TYPE_CAR, "car");
        map.put(UI_MODE_TYPE_TELEVISION, "television");
        map.put(UI_MODE_TYPE_APPLIANCE, "appliance");
        map.put(UI_MODE_TYPE_WATCH, "watch");
        map.put(UI_MODE_TYPE_VR_HEADSET, "vrheadset");
        UI_MODE_TYPE_VALUES = Collections.unmodifiableMap(map);
    }

    public int size;
    public int mcc;
    public int mnc;
    public byte[] language;
    public byte[] region;
    public int orientation;
    public int touchscreen;
    public int density;
    public int keyboard;
    public int navigation;
    public int inputFlags;
    public int screenWidth;
    public int screenHeight;
    public int sdkVersion;
    public int minorVersion;
    public int screenLayout;
    public int uiMode;
    public int smallestScreenWidthDp;
    public int screenWidthDp;
    public int screenHeightDp;
    public byte[] localeScript;
    public byte[] localeVariant;
    public int screenLayout2;
    public int colorMode;
    public byte[] unknown;

    /**
     * Returns a {@link ikas.project.java.apk.arsc.ResourceConfiguration.Builder} with sane default properties.
     */
    public static Builder builder() {
        return new Builder()
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

    static ResourceConfiguration create(ByteBuffer buffer) {
        int startPosition = buffer.position();  // The starting buffer position to calculate bytes read.
        int size = buffer.getInt();
        Preconditions.checkArgument(size >= MIN_SIZE,
                STR. "Expected minimum ResourceConfiguration size of \{ MIN_SIZE }, got \{ size }" );
        // Builder order is important here. It's the same order as the data stored in the buffer.
        // The order of the builder's method calls, such as #mcc and #mnc, should not be changed.
        Builder configurationBuilder = builder()
                .size(size)
                .mcc(buffer.getShort() & 0xFFFF)
                .mnc(buffer.getShort() & 0xFFFF);
        byte[] language = new byte[2];
        buffer.get(language);
        byte[] region = new byte[2];
        buffer.get(region);
        configurationBuilder.language(language)
                .region(region)
                .orientation(Byte.toUnsignedInt(buffer.get()))
                .touchscreen(Byte.toUnsignedInt(buffer.get()))
                .density(buffer.getShort() & 0xFFFF)
                .keyboard(Byte.toUnsignedInt(buffer.get()))
                .navigation(Byte.toUnsignedInt(buffer.get()))
                .inputFlags(Byte.toUnsignedInt(buffer.get()));
        buffer.get();  // 1 byte of padding
        configurationBuilder.screenWidth(buffer.getShort() & 0xFFFF)
                .screenHeight(buffer.getShort() & 0xFFFF)
                .sdkVersion(buffer.getShort() & 0xFFFF)
                .minorVersion(buffer.getShort() & 0xFFFF);

        // At this point, the configuration's size needs to be taken into account as not all
        // configurations have all values.
        if (size >= SCREEN_CONFIG_MIN_SIZE) {
            configurationBuilder.screenLayout(Byte.toUnsignedInt(buffer.get()))
                    .uiMode(Byte.toUnsignedInt(buffer.get()))
                    .smallestScreenWidthDp(buffer.getShort() & 0xFFFF);
        }

        if (size >= SCREEN_DP_MIN_SIZE) {
            configurationBuilder.screenWidthDp(buffer.getShort() & 0xFFFF)
                    .screenHeightDp(buffer.getShort() & 0xFFFF);
        }

        if (size >= LOCALE_MIN_SIZE) {
            byte[] localeScript = new byte[4];
            buffer.get(localeScript);
            byte[] localeVariant = new byte[8];
            buffer.get(localeVariant);
            configurationBuilder.localeScript(localeScript)
                    .localeVariant(localeVariant);
        }

        if (size >= SCREEN_CONFIG_EXTENSION_MIN_SIZE) {
            configurationBuilder.screenLayout2(Byte.toUnsignedInt(buffer.get()));
            configurationBuilder.colorMode(Byte.toUnsignedInt(buffer.get()));
            buffer.getShort();  // More reserved padding
        }

        // After parsing everything that's known, account for anything that's unknown.
        int bytesRead = buffer.position() - startPosition;
        byte[] unknown = new byte[size - bytesRead];
        buffer.get(unknown);
        configurationBuilder.unknown(unknown);

        return configurationBuilder.build();
    }

    public static String unpackLanguage(byte[] language) {
        return unpackLanguageOrRegion(language, 0x61);
    }

    private static String unpackLanguageOrRegion(byte[] value, int base) {
        Preconditions.checkState(value.length == 2, "Language or region value must be 2 bytes.");
        if (value[0] == 0 && value[1] == 0) {
            return "";
        }

        if ((Byte.toUnsignedInt(value[0]) & 0x80) != 0) {
            byte[] result = new byte[3];
            result[0] = (byte) (base + (value[1] & 0x1F));
            result[1] = (byte) (base + ((value[1] & 0xE0) >>> 5) + ((value[0] & 0x03) << 3));
            result[2] = (byte) (base + ((value[0] & 0x7C) >>> 2));
            return new String(result, StandardCharsets.US_ASCII);
        }
        return new String(value, StandardCharsets.US_ASCII);
    }

    /**
     * Packs a 2 or 3 character language string into two bytes. If this is a 2 character string the
     * returned bytes is simply the string bytes, if this is a 3 character string we use a packed
     * format where the two bytes are:
     *
     * <pre>
     *  +--+--+--+--+--+--+--+--+  +--+--+--+--+--+--+--+--+
     *  |B |2 |2 |2 |2 |2 |1 |1 |  |1 |1 |1 |0 |0 |0 |0 |0 |
     *  +--+--+--+--+--+--+--+--+  +--+--+--+--+--+--+--+--+
     * </pre>
     *
     * <p>B : if bit set indicates this is a 3 character string (languages are always old style 7 bit
     * ascii chars only, so this is never set for a two character language)
     *
     * <p>2: The third character - 0x61
     *
     * <p>1: The second character - 0x61
     *
     * <p>0: The first character - 0x61
     *
     * <p>Languages are always lower case chars, so max is within 5 bits (z = 11001)
     *
     * @param language The language to pack.
     * @return The two byte representation of the language
     */
    public static byte[] packLanguage(String language) {
        byte[] unpacked = language.getBytes(StandardCharsets.US_ASCII);
        if (unpacked.length == 2) {
            return unpacked;
        }
        int base = 0x61;
        byte[] result = new byte[2];
        Preconditions.checkState(unpacked.length == 3);
        for (byte value : unpacked) {
            Preconditions.checkState(value >= 'a' && value <= 'z');
        }
        result[0] = (byte) (((unpacked[2] - base) << 2) | ((unpacked[1] - base) >> 3) | 0x80);
        result[1] = (byte) ((unpacked[0] - base) | ((unpacked[1] - base) << 5));
        return result;
    }

    /**
     * Returns true if this is the default "any" configuration.
     */
    public final boolean isDefault() {
        // Ignore size and unknown when checking if this is the default configuration. It's possible
        // that we're comparing against a different version.
        return DEFAULT_BUILDER.size(size()).unknown(unknown()).build().equals(this)
               && Arrays.equals(unknown(), new byte[unknown().length]);
    }

    public final String languageString() {
        return unpackLanguage();
    }

    private String unpackLanguage() {
        return unpackLanguage(language());
    }

    /** Returns {@link #region} as an unpacked string representation. */
    public String regionString()
    {
        return unpackRegion();
    }

    private String unpackRegion()
    {
        return unpackLanguageOrRegion(region(), 0x30);
    }

    public int size() {
        return size;
    }

    public int mcc() {
        return mcc;
    }

    public int mnc() {
        return mnc;
    }

    public byte[] language() {
        return language;
    }

    public byte[] region() {
        return region;
    }

    public int orientation() {
        return orientation;
    }

    public int touchscreen() {
        return touchscreen;
    }

    public int density() {
        return density;
    }

    public int keyboard() {
        return keyboard;
    }

    public int navigation() {
        return navigation;
    }

    public int inputFlags() {
        return inputFlags;
    }

    public int screenWidth() {
        return screenWidth;
    }

    public int screenHeight() {
        return screenHeight;
    }

    public int sdkVersion() {
        return sdkVersion;
    }

    public int minorVersion() {
        return minorVersion;
    }

    public int screenLayout() {
        return screenLayout;
    }

    public int uiMode() {
        return uiMode;
    }

    public int smallestScreenWidthDp() {
        return smallestScreenWidthDp;
    }

    public int screenWidthDp() {
        return screenWidthDp;
    }

    public int screenHeightDp() {
        return screenHeightDp;
    }

    public byte[] localeScript() {
        return localeScript;
    }

    public byte[] localeVariant() {
        return localeVariant;
    }

    public int screenLayout2() {
        return screenLayout2;
    }

    public int colorMode() {
        return colorMode;
    }

    public byte[] unknown() {
        return unknown;
    }


    public static class Builder {
        private final ResourceConfiguration res;

        public Builder() {
            res = new ResourceConfiguration();
        }


        public Builder size(int size) {
            res.size = size;
            return this;
        }


        public Builder mcc(int mcc) {
            res.mcc = mcc;
            return this;
        }


        public Builder mnc(int mnc) {
            res.mnc = mnc;
            return this;
        }


        public Builder language(byte[] language) {
            res.language = language;
            return this;
        }


        public Builder region(byte[] region) {
            res.region = region;
            return this;
        }


        public Builder orientation(int orientation) {
            res.orientation = orientation;
            return this;
        }


        public Builder touchscreen(int touchscreen) {
            res.touchscreen = touchscreen;
            return this;
        }


        public Builder density(int density) {
            res.density = density;
            return this;
        }


        public Builder keyboard(int keyboard) {
            res.keyboard = keyboard;
            return this;
        }


        public Builder navigation(int navigation) {
            res.navigation = navigation;
            return this;
        }


        public Builder inputFlags(int inputFlags) {
            res.inputFlags = inputFlags;
            return this;
        }


        public Builder screenWidth(int screenWidth) {
            res.screenWidth = screenWidth;
            return this;
        }


        public Builder screenHeight(int screenHeight) {
            res.screenHeight = screenHeight;
            return this;
        }


        public Builder sdkVersion(int sdkVersion) {
            res.sdkVersion = sdkVersion;
            return this;
        }


        public Builder minorVersion(int minorVersion) {
            res.minorVersion = minorVersion;
            return this;
        }


        public Builder screenLayout(int screenLayout) {
            res.screenLayout = screenLayout;
            return this;
        }


        public Builder uiMode(int uiMode) {
            res.uiMode = uiMode;
            return this;
        }


        public Builder smallestScreenWidthDp(int smallestScreenWidthDp) {
            res.smallestScreenWidthDp = smallestScreenWidthDp;
            return this;
        }


        public Builder screenWidthDp(int screenWidthDp) {
            res.screenWidthDp = screenWidthDp;
            return this;
        }


        public Builder screenHeightDp(int screenHeightDp) {
            res.screenHeightDp = screenHeightDp;
            return this;
        }


        public Builder localeScript(byte[] localeScript) {
            res.localeScript = localeScript;
            return this;
        }


        public Builder localeVariant(byte[] localeVariant) {
            res.localeVariant = localeVariant;
            return this;
        }


        public Builder screenLayout2(int screenLayout2) {
            res.screenLayout2 = screenLayout2;
            return this;
        }


        public Builder colorMode(int colorMode) {
            res.colorMode = colorMode;
            return this;
        }


        public Builder unknown(byte[] unknown) {
            res.unknown = unknown;
            return this;
        }

        public ResourceConfiguration build() {
            return res;
        }
    }
}
