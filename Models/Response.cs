using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIConceptUsingFileSystem.Models
{

    public enum EC
    {
        OK,
        UNKNOWN_SERVER_ERROR,
        NOT_IMPLEMENTED,
        UNSUPPORTED_ITEM_TYPE,
        COULD_NOT_STORE_ITEM
    }

    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(List<object>))]
    public class Response
        {
            public Response()
            {
            }

            public void SetErrorCode(EC errorCode)
            {
                this.ErrorCode = (int)errorCode;
                this.Message = errorCode.ToString();
            }

            public Response(EC errorCode)
            {
                SetErrorCode(errorCode);
            }

            public Response(string message)
            {
                this.Message = Utils.GetFirstLine(message);
            }

            public Response(string message, int value) : this(message)
            {
                _result.Integer = value;
            }

            public Response(string message, float value) : this(message)
            {
                _result.Float = value;
            }

            public Response(string message, decimal value) : this(message)
            {
                _result.Decimal = value;
            }

            public Response(string message, string value) : this(message)
            {
                _result.String = value;
            }

            public Response(string message, bool value) : this(message)
            {
                _result.Bit = value;
            }

            public Response(string message, object value) : this(message)
            {
                _result.Object = value;
            }

            private string _message = string.Empty;

            public string Message
            {
                get { return _message; }
                set
                {
                    _message = value;
                    if (value != Definitions.OK)
                    {
                        this.ErrorCode = -1;
                    }
                }
            }

            public int ErrorCode
            {
                get;
                set;
            }

            private Result _result = new Result();

            public Result Result
            {
                get { return _result; }
                set { _result = value; }
            }

            private string caller = "";

            public string Caller
            {
                get { return caller; }
                set { caller = value; }
            }
        }

    public class Result
    {
        public Guid Guid { get; set; }

        private string _string = string.Empty;

        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private int _int;

        public int Integer
        {
            get { return _int; }
            set { _int = value; }
        }

        private long _long;

        public long Number
        {
            get { return _long; }
            set { _long = value; }
        }

        private float _float;

        public float Float
        {
            get { return _float; }
            set { _float = value; }
        }
        private decimal _decimal;

        public decimal Decimal
        {
            get { return _decimal; }
            set { _decimal = value; }
        }
        private bool _bit;

        public bool Bit
        {
            get { return _bit; }
            set { _bit = value; }
        }

        private object _obj;

        public object Object
        {
            get { return _obj; }
            set { _obj = value; }
        }
    }

    public static class Utils
    {
        public static void StartThread(Action task)
        {
            //Task.Factory.StartNew(() =>
            //{
            try //prevent crashes within this thread
            {
                task();
            }
            catch (Exception ex)
            {
                DataManager.GetInstance().ForceLog2FS("THREAD/CRASH: " + ex + Environment.NewLine, true);
            }
            //}, System.Threading.CancellationToken.None,
            //    TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness,
            //    TaskScheduler.Default
            //);
        }

        public static byte[] Compress(byte[] str)
        {
            try
            {
                MemoryStream msObj = new MemoryStream();
                using (Stream ds = new DeflateStream(msObj, CompressionMode.Compress))
                    ds.Write(str, 0, str.Length);
                byte[] buf = msObj.GetBuffer();
                byte[] final = new byte[buf.Length + 4];
                byte[] head = BitConverter.GetBytes(str.Length);
                Buffer.BlockCopy(head, 0, final, 0, 4);
                Buffer.BlockCopy(buf, 0, final, 4, final.Length - 4);
                return final;
            }
            catch
            { }
            return null;
        }

        public static byte[] Uncompress(byte[] str)
        {
            try
            {
                int size = BitConverter.ToInt32(str, 0);
                byte[] final = new byte[size];
                MemoryStream ms = new MemoryStream(str);
                ms.Position = 4;
                using (Stream ds = new DeflateStream(ms, CompressionMode.Decompress))
                    ds.Read(final, 0, size);
                return final;
            }
            catch { }
            return null;
        }

        public class AcceptAllCertificatePolicy : ICertificatePolicy
        {
            public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
            {
                return true;
            }
        }

        public static DateTime UnixTimestampToLocalTime(long timestamp, int tzOffset = 0)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(-tzOffset).AddMilliseconds(timestamp);
        }

        private static ImageCodecInfo getEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        public static string GetCustomLogo(Guid memberId, string logoName, string tempPath)
        {
            try
            {
                string style = "";

                string imageName = Path.GetFileNameWithoutExtension(logoName);
                string fileName = Path.Combine(tempPath, imageName + ".jpg");
                Directory.CreateDirectory(tempPath);

                Monsoon mon = new Monsoon();
                if (mon.Load(logoName) == 0)
                {
                    //                    if (mon.Load(logoName) == 0)
                    //                    {
                    if (mon.Bitmaps != null && mon.Bitmaps.Count > 0)
                    {
                        MonBitmap bitmap = mon.Bitmaps[0];
                        using (Bitmap tmpImage = new Bitmap(bitmap.Image))
                        {
                            ImageCodecInfo jpegCodec = getEncoderInfo("image/jpeg");
                            EncoderParameters encoderParams = new EncoderParameters(1);
                            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 20L);
                            tmpImage.Save(fileName, jpegCodec, encoderParams);
                        }
                        float fleft = (float)Math.Round((bitmap.Area.Left * 72f / 1000), 2);
                        float ftop = (float)Math.Round((bitmap.Area.Top * 72f / 1000), 2);
                        float fright = (float)Math.Round((bitmap.Area.Right * 72f / 1000), 2);
                        float fbottom = (float)Math.Round((bitmap.Area.Bottom * 72f / 1000), 2);
                        float fwidth = (float)Math.Round(fright - fleft, 2);
                        float fheight = (float)Math.Round(fbottom - ftop, 2);
                        fleft = (float)Math.Round(fleft + 2f, 2);
                        style = "left:" + fleft.ToString() + "pt;top:" + ftop.ToString() + "pt;width:" + fwidth.ToString() + "pt;height:" + fheight.ToString() + "pt;";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    try
                    {
                        File.Copy(logoName, fileName, true);
                    }
                    catch { }
                }
                string src = "/tmp/" + memberId.ToString().ToUpper() + "/logos/" + Path.GetFileName(fileName);
                string imgPath = "<img class=\"zf\" src=\"" + src + "\"" + ((style.Trim().Length > 0) ? " style=\"" + style + "\"" : "") + " alt=\"Logo\" />";
                File.WriteAllText(Path.Combine(tempPath, imageName + ".img"), imgPath);

                return imgPath;
            }
            catch (Exception ex)
            {
                string dir = "" + ConfigurationManager.AppSettings["LogDirectory"];
                if (!string.IsNullOrEmpty(dir))
                {
                    try
                    {
                        File.WriteAllText(dir + "customLogoError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
                    }
                    catch
                    {
                    }
                }
                return string.Empty;
            }
        }

        #region encryption

        public static string PackJS(string input)
        {
            ECMAScriptPacker packer = new ECMAScriptPacker();
            //packer.Encoding = ECMAScriptPacker.PackerEncoding.None;
            return getCopyright() + packer.Pack(input);
        }

        public static string PackCSS(string input)
        {
            //return input;
            string r = Regex.Replace(input, @"\s*{\s*", "{");
            r = Regex.Replace(r, @"\s*}\s*", "}");
            r = Regex.Replace(r, @"\s*;\s*", ";");
            r = Regex.Replace(r, @"\s*:\s*", ":");
            r = Regex.Replace(r, @"\s*,\s*", ",");
            r = r.Replace("\t", string.Empty);
            return getCopyright() + Regex.Replace(r, "(/" + Regex.Escape("*") + ".*?" + Regex.Escape("*") + "/)", string.Empty);
        }

        private static string getCopyright()
        {
            return getCopyright(false);
        }

        private static string getCopyright(bool asMarkup)
        {
            return (asMarkup ? "<!--\n" : "/*\n")
                    + "The copyright laws of the United States (Title 17 U.S. Code) forbid the unauthorized reproduction of this webpage,\n"
                    + "form, the source or image version of this webpage or form, or any portion of any of the foregoing,\n"
                    + "including but not limited to the HTML code, javascript, CSS, or images, by photocopy machine or any other means,\n"
                    + "including but not limited to, use of copy/paste or save as functions, or by copying the source of any portion of the page.\n"
                    + "Copyright© " + DateTime.Now.Year.ToString() + ", project.  ALL RIGHTS RESERVED. \n"
                    + (asMarkup ? "-->\n" : "*/\n");
        }

        public static string GetMD5Hash(string input)
        {
            return Utils.GetMD5Hash(System.Text.Encoding.UTF8.GetBytes(input));
        }


        public static string GetMD5Hash(byte[] input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = x.ComputeHash(input);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2"));
            }
            return s.ToString();
        }

        public static void RC4(ref Byte[] bytes, Byte[] key)
        {
            Byte[] s = new Byte[256];
            Byte[] k = new Byte[256];
            Byte temp;
            int i, j;

            for (i = 0; i < 256; i++)
            {
                s[i] = (Byte)i;
                k[i] = key[i % key.GetLength(0)];
            }

            j = 0;
            for (i = 0; i < 256; i++)
            {
                j = (j + s[i] + k[i]) % 256;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
            }

            i = j = 0;
            for (int x = 0; x < bytes.GetLength(0); x++)
            {
                i = (i + 1) % 256;
                j = (j + s[i]) % 256;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
                int t = (s[i] + s[j]) % 256;
                bytes[x] ^= s[t];
            }
        }

        public static string RC4(string data)
        {
            return RC4(Encoding.Default.GetBytes(data), "refn2003#", false);
        }

        public static string Encrypt2Base64(string data)
        {
            byte[] enc = Encoding.Default.GetBytes(data);
            RC4(ref enc, Encoding.Default.GetBytes("refn2003#"));
            return Convert.ToBase64String(enc);
        }

        public static string DecryptBase64(string data)
        {
            try
            {
                return RC4(Convert.FromBase64String(data.Trim().Replace(" ", "+")), "refn2003#", false);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string RC4(string data, string key)
        {
            return RC4(Encoding.Default.GetBytes(data), key, false);
        }

        private static string RC4(byte[] str, string key, bool pipeDelimited)
        {
            if (str.Length == 0 || string.IsNullOrEmpty(key))
                return string.Empty;

            byte[] keyArr = Encoding.Default.GetBytes(key);

            RC4(ref str, keyArr);

            if (pipeDelimited)
            {
                StringBuilder sb = new StringBuilder();
                for (int k = 0; k < str.Length; k++)
                {
                    sb.Append(str[k].ToString("N0") + "|");
                }
                return sb.ToString(0, sb.Length - 1);
            }
            else
            {
                return Encoding.Default.GetString(str);
            }
        }

        public static string ZfoPwdEncrypt(string password)
        {
            return RC4(Encoding.Default.GetBytes(password), "refn2003#", true);
        }

        public static string ZfoPwdDecrypt(string pipeDelimitedString)
        {
            string[] arr = pipeDelimitedString.Split(new char[] { '|' }, StringSplitOptions.None);
            byte[] enc = new byte[arr.Length];

            if (arr.Length > 1)
            {
                for (int j = 0; j < arr.Length; j++)
                {
                    if (!string.IsNullOrEmpty(arr[j]))
                    {
                        enc[j] = byte.Parse(arr[j]);
                    }
                }

                return RC4(enc, "refn2003#", false);
            }
            return string.Empty;
        }

        /// <summary>
        /// Generates statistically unique alphanumeric code based on the input string and desired character length. The code is separated into 2 
        /// equal groups if the specified codelength is even. If the codelength is odd the second group will be 1 characted longer.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="codelength"></param>
        /// <returns>Alphanumeric string of specified length with the dash breaking it into 2 groups</returns>
        public static string GetUniqueCode(string input, int codelength)
        {
            char[] chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            string s = input + DateTime.UtcNow.GetHashCode().ToString("X");
            byte[] hash;
            using (HMACSHA1 sha1 = new HMACSHA1())
            {
                hash = sha1.ComputeHash(UTF8Encoding.UTF8.GetBytes(s));
            }
            int startpos = hash[hash.Length - 1] % (hash.Length - codelength);
            StringBuilder passbuilder = new StringBuilder();
            for (int i = startpos; i < startpos + codelength; i++)
            {
                passbuilder.Append(chars[hash[i] % chars.Length]);
            }
            int dashIdx = codelength / 2 - codelength % 2;
            return passbuilder.ToString().Insert(dashIdx, "-");
        }

        /// <summary>
        /// Generates statistically unique alphanumeric code of desired character length. The code is separated into 2 
        /// equal groups if the specified codelength is even. If the codelength is odd the second group will be 1 characted longer.
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns>Alphanumeric string of specified length with the dash breaking it into 2 groups</returns>
        public static string GetUniqueKey(int keySize)
        {
            char[] chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            byte[] data = new byte[keySize];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(keySize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            int dashIdx = keySize / 2 - keySize % 2;
            return result.ToString().Insert(dashIdx, "-");
        }

        #endregion

        #region misc

        public static bool IsNonEmpty(string str)
        {
            int n = 0;
            if (int.TryParse(str, out n))
                return n > 0;
            return !string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Uses <code>Path.GetTempFileName</code> to create a file on the file system or, if <paramref name="path"/>
        /// is provided, creates a file in that directory. (WARNING: directory must exist).
        /// </summary>
        /// <param name="path">Optional directory in which to create a temporary file. Must exist on disk.</param>
        /// <returns>Full path to newly created file.</returns>
        public static string GetTempFileName(string path)
        {
            string fn;
            if (string.IsNullOrEmpty(path))
            {
                fn = Path.GetTempFileName();
            }
            else
            {
                fn = path + "\\" + Guid.NewGuid().ToString();
                FileStream fs = File.Create(fn);
                fs.Close();
                File.SetAttributes(fn, File.GetAttributes(fn) | FileAttributes.Temporary);
            }
            return fn;
        }

        /// <summary>
        /// Uses <code>Path.GetRandomFileName</code> to get a cryptographically strong, random string that can be used as
        /// either a folder name or a file name. If <paramref name="path"/> has an extension, the returned name will have
        /// the same extension by removing existing <code>Path.GetRandomFileName</code> dot and then appending the extension.
        /// This is just a name, does not create the file.
        /// </summary>
        /// <param name="path">If this contains an extension, return value will have the same extension.</param>
        /// <returns>The new random file name.</returns>
        public static string GetRandomFileName(string path)
        {
            string rand = Path.GetRandomFileName();
            try
            {
                string ext = Path.GetExtension(path);
                if (!string.IsNullOrEmpty(ext))
                {
                    rand = rand.Replace(".", "") + ext;
                }
            }
            catch { }
            return rand;
        }

        public static Guid String2Guid(string s)
        {
            Guid g = Guid.Empty;
            if (!string.IsNullOrEmpty(s))
            {
                try
                {
                    g = new Guid(s);
                }
                catch
                {
                }
            }
            return g;
        }

        public static string Guid2String(Guid g)
        {
            return g.ToString().ToUpper();
        }

        public static bool IsNullOrEmpty(Guid g)
        {
            return g == null || g == Guid.Empty;
        }

        public static string DoubleSingleQuotes(string str)
        {
            return str.Replace("\'", "\'\'");
        }
        public static string EscapeQuotes(string str)
        {
            return str.Replace("\'", "\\\'").Replace("\"", "\\\"");
        }

        public static string JsonSafe(string str)
        {
            str = str.Replace("\\", "\\\\");
            return EscapeQuotes(str);
        }

        public static string GetFirstLine(string message)
        {
            int len = message.IndexOf(Environment.NewLine);
            if (len > 0)
            {
                message = message.Substring(0, len);
            }
            return message;
        }

        public static DateTime ParseDate(string s)
        {
            DateTime dt = DateTime.MinValue;
            if (!DateTime.TryParseExact(s, "yyyyMMdd", null, DateTimeStyles.None, out dt))
            {
                DateTime.TryParse(s, out dt);
            }
            return dt;
        }

        public static int ParseInt(string s)
        {
            int n = 0;
            int.TryParse(s, out n);
            return n;
        }
        public static long ParseLong(string s)
        {
            long n = 0;
            long.TryParse(s, out n);
            return n;
        }
        public static float ParseFloat(string s)
        {
            float n = 0;
            float.TryParse(s, out n);
            return n;
        }

        public static string FormatCurrency(float price)
        {
            return price.ToString("N2");
        }

        #endregion

        public static void DataBind(object o, Dictionary<string, object> keyVal)
        {
            if (o == null || keyVal == null)
                return;

            foreach (string key in keyVal.Keys)
            {
                try
                {
                    PropertyInfo p = o.GetType().GetProperty(key);
                    if (p != null)
                    {

                        p.SetValue(o, keyVal[key], null);
                    }
                }
                catch
                {
                    //skip
                }
            }
        }

        public static string ResolveFilePath(string fileName)
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (File.Exists(dir + "\\" + fileName))
                return dir + "\\" + fileName;

            dir = Directory.GetParent(dir).FullName;
            if (File.Exists(dir + "\\" + fileName))
                return dir + "\\" + fileName;

            dir = ConfigurationManager.AppSettings[fileName.ToLower()];
            if (!string.IsNullOrEmpty(dir) && File.Exists(dir))
                return dir;

            return string.Empty;
        }

        public static bool MatchString(string str, string regexstr)
        {
            if (str == null)
            {
                return false;
            }
            str = str.Trim();
            System.Text.RegularExpressions.Regex pattern = new System.Text.RegularExpressions.Regex(regexstr);
            return pattern.IsMatch(str);
        }

        public static bool IsValidUSState(string strState)
        {
            // Names of 50 US States
            string[] stateNames =  {"ALABAMA","ALASKA","ARIZONA","ARKANSAS","CALIFORNIA","COLORADO","CONNECTICUT","DELAWARE",
                            "FLORIDA","GEORGIA","HAWAII","IDAHO","ILLINOIS","INDIANA","IOWA","KANSAS","KENTUCKY","LOUISIANA",
                            "MAINE","MARYLAND","MASSACHUSETTS","MICHIGAN","MINNESOTA","MISSISSIPPI","MISSOURI","MONTANA",
                            "NEBRASKA","NEVADA","NEW HAMPSHIRE","NEW JERSEY","NEW MEXICO","NEW YORK","NORTH CAROLINA",
                            "NORTH DAKOTA","OHIO","OKLAHOMA","OREGON","PENNSYLVANIA","RHODE ISLAND","SOUTH CAROLINA",
                            "SOUTHDAKOTA","TENNESSEE","TEXAS","UTAH","VERMONT","VIRGINIA","WASHINGTON","WEST VIRGINIA",
                            "WISCONSIN","WYOMING"};
            // Postal codes of 50 US States
            string[] stateCodes =  {"AL","AK","AZ","AR","CA","CO","CT","DE","DC","FL","GA","HI","ID","IL","IN","IA","KS","KY","LA",
                             "ME","MD","MA","MI","MN","MS","MO","MT","NE","NV","NH","NJ","NM","NY","NC","ND","OH","OK","OR",
                             "PA","RI","SC","SD","TN","TX","UT","VT","VA","WA","WV","WI","WY"};

            // This one is somewhat easier because we have a finite set of values to check against.
            // We simply uppercase our value anc check against our list.
            strState = strState.ToUpper();
            ArrayList stateCodesArray = new ArrayList(stateCodes);
            ArrayList stateNamesArray = new ArrayList(stateNames);
            return (stateCodesArray.Contains(strState) || stateNamesArray.Contains(strState));
        }

        public static bool IsSampleLibrary(string container)
        {
            string[] sample = {"AIRWF","ARES","C21AE","C21LLR","CAR","CARCFC","CARFR","CB","CBNC","CBRB","CORE","DPODR",
                                "EPUBD","EPUBS","HHRE","LSPR","NCFC","PJD","PRUDCR","PURB","PWVAR","REIL","REPCA",
                                "RMF","SFAOR","SLRAR","WINK","WWRER","ZXSAMPLE"};
            ArrayList sampleArray = new ArrayList(sample);
            return (sampleArray.Contains(container));

        }

        public static bool IsValidEmailAddress(string strEmail)
        {
            // Allows common email address that can start with a alphanumeric char and contain word, dash and period characters
            // followed by a domain name meeting the same criteria followed by a alpha suffix between 2 and 9 character lone
            string regExPattern = @"^([_0-9a-zA-Z]([-.\w]*[_0-9a-zA-Z])*@([_0-9a-zA-Z]([-\w]*[_0-9a-zA-Z]){0,1}\.)+[a-zA-Z]{2,9})$";
            return MatchString(strEmail, regExPattern);
        }

        public static void AddMultipleRecipients(MailAddressCollection mac, string addresses)
        {
            if (string.IsNullOrEmpty(addresses) || mac == null)
                return;

            string[] arr = addresses.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in arr)
            {
                string m = str.Trim().ToLower();
                if (m.Length > 0)
                {
                    mac.Add(m);
                }
            }
        }

        public static string SmtpHost = string.Empty;

        public static bool SendEmail(string to, string subject, string body)
        {
            return SendEmail("do-not-reply-zipformplus@mail.project.com", to, subject, body, null);
        }

        public static bool SendEmail(string from, string to, string subject, string body)
        {
            return SendEmail(from, to, "", "", from, subject, body, null);
        }

        public static bool SendEmail(string from, string to, string subject, string body, object attachments)
        {
            return SendEmail(from, to, "", "", from, subject, body, attachments);
        }

        public static bool SendEmail(string from, string to, string cc, string subject, string body, object attachments)
        {
            return SendEmail(from, to, cc, "", from, subject, body, attachments);
        }

        public static bool SendEmail(string from, string to, string cc, string bcc, string replyto, string subject, string body, object attachments)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                return false;

            string sender = from;
            from = fixYahooFromField(from, to + cc + bcc);

            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress(from);
                //message.Sender = new MailAddress(sender);

                AddMultipleRecipients(message.To, to);
                AddMultipleRecipients(message.CC, cc);
                AddMultipleRecipients(message.Bcc, bcc);

                message.Subject = subject;
                message.Body = body;

                if (body.IndexOf("<html") >= 0 || body.IndexOf("<HTML") >= 0)
                {
                    message.IsBodyHtml = true;
                    message.Body = message.Body
                        .Replace("<p>",
                        "<p style=\"margin-bottom:0px;margin-left:0px;margin-right:0px;margin-top:0px;-webkit-margin-before: 0em;-webkit-margin-after: 0em;\">")
                        .Replace("<body>",
                        "<body style=\"margin: 0px;\">");
                }

                message.Priority = MailPriority.Normal;


                if (!string.IsNullOrEmpty(replyto))
                {
                    FrameworkSpecific.AddReplyTo(message, replyto);
                }

                if (attachments is AttachmentCollection)
                {
                    AttachmentCollection c = attachments as AttachmentCollection;
                    foreach (Attachment a in c)
                    {
                        message.Attachments.Add(a);
                    }
                }

                SmtpClient client;
                string faxSmtpHost = ConfigurationManager.AppSettings["FaxOverrideSmtpHostIP"];
                if (to.Contains("@fax.project") && !string.IsNullOrEmpty(faxSmtpHost))
                {
                    client = new SmtpClient(faxSmtpHost);
                }
                else if (!string.IsNullOrEmpty(Utils.SmtpHost))
                {
                    client = new SmtpClient(Utils.SmtpHost);
                }
                else
                {
                    client = new SmtpClient();
                }

                client.Credentials = (ICredentialsByHost)CredentialCache.DefaultNetworkCredentials;
                FrameworkSpecific.CleanupForSendAsync(client, message);
                client.SendAsync(message, null);
            }
            catch (Exception ex)
            {
                LogPublisher pub = new LogPublisher("calypso");
                pub.Publish(new LogEntry(LogEntryType.Error, "SendEmail", ex.Message));
                return false;
            }

            return true;
        }

        static string fixYahooFromField(string from, string to)
        {
            from = from.ToLower();
            string dmarc = "yahoo.com,aol.com," + ConfigurationManager.AppSettings["DMARC_Participants"];
            List<string> li = Utils.Split(dmarc.ToLower());
            bool found = false;
            foreach (string s in li)
            {
                if (from.Contains(s))
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                return GetNoReplyAddress();
            }
            return from;
        }

        public static string GetNoReplyAddress()
        {
            string f = "" + ConfigurationManager.AppSettings["NoReplyAddress"];
            if (f.Length > 0)
                return f;
            else
                return "do-not-reply-zipformplus@mail.project.com";
        }


        #region Email methods with NDR check
        public static Response SendEmailWithNdrCheck(string to, string subject, string body)
        {
            return SendEmailWithNdrCheck("do-not-reply-zipformplus@mail.project.com", to, subject, body, null);
        }

        public static Response SendEmailWithNdrCheck(string from, string to, string subject, string body)
        {
            return SendEmailWithNdrCheck(from, to, "", "", from, subject, body, null);
        }

        public static Response SendEmailWithNdrCheck(string from, string to, string subject, string body, object attachments)
        {
            return SendEmailWithNdrCheck(from, to, "", "", from, subject, body, attachments);
        }

        public static Response SendEmailWithNdrCheck(string from, string to, string cc, string subject, string body, object attachments)
        {
            return SendEmailWithNdrCheck(from, to, cc, "", from, subject, body, attachments);
        }

        public static Response SendEmailWithNdrCheck(string from, string to, string cc, string bcc, string replyto, string subject, string body, object attachments)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                return new Response("SENDEMAIL_FAILED");

            string sender = from;
            from = fixYahooFromField(from, to + cc + bcc);

            NDRValidationResult r = NDRValidator.ValidateList(to);
            to = r.ValidEmails;
            string ndEmails = r.InvalidEmails;
            if (string.IsNullOrEmpty(to))
                return new Response("TO_UNDELIVERABLE", ndEmails);
            if (!string.IsNullOrEmpty(cc))
            {
                r = NDRValidator.ValidateList(cc);
                cc = r.ValidEmails;
                if (!string.IsNullOrEmpty(r.InvalidEmails))
                {
                    ndEmails += string.IsNullOrEmpty(ndEmails) ? r.InvalidEmails : ";" + r.InvalidEmails;
                }
            }
            if (!string.IsNullOrEmpty(bcc))
            {
                r = NDRValidator.ValidateList(bcc);
                bcc = r.ValidEmails;
                if (!string.IsNullOrEmpty(r.InvalidEmails))
                {
                    ndEmails += string.IsNullOrEmpty(ndEmails) ? r.InvalidEmails : ";" + r.InvalidEmails;
                }
            }
            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress(from);
                //message.Sender = new MailAddress(sender);

                AddMultipleRecipients(message.To, to);
                AddMultipleRecipients(message.CC, cc);
                AddMultipleRecipients(message.Bcc, bcc);

                message.Subject = subject;
                message.Body = body;

                if (body.IndexOf("<html") >= 0 || body.IndexOf("<HTML") >= 0)
                {
                    message.IsBodyHtml = true;
                    message.Body = message.Body
                        .Replace("<p>",
                        "<p style=\"margin-bottom:0px;margin-left:0px;margin-right:0px;margin-top:0px;-webkit-margin-before: 0em;-webkit-margin-after: 0em;\">")
                        .Replace("<body>",
                        "<body style=\"margin: 0px;\">");
                }

                message.Priority = MailPriority.Normal;

                if (!string.IsNullOrEmpty(replyto))
                {
                    FrameworkSpecific.AddReplyTo(message, replyto);
                }

                if (attachments is AttachmentCollection)
                {
                    AttachmentCollection c = attachments as AttachmentCollection;
                    foreach (Attachment a in c)
                    {
                        message.Attachments.Add(a);
                    }
                }

                SmtpClient client;
                string faxSmtpHost = ConfigurationManager.AppSettings["FaxOverrideSmtpHostIP"];
                if (to.Contains("@fax.project") && !string.IsNullOrEmpty(faxSmtpHost))
                {
                    client = new SmtpClient(faxSmtpHost);
                }
                else if (!string.IsNullOrEmpty(Utils.SmtpHost))
                {
                    client = new SmtpClient(Utils.SmtpHost);
                }
                else
                {
                    client = new SmtpClient();
                }

                client.Credentials = (ICredentialsByHost)CredentialCache.DefaultNetworkCredentials;
                FrameworkSpecific.CleanupForSendAsync(client, message);
                client.SendAsync(message, null);
            }
            catch (Exception ex)
            {
                LogPublisher pub = new LogPublisher("calypso");
                pub.Publish(new LogEntry(LogEntryType.Error, "SendEmail", ex.Message));
                return new Response("SENDEMAIL_FAILED", ex.Message);
            }

            return new Response(Definitions.OK, ndEmails);
        }

        #endregion

        public static string ConvertNonASCII2EntityReference(string data)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < data.Length; j++)
            {
                int c = (int)data[j];
                if (/*c < 32 || */c > 127)
                {
                    sb.Append("&#" + c.ToString() + ";");
                }
                else
                {
                    sb.Append(data[j]);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// This one is the good one, the other one crashes in replaceAmp
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// 
        //public static string XmlEncode(string data)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    XmlNode node = doc.AppendChild(doc.CreateElement("root"));
        //    node.InnerText = data;
        //    StringWriter writer = new StringWriter();
        //    XmlTextWriter xml_writer = new XmlTextWriter(writer);
        //    node.WriteContentTo(xml_writer);
        //    return ConvertNonASCII2EntityReference(writer.ToString());
        //}

        public static string XmlDecode(string str)
        {
            return System.Web.HttpUtility.HtmlDecode(str);
        }

        public static string XmlEncode(string data)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            StringBuilder sb = new StringBuilder(Regex.Replace(data, "&(?![a-zA-Z]{2,6};|#[0-9]{2,4};)", "&amp;"));
            sb.Replace("<", "&lt;");
            sb.Replace(">", "&gt;");
            sb.Replace("\"", "&quot;");
            sb.Replace("'", "&apos;");

            return ConvertNonASCII2EntityReference(sb.ToString());
        }

        public static bool GetFromCdnFolder(string cdnSubfolder, string localPath)
        {
            try
            {
                bool ok = File.Exists(localPath) && new FileInfo(localPath).Length > 0;
                if (!ok)
                {
                    string url = ConfigurationManager.AppSettings["ServerPathToOrigin"];
                    //if (InputValidator.IsValidURL(url))
                    {
                        url += ("/" + cdnSubfolder.Trim() + "/").Replace("\\", "/").Replace("//", "/");
                        ok = Utils.SaveFromRemoteFolder(url, localPath) && new FileInfo(localPath).Length > 0;
                    }
                }
                return ok;
            }
            catch
            {
                return false;
            }
        }

        public static bool CheckRemoteFileExists(string url)
        {
            return SaveFromRemoteFolder(url, null, true);
        }

        public static bool SaveFromRemoteFolder(string url, string fullPath)
        {
            return SaveFromRemoteFolder(url, fullPath, false);
        }

        private static bool SaveFromRemoteFolder(string url, string fullPath, bool justCheck)
        {
            HttpWebResponse res = null;
            try
            {
                if (!justCheck)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    url += "/" + Path.GetFileName(fullPath);
                }
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                res = (HttpWebResponse)req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    if (justCheck)
                    {
                        if (res != null)
                        {
                            res.Close();
                        }
                        return true;
                    }

                    Stream str = res.GetResponseStream();
                    int l = (int)res.ContentLength;
                    using (BinaryReader br = new BinaryReader(str))
                    using (BinaryWriter bw = new BinaryWriter(new FileStream(fullPath, FileMode.Create)))
                    {
                        int len = 64 * 1024;
                        byte[] arr = new byte[len];
                        int read = 0;
                        while ((read = br.Read(arr, 0, len)) > 0)
                        {
                            bw.Write(arr, 0, read);
                        }
                    }
                    return true;
                }
                else
                {
                    throw new Exception("SaveFromRemoteFolder web failure: " + res.StatusCode + ": " + res.StatusDescription);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (!string.IsNullOrEmpty(fullPath))
                        File.WriteAllText(fullPath + ".err", ex.Message);
                }
                catch
                {
                }
                return false;
            }
            finally
            {
                if (res != null)
                {
                    res.Close();
                }
            }
        }

        public static bool SearchFor(string needle, string haystack)
        {
            if (string.IsNullOrEmpty(needle) || string.IsNullOrEmpty(haystack))
                return false;
            string[] arr = needle.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string n in arr)
            {
                if (Regex.IsMatch(haystack, @"(^|[,;|])\s*" + n.Trim() + @"\s*($|[,;|])", RegexOptions.IgnoreCase))
                    return true;
            }
            return false;
        }

        public static List<string> Split(string string2split)
        {
            return Split(string2split, new char[] { '|', ',', ';' }, StringSplitOptions.None);
        }

        public static List<string> Split(string string2split, char[] seps)
        {
            return Split(string2split, seps, StringSplitOptions.None);
        }

        public static List<string> Split(string string2split, char[] seps, StringSplitOptions sso)
        {
            List<string> li = new List<string>();
            string2split = "" + string2split;
            if (seps != null && seps.Length > 0)
            {
                string[] arr = string2split.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in arr)
                {
                    li.Add(s.Trim());
                }
            }
            if (li.Count < 2 && sso == StringSplitOptions.None)
            {
                li.Clear();
                li.Add(string2split);
            }
            return li;
        }
        //Compares a tasks duedate and Returns true if the duedate was changed to a future date. 
        public static bool taskDateTimeCompare(string oldone, string newone)
        {
            DateTime dt1 = new DateTime();
            DateTime dt2 = new DateTime();
            try
            {
                dt1 = DateTime.ParseExact(oldone, "yyyyMMdd", CultureInfo.InvariantCulture);
                dt2 = DateTime.ParseExact(newone, "yyyyMMdd", CultureInfo.InvariantCulture);
                if (dt1 == dt2 || dt1 > dt2)
                    return false;
                else
                {
                    if (dt2 < DateTime.Now)
                        return false;
                    else
                        return true;
                }
            }
            catch { return false; }
        }


        /// <summary>
        /// Generates a random string with the given length
        /// </summary>
        /// <param name="size">Size of the string</param>
        /// <param name="lowerCase">If true, generate lowercase string</param>
        /// <returns>Random string</returns>
        public static string RandomString(int size)
        {
            return RandomString(size, false);
        }
        public static string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        public static bool InterpretBoolString(string s)
        {
            return (!string.IsNullOrEmpty(s) && ".true.yes.1.on.active.enabled.y.".IndexOf("." + s.ToLower() + ".") >= 0);
        }

        public static string ConvertObjectToJSON(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public static string HttpCall(string url, string postData)
        {
            return HttpCall(url, Encoding.ASCII.GetBytes(postData), "application/x-www-form-urlencoded", string.Empty, true);
        }

        public static string HttpCall(string url, byte[] postData, string contentType, string cookieHeader, bool getResponse)
        {
            try
            {
                ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = "application/x-www-form-urlencoded";
                }
                httpRequest.ContentType = contentType;
                httpRequest.ContentLength = postData.Length;
                //httpRequest.Timeout = 60000;
                httpRequest.KeepAlive = false;
                httpRequest.ProtocolVersion = new Version(1, 0);
                httpRequest.SendChunked = false;
                httpRequest.UserAgent = "project/http";
                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    httpRequest.Headers.Add("Cookie", cookieHeader);
                }

                if (postData != null && postData.Length > 0)
                {
                    Stream send = httpRequest.GetRequestStream();
                    send.Write(postData, 0, postData.Length);
                    send.Close();
                }

                if (getResponse)
                {

                    HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    Stream response = httpResponse.GetResponseStream();

                    StreamReader responseRead = new StreamReader(response, true);
                    string r = responseRead.ReadToEnd();

                    response.Close();

                    return r;
                }
                else
                {
                    return Definitions.OK;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        #region creating thumbnails

        public static byte[] CreateThumbnail(byte[] photo)
        {
            return CreateThumbnail(photo, 64, 64);
        }

        public static byte[] CreateThumbnail(byte[] photo, int width, int height)
        {
            if (width == 0)
                width = 64;
            if (height == 0)
                height = 64;
            try
            {
                Image i = Image.FromStream(new MemoryStream(photo));
                MemoryStream ms = new MemoryStream();
                CreateThumbnail(i, new Size(width, height)).Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
            catch
            {
                return null;
            }
        }

        public static Image CreateThumbnail(Image image, Size thumbnailSize)
        {
            float scalingRatio = CalculateScalingRatio(image.Size, thumbnailSize);

            int scaledWidth = (int)Math.Round((float)image.Size.Width * scalingRatio);
            int scaledHeight = (int)Math.Round((float)image.Size.Height * scalingRatio);
            int scaledLeft = (thumbnailSize.Width - scaledWidth) / 2;
            int scaledTop = (thumbnailSize.Height - scaledHeight) / 2;

            //For portrait mode, adjust the vertical top of the crop area so that we get more of the top area
            if (scaledWidth < scaledHeight && scaledHeight > thumbnailSize.Height)
            {
                scaledTop = (thumbnailSize.Height - scaledHeight) / 4;
            }

            Rectangle cropArea = new Rectangle(scaledLeft, scaledTop, scaledWidth, scaledHeight);

            System.Drawing.Image thumbnail = new Bitmap(thumbnailSize.Width, thumbnailSize.Height);
            using (Graphics thumbnailGraphics = Graphics.FromImage(thumbnail))
            {
                thumbnailGraphics.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                thumbnailGraphics.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraphics.DrawImage(image, cropArea);
            }
            return thumbnail;
        }

        private static float CalculateScalingRatio(Size originalSize, Size targetSize)
        {
            float originalAspectRatio = (float)originalSize.Width / (float)originalSize.Height;
            float targetAspectRatio = (float)targetSize.Width / (float)targetSize.Height;

            float scalingRatio = 0;

            if (targetAspectRatio >= originalAspectRatio)
            {
                scalingRatio = (float)targetSize.Width / (float)originalSize.Width;
            }
            else
            {
                scalingRatio = (float)targetSize.Height / (float)originalSize.Height;
            }

            return scalingRatio;
        }

        public static string Ellipses(string content, int len)
        {
            if (content == null)
            {
                return string.Empty;
            }
            if (content.Length > len)
            {
                return content.Substring(0, len - 3) + "...";
            }
            return content;
        }

        #endregion
    }

}
