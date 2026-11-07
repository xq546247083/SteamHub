using Microsoft.Win32.TaskScheduler;

namespace SteamHub.Helper
{
    /// <summary>
    /// 计划任务帮助类
    /// </summary>
    public static class TaskSchedulerHelper
    {
        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="taskName"></param>
        public static void Delete(string taskName)
        {
            using (var ts = new TaskService())
            {
                ts.RootFolder.DeleteTask(taskName, exceptionOnNotExists: false);
            }
        }

        /// <summary>
        /// 检查任务是否存在
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <returns>是否存在</returns>
        public static bool Exists(string taskName)
        {
            using (var ts = new TaskService())
            {
                return ts.FindTask(taskName) != null;
            }
        }

        /// <summary>
        /// 添加启动任务
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Add(string taskName, string path)
        {
            using (var ts = new TaskService())
            {
                var existingTask = ts.FindTask(taskName);
                if (existingTask != null)
                {
                    ts.RootFolder.DeleteTask(taskName);
                }

                var td = ts.NewTask();
                td.RegistrationInfo.Author = AppGlobal.AppName;
                td.RegistrationInfo.Description = AppGlobal.AppName;
                td.Principal.RunLevel = TaskRunLevel.Highest;

                // 触发类型: 登录时
                td.Triggers.Add(new LogonTrigger());

                // 动作: 执行程序
                td.Actions.Add(new ExecAction(path));

                // 设置
                td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.RunOnlyIfIdle = false;

                ts.RootFolder.RegisterTaskDefinition(taskName, td, TaskCreation.CreateOrUpdate, null, null, TaskLogonType.InteractiveToken);
                return true;
            }
        }
    }
}
