namespace SteamHub;

/// <summary>
/// 数据库变更通知器
/// </summary>
public static class DbChangeNotifier
{
    private static readonly AutoResetEvent _signal = new(false);

    /// <summary>
    /// 数据库变更事件
    /// </summary>
    public static event Action OnDatabaseChanged;

    static DbChangeNotifier()
    {
        var worker = new Thread(ProcessQueue)
        {
            IsBackground = true,
            Name = "DbChangeNotifierWorker"
        };
        worker.Start();
    }

    /// <summary>
    /// 通知数据库发生变更
    /// </summary>
    public static void Notify()
    {
        _signal.Set();
    }

    /// <summary>
    /// 消费事件并合并 1 秒内的多余信号
    /// </summary>
    private static void ProcessQueue()
    {
        while (true)
        {
            try
            {
                // 阻塞直到有新事件进入
                _signal.WaitOne();

                // 等待0.2秒并抹掉0.2秒内的其他信号
                Thread.Sleep(200);
                _signal.Reset();

                // 触发刷新事件
                OnDatabaseChanged?.Invoke();
            }
            catch
            {
                // 忽略异常，确保后台消费循环持续工作
            }
        }
    }
}
