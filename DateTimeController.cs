using System;
using System.Globalization;
using System.Net;

namespace EncrypterEngine_JDSA
{
    public static class DateTimeController
    {
        private const string GoogleUrl = "https://www.google.com";
        private const int TimeoutMilliseconds = 2000;

        public static string GetFormattedNetworkDate()
        {
            DateTime dateToUse = GetGoogleDateOrLocalFallback();
            return dateToUse.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static DateTime GetGoogleDateOrLocalFallback()
        {
            DateTime? googleTime = TryFetchGoogleTime();
            return googleTime ?? DateTime.Now;
        }

        private static DateTime? TryFetchGoogleTime()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(GoogleUrl);
                request.Method = "HEAD";
                request.Timeout = TimeoutMilliseconds;
                request.ReadWriteTimeout = TimeoutMilliseconds;
                request.UserAgent = "Mozilla/5.0";

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    string dateHeader = response.Headers["Date"];
                    if (!string.IsNullOrWhiteSpace(dateHeader) &&
                        DateTimeOffset.TryParse(
                            dateHeader,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out DateTimeOffset utcDate))
                    {
                        return utcDate.ToLocalTime().DateTime;
                    }
                }
            }
            catch
            {
                // Sin conexión, timeout o bloqueo de red: cae al reloj del sistema operativo.
            }

            return null;
        }
    }
}