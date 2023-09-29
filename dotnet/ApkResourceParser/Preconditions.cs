/*
 *	apk resource parser for .net
 *  port of https://github.com/google/android-arscblamer
 *
 *  ikas-mc 2023
 */

namespace ApkResourceParser
{
    public static class Preconditions
    {
        public static void checkArgument(bool expression, object errorMessage)
        {
            if (!expression)
            {
                var message = errorMessage switch
                {
                    null => "",
                    string s => s,
                    _ => errorMessage.ToString()
                };
                throw new ArgumentException(message);
            }
        }

        public static void checkState(bool expression)
        {
            if (!expression)
            {
                throw new ArgumentException();
            }
        }

        public static void checkState(bool expression, object errorMessage)
        {
            if (!expression)
            {
                var message = errorMessage switch
                {
                    null => "expression is false",
                    string s => s,
                    _ => errorMessage.ToString()
                };
                throw new ArgumentException(message);
            }
        }

        public static T checkNotNull<T>(T reference)
        {
            if (reference == null)
            {
                throw new ArgumentException();
            }

            return reference;
        }

        public static T checkNotNull<T>(T reference, object errorMessage)
        {
            if (reference == null)
            {
                var message = errorMessage switch
                {
                    null => "value is null",
                    string s => s,
                    _ => errorMessage.ToString()
                };
                throw new ArgumentException(message);
            }

            return reference;
        }
    }
}