
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class ULogger
{
    private static event Application.LogCallback LogCallbackEvent;
    private static bool _hasRegisterLogCallback = false;

    /// <summary>
    /// 是否输出到日志文件,默认false，需要初始化手工设置
    /// </summary>
    private static bool _isLogFile = false;
    private static bool _onlyError = false;
    public static bool IsLogFile
    {
        get { return _isLogFile; }
    }

    public static void SetLogFile(bool active, bool onlyError)
    {
        _isLogFile = active;
        _onlyError = onlyError;
        if (_isLogFile)
        {
            AddLogCallback(LogFileCallbackHandler);
        }
        else
        {
            RemoveLogCallback(LogFileCallbackHandler);
        }
    }

    /// <summary>
    /// 第一次使用时注册，之所以不放到静态构造器，因为多线程问题
    /// </summary>
    /// <param name="callback"></param>
    private static void AddLogCallback(Application.LogCallback callback)
    {
        if (!_hasRegisterLogCallback)
        {
            Application.logMessageReceivedThreaded += OnLogCallback;
            _hasRegisterLogCallback = true;
        }
        LogCallbackEvent += callback;
    }

    private static void RemoveLogCallback(Application.LogCallback callback)
    {
        LogCallbackEvent -= callback;
    }

    private static void LogFileCallbackHandler(string condition, string stacktrace, LogType type)
    {
        try
        {
            LogToFile(condition + stacktrace + "\n\n", true, type);
        }
        catch (Exception e)
        {
            LogToFile(string.Format("LogFileError: {0}, {1}", condition, e.Message), true, type);
        }
    }

    private static void OnLogCallback(string condition, string stacktrace, LogType type)
    {
        if (LogCallbackEvent != null)
        {
            lock (LogCallbackEvent)
            {
                LogCallbackEvent(condition, stacktrace, type);
            }
        }
    }

    // 是否写过log file
    public static bool IsLogFileExists(LogType type)
    {
        string fullPath = GetLogPath(type);
        return File.Exists(fullPath);
    }

    public static void ClearLogFile()
    {
        string dir = Path.Combine(Application.persistentDataPath, "logs");
        Directory.Delete(dir, true);
    }

    // 写log文件
    public static void LogToFile(string szMsg, bool append, LogType type)
    {
        if (_onlyError && type != LogType.Error && type != LogType.Exception)
        {
            return;
        }

        string fullPath = GetLogPath(type);
        string dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using (
            FileStream fileStream = new FileStream(fullPath, append ? FileMode.Append : FileMode.CreateNew,
                FileAccess.Write, FileShare.ReadWrite)) // 不会锁死, 允许其它程序打开
        {
            lock (fileStream)
            {
                StreamWriter writer = new StreamWriter(fileStream); // Append
                writer.Write(szMsg);
                writer.Flush();
                writer.Close();
            }
        }
    }

    // 用于写日志的可写目录
    public static string GetLogPath(LogType type)
    {
        var logPath = Path.Combine(Application.persistentDataPath, "logs/");
        var now = DateTime.Now;
        string logName = null;
        if (type == LogType.Error || type == LogType.Exception)
        {
            logName = string.Format("game_{0}_{1}_{2}_error.log", now.Year, now.Month, now.Day);
        }
        else
        {
            logName = string.Format("game_{0}_{1}_{2}.log", now.Year, now.Month, now.Day);
        }
        return logPath + logName;
    }

    public static void WatchPerformance(Action del)
    {
        WatchPerformance("执行耗费时间: {0}s", del);
    }

    public static void WatchPerformance(string outputStr, Action del)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start(); //  开始监视代码运行时间

        if (del != null)
        {
            del();
        }

        stopwatch.Stop(); //  停止监视
        TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
                                               //double seconds = timespan.TotalSeconds;  //  总秒数
        double millseconds = timespan.TotalMilliseconds;
        decimal seconds = (decimal)millseconds / 1000m;

        Debug.LogFormat(outputStr, seconds.ToString("F7")); // 7位精度
    }
}
