using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Solid
{
    //Пример базового "божественного объекта" из реального кода, нарушающий Single responsibility principle,
    //Open/closed principle, Dependency inversion principle
    //Этот класс был удален, а функционал перераспределен между классами с соответсвующей ответсвенностью (функционалом)

    [Serializable]
    public class TBase //: TBaseRegClass
    {
        string mName = "0";
        [NonSerialized]
        private TDevSrvLogger Logger;

        //ublic TError Error;

        public struct SYSTEMTIME
        {
            public ushort wYear, wMonth, wDayOfWeek, wDay, wHour, wMinute, wSecond, wMilliseconds;
        }

        [DllImport("Ole32.dll",
        ExactSpelling = true,
        EntryPoint = "CoInitializeSecurity",
        CallingConvention = CallingConvention.StdCall,
        SetLastError = false,
        PreserveSig = false)]

        public static extern void CoInitializeSecurity(
        IntPtr voidPtr,
        int authSvc,
        IntPtr asAuthSvc,
        IntPtr reserved1,
        uint dwAuthnLevel,
        uint dwImpLevel,
        IntPtr authList,
        uint dwCapabilities,
        IntPtr reserved3);

        /// <summary>
        /// This function retrieves the current system date
        /// and time expressed in Coordinated Universal Time (UTC).
        /// </summary>
        /// <param name="lpSystemTime">[out] Pointer to a SYSTEMTIME structure to
        /// receive the current system date and time.</param>
        [DllImport("kernel32.dll")]
        public extern static void GetSystemTime(ref SYSTEMTIME lpSystemTime);

        /// <summary>
        /// This function sets the current system date
        /// and time expressed in Coordinated Universal Time (UTC).
        /// </summary>
        /// <param name="lpSystemTime">[in] Pointer to a SYSTEMTIME structure that
        /// contains the current system date and time.</param>
        [DllImport("kernel32.dll")]
        public extern static uint SetSystemTime(ref SYSTEMTIME lpSystemTime);

        public bool SetSystemTime(DateTime pDateTime)
        {
            SYSTEMTIME st = new SYSTEMTIME();
            GetSystemTime(ref st);
            st.wYear = (ushort)pDateTime.Year;
            st.wMonth = (ushort)pDateTime.Month;
            st.wDay = (ushort)pDateTime.Day;
            st.wHour = (ushort)pDateTime.Hour;
            st.wMinute = (ushort)pDateTime.Minute;

            if (SetSystemTime(ref st) == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Kills the prev copies.
        /// </summary>
        /// <param name="nameApp">The name app.</param>
        /// <param name="dateTime">The date time.</param>
        public void KillPrevCopies(string nameApp, DateTime dateTime)
        {
            Process[] processes = Process.GetProcessesByName(nameApp);
            //string vStr = Process.GetCurrentProcess().ProcessName;
            if (processes != null)
                if (processes.Length > 0)
                {
                    //Убиваем все процессы, которые были ранее запущены, но не текущий!
                    int cnt = processes.Length - 1;
                    for (int i = cnt; i >= 0; i--)
                    {
                        Process process = processes[i];
                        if (dateTime != process.StartTime)
                        {
                            LogValue(string.Format("Память процесса {0} before kill {1} / {2}", nameApp, process.WorkingSet64 / 1048576, process.PrivateMemorySize64), ETypeLog.Serv);
                            try
                            {
                                process.Kill();
                            }
                            catch (Exception ex)
                            {
                                LogValue(string.Format("Убить не удалось {0} - {1}", nameApp, ex.ToString()), ETypeLog.Serv);
                            }
                        }
                    }
                    //v_AlreadyExist = true;                    
                    //MessageBox.Show("Зафиксирован уже один запущенный процесс");                    
                }
        }

        public virtual void Destroy()
        {
        }

        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }

        //private string PreLogMessage; //для того, чтобы повторяющиеся сообщения не забивали Лог файл
        //private FileStream FileOfServLog;
        //private FileStream FileOfOutLog;
        //private FileStream FileOfCritLog;

        static TBase()
        {
            XmlConfigurator.Configure();
        }

        /// <summary>
        /// Logs the value to.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void LogOutTo(string value)
        {
        }

        /// <summary>
        /// Открытые лог файлы на текущую дату
        /// </summary>
        Hashtable logFiles = new Hashtable();
        /// <summary>
        /// Открытые лог файлы по устройству
        /// </summary>
        Hashtable logFilesWas = new Hashtable();

        /// <summary>
        /// Logs the value to.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="fileStream">The file stream.</param>
        private void LogValueTo(string value, string fileNameTemplate)
        {
            try
            {
                FileStream fileStream = null;
                bool exist = false;// = System.IO.File.Exists(fileName);
                string fileName = string.Format("{0}{1}{2}{3}{4}", fileNameTemplate, DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, ".log");

                lock (logFiles.SyncRoot)
                {
                    if (logFiles[fileName] != null)
                    {
                        exist = true;
                        fileStream = (FileStream)logFiles[fileName];
                    }

                    if (!exist)
                    {
                        if (logFilesWas[fileNameTemplate] != null)
                        {
                            fileStream = (FileStream)logFilesWas[fileNameTemplate];
                            fileStream.Close();
                            fileStream = null;
                        }

                        fileStream = new FileStream(fileName, FileMode.Append);
                        value = string.Format("\n\n\nStart app {0}\n\n{1}", DateTime.Now, value);
                        logFiles[fileName] = fileStream;
                        logFilesWas[fileNameTemplate] = fileStream;
                    }

                    if (fileStream != null)
                    {
                        StreamWriter sw = new StreamWriter(fileStream, Encoding.GetEncoding("Windows-1251"));
                        sw.WriteLine(value);
                        sw.Flush();
                        sw = null;
                    }
                }
            }
            catch
            {
            }
        }

        public void LogValue(string value, ETypeLog typeLog)
        {
            LogValue(value, typeLog, "");
        }

        /// <summary>
        /// Logs the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="typeLog">The type log.</param>
        public void LogValue(string value, ETypeLog typeLog, string specFileName)
        {
            if (Logger == null)
            {
                Logger = new TDevSrvLogger();
            }

            try
            {
                DateTime now = DateTime.Now;
                string valueTime = "{0} {1} [" + value + "]";
                string time = string.Format("[{0}/{1}/{2} {3}:{4}:{5}:{6}]",
                    now.Year.ToString("0000"),              //{0}
                    now.Month.ToString("00"),               //{1}
                    now.Day.ToString("00"),                 //{2}
                    now.Hour.ToString("00"),                //{3}
                    now.Minute.ToString("00"),              //{4}
                    now.Second.ToString("00"),              //{5}
                    now.Millisecond.ToString("00"));        //{6}

                string prefix = "";
                string str = string.Format(valueTime, time, prefix);

                switch (typeLog)
                {
                    case ETypeLog.External:
                        LogValueTo(str, specFileName);
                        break;
                    case ETypeLog.OPC:
                        LogValueTo(str, specFileName);
                        break;
                    case ETypeLog.Crit:
                        Logger.Warn(str);
                        break;
                    case ETypeLog.Out:
                        Logger.Info(str);
                        break;
                    case ETypeLog.Serv:
                        Logger.Debug(valueTime, time, prefix);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private object GetValueKeyFromFormatStr(string formatStr, out int afterIdx)
        {
            Hashtable result = null;
            int started = 0;
            string valueStr = "";
            string turnerStr = "";
            afterIdx = 0;
            int cntSymbs = 0;
            int cntSpecSymbs = 2;
            int num = 0;

            int i = 0;
            while (i < formatStr.Length)
            {
                afterIdx++;
                string ch = formatStr.Substring(i, 1);
                i++;
                turnerStr = ch + turnerStr;
                switch (ch)
                {
                    case "[":
                        started++;
                        break;
                    case "]":
                        started--;
                        break;
                    default:
                        if (int.TryParse(ch, out num))
                        {
                            cntSymbs++;
                        }
                        else
                        {
                            if ((ch != ":") && (ch != "["))
                            {
                                cntSymbs = 0;
                            }
                            else
                            {
                                cntSymbs++;
                            }
                        }

                        valueStr = valueStr + ch;
                        break;
                }

                if (turnerStr.Length > cntSpecSymbs)
                {
                    if (turnerStr.Substring(0, cntSpecSymbs) == "[:") //это значит, что встретилось в парсинге строка ":["
                    {
                        //это означает, что параметр содержит другие параметры
                        int startPos = Math.Max(0, afterIdx - cntSpecSymbs - cntSymbs);

                        string str = "";
                        result = ParseFormatStr(formatStr.Substring(startPos), result, out str);
                        afterIdx = formatStr.Length - str.Length;
                        i = afterIdx - 1;
                        afterIdx = afterIdx - 1;
                        valueStr = "";
                        cntSymbs = 0;
                    }
                }

                if (started < 0)
                {
                    i = formatStr.Length;
                }

            }

            if (result != null)
                return result;
            else
                return valueStr;
        }

        private bool IsBeginValueKey(string pFormatStr)
        {
            bool vResult = false;
            if (pFormatStr.Length >= 2)
            {
                if (pFormatStr.Substring(0, 2) == "[:")
                {
                    vResult = true;
                }
            }

            return vResult;
        }

        private string GetKey(string pFormatStr)
        {
            string vResult = "";

            for (int i = 0; i < pFormatStr.Length; i++)
            {
                string vCh = pFormatStr.Substring(i, 1);
                if ((vCh == ";") || (vCh == "[") || (vCh == "]") || (vCh == ":"))
                {
                    if (vResult.Length > 0)
                    {
                        break;
                    }
                }
                else
                {
                    vResult = vResult + vCh;
                }
            }

            return vResult;
        }

        public Hashtable ParseFormatString(string formatStr)
        {
            int delta = 0;
            Hashtable result = null;
            if (formatStr.Length > 0)
            {
                result = GetValueKeyFromFormatStr(formatStr, out delta) as Hashtable;
            }

            return result;
        }

        private Hashtable ParseFormatStr(string pFormatStr, Hashtable pResHash, out string UnParsedStr)
        {
            Hashtable vResult = null;
            if (pResHash != null)
            {
                vResult = pResHash;
            }
            else
            {
                vResult = new Hashtable();
            }
            string vKey = "";
            string vTurnStr = "";
            object vValue = "";

            //FindKey
            int vCurrPos = 0;
            int vStarter = 0;

            while (vStarter >= 0)
            {
                string vCh = pFormatStr.Substring(vCurrPos, 1);
                vCurrPos++;
                if (vCh == "]")
                {
                    vStarter--;
                }
                else
                {
                    vTurnStr = vCh + vTurnStr;
                    if (IsBeginValueKey(vTurnStr))
                    {
                        vKey = GetKey(pFormatStr);
                        int vDelta = 0;
                        vValue = GetValueKeyFromFormatStr(pFormatStr.Substring(vCurrPos), out vDelta);
                        vCurrPos = vCurrPos + vDelta;
                        vResult.Add(vKey, vValue);
                        break;
                    }
                }
            }
            UnParsedStr = pFormatStr.Substring(vCurrPos);

            return vResult;
        }

        public string GetParamValueByNum(string pFormatStr, string pKey, out Hashtable pHashTable)
        {
            pHashTable = ParseFormatString(pFormatStr);

            return (string)pHashTable[pKey];
        }
    }
}
