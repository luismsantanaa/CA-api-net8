using Shared.Exceptions;
using Shared.Extensions.Contracts;


namespace Shared.Extensions
{
    public static class ExeptionsExtensions
    {
        public static void IfClassNull<T>(this IThrowException validatR, T value) where T : class
        {
            if (value == null)
            {
                throw new NotFoundException(typeof(T).Name);
            }
        }

        public static void IfObjectClassNull<T>(this IThrowException validatR, T value, object key) where T : class
        {
            if (value == null)
            {
                throw new NotFoundException(typeof(T).Name, key);
            }
        }

        public static void IfNull<T>(this IThrowException validatR, T value, string propertyName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(propertyName);
            }
        }

        public static void IfNull<T>(this IThrowException validatR, T value, string propertyName, string message)
        {
            if (value == null)
            {
                throw new ArgumentNullException($"{propertyName} is NULL. {message}");
            }
        }

        public static void IfNotNull<T>(this IThrowException validatR, T value, string message)
        {
            if (value != null)
            {
                throw new ArgumentException(message);
            }
        }

        public static void IfNullOrWhiteSpace(this IThrowException validatR, string value, string propertyName)
        {
            ThrowException.Exception.IfNull(value, propertyName);
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"Paramater {propertyName} cannot be empty.");
            }
        }

        public static void IfNotEqual<T>(this IThrowException validatR, int valueOne, int valueTwo, string property)
        {
            if (valueOne != valueTwo)
            {
                throw new ArgumentException($"Supplied {property} Values are not equal.");
            }
        }

        public static void IfFalse(this IThrowException validatR, bool value, string message)
        {
            if (!value)
            {
                throw new ArgumentException(message);
            }
        }

        public static void IfTrue(this IThrowException validatR, bool value, string message)
        {
            if (value)
            {
                throw new ArgumentException(message);
            }
        }

        public static void IfZero(this IThrowException validatR, int value, string property)
        {
            if (value == 0)
            {
                throw new ArgumentException($"This Property {property} Cannot be Zero");
            }
        }

        public static void IfFileToLarge(this IThrowException validateR, long max, long current, string fileName)
        {
            if (current > max)
            {
                throw new ArgumentOutOfRangeException($"El Archivo [{fileName}] excede los {max}MB permitidos para Almacenar!");
            }
        }

        public static void IfFileHasBadExtension(this IThrowException validateR, List<string> extValid, string current, string fileName)
        {
            if (!extValid.Contains(current))
            {
                throw new FormatException($"El Archivo [{fileName}] no tiene una extensión de archivo permitida para almacenar!!");
            }
        }
    }
}
