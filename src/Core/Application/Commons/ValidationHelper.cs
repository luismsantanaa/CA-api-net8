using System.Text.RegularExpressions;

namespace Application.Commons
{
    internal static class ValidationHelper
    {
        private const string EmailRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        private static readonly List<char> GenderList = new List<char> { 'F', 'M' };

        public static bool IsGuid(this string? value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            return Guid.TryParse(value, out _);
        }
        public static bool IsEmail(this string? value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            var regex = new Regex(EmailRegex);
            return regex.IsMatch(value);
        }
        public static bool ValidateGender(this char? sex)
        {
            return GenderList.Contains((char)sex!);
        }
        public static bool BeAValidDate(this DateTime? date)
        {
            return !date.Equals(default(DateTime));
        }
    }
}
