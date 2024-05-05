using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// CookieParserExtension
    /// </summary>
    internal static class CookieParserExtension
    {
        /// <summary>
        /// Parse
        /// </summary>
        /// <param name="cookieHeader"></param>
        /// <returns></returns>
        public static Cookie ParseCookie(this string cookieHeader)
        {
            if (string.IsNullOrWhiteSpace(cookieHeader))
            {
                return null;
            }

            var vals = cookieHeader.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (0 == vals.Length)
            {
                return null;
            }

            var regex = new Regex(@"(.+?)=(.*)?");
            var isFirst = true;
            Cookie cookie = null;

            foreach (var val in vals)
            {
                var match = regex.Match(val);
                if (isFirst)
                {
                    if (!match.Success)
                    {
                        return null;
                    }

                    cookie = new Cookie(match.Groups[1].Value, match.Groups[2].Value);
                    isFirst = false;

                    continue;
                }

                if (!match.Success)
                {
                    continue;
                }

                var value = match.Groups[2].Value;
                switch (match.Groups[1].Value.ToUpper().Trim())
                {
                    case "DOMAIN":
                        cookie.Domain = value;

                        break;

                    case "EXPIRES":
                        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AdjustToUniversal, out var dateTime))
                        {
                            cookie.Expires = dateTime;
                        }

                        break;

                    case "MAX-AGE":
                        if (DateTime.MinValue == cookie.Expires && int.TryParse(value, out var intVal))
                        {
                            cookie.Expires = DateTime.Now.AddSeconds(intVal);
                        }

                        break;

                    case "PATH":
                        cookie.Path = value;

                        break;

                    case "HTTPONLY":
                        cookie.HttpOnly = bool.TryParse(value, out var boolVal) && boolVal;

                        break;

                    case "SECURE":
                        cookie.Secure = bool.TryParse(value, out boolVal) && boolVal;

                        break;

                    case "DISCARD":
                        cookie.Discard = bool.TryParse(value, out boolVal) && boolVal;

                        break;

                    case "VERSION":
                        if (int.TryParse(value, out intVal))
                        {
                            cookie.Version = intVal;
                        }

                        break;

                    case "COMMENT":
                        cookie.Comment = value;

                        break;

                    case "COMMENTURI":
                        if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri))
                        {
                            cookie.CommentUri = uri;
                        }

                        break;

                    case "PORT":
                        cookie.Port = value;

                        break;
                }
            }

            return cookie;
        }

        /// <summary>
        /// ParseCookie
        /// </summary>
        /// <param name="cookieHeaders"></param>
        /// <returns></returns>
        public static IReadOnlyList<Cookie> ParseCookie(this IEnumerable<string> cookieHeaders)
        {
            var cookies = new List<Cookie>();

            if (!cookieHeaders?.Any() ?? true)
            {
                return cookies;
            }

            foreach (var cookieHeader in cookieHeaders)
            {
                var cookie = ParseCookie(cookieHeader);
                if (null != cookie)
                {
                    cookies.Add(cookie);
                }
            }

            return cookies;
        }
    }
}