namespace api.Utils
{
    public static class DateTimeUtils
    {
        public static string ToRelativeTimeString(this DateTime dateTime)
        {
            var timeSpan = new TimeSpan(DateTime.Now.Ticks - dateTime.Ticks);
            var delta = Math.Abs(timeSpan.TotalSeconds);
            if (delta < 60) return "Vừa xong";
            if (delta < 120) return "1 phút trước";
            if (delta < 3600) return $"{timeSpan.Minutes} phút trước";
            if (delta < 7200) return "1 giờ trước";
            if (delta < 86400) return $"{timeSpan.Hours} giờ trước";
            if (delta < 172800) return "Hôm qua";
            if (delta < 2592000) return $"{timeSpan.Days} ngày trước";
            if (delta < 31104000)
            {
                int months = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 30));
                return months <= 1 ? "1 tháng trước" : $"{months} tháng trước";
            }
            int years = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 365));
            return years <= 1 ? "1 năm trước" : $"{years} năm trước";
        }

        public static DateTime FormatDateTime(this DateTime dateTime)
        {
            return DateTime.Parse(dateTime.ToString("yyyy-MM-ddTHH:mm:ss"));
        }
    }
}