/*
 *	apk resource parser for .net
 *  port of https://github.com/google/android-arscblamer
 *
 *  ikas-mc 2023
 */

using System;

namespace ApkResourceParser
{
    public static class Preconditions
    {
        public static void checkArgument(bool expression, object errorMessage)
        {
            if (!expression)
            {
                string message;
                switch (errorMessage)
                {
                    case null:
                        message = "";
                        break;
                    case string s:
                        message = s;
                        break;
                    default:
                        message = errorMessage.ToString();
                        break;
                }
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
                string message;
                switch (errorMessage)
                {
                    case null:
                        message = "expression is false";
                        break;
                    case string s:
                        message = s;
                        break;
                    default:
                        message = errorMessage.ToString();
                        break;
                }
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
                string message;
                switch (errorMessage)
                {
                    case null:
                        message = "value is null";
                        break;
                    case string s:
                        message = s;
                        break;
                    default:
                        message = errorMessage.ToString();
                        break;
                }
                throw new ArgumentException(message);
            }

            return reference;
        }
    }
}