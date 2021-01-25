using System;

namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int IzracunajDob(this DateTime datumRodjenja)
        {
            var today = DateTime.Today;
            var age = today.Year - datumRodjenja.Year;
            if (datumRodjenja.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}