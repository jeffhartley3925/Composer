using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;
using System.Windows.Browser;

namespace Composer.Infrastructure.Constants
{
    public class CookieManager
    {
        public static void SetCookie(string key, string val, TimeSpan? expires)
        {
            SetCookie(key, val, expires, null, null, false);
        }
        public static void SetCookie(string key, string val, TimeSpan? expires, string path, string domain, bool secure)
        {
            StringBuilder fullCookie = new StringBuilder(); fullCookie.Append(string.Concat(key, "=", val));
            if (expires.HasValue)
            {
                DateTime expire = DateTime.UtcNow + expires.Value; 
                fullCookie.Append(string.Concat(";expires=", expire.ToString("R")));
            }
            if (path != null)
            {
                fullCookie.Append(string.Concat(";path=", path));
            }
            if (domain != null)
            {
                fullCookie.Append(string.Concat(";domain=", domain));
            }
            if (secure)
            {
                fullCookie.Append(";secure");
            }
            HtmlPage.Document.SetProperty("cookie", fullCookie.ToString());
        }
    }
}
