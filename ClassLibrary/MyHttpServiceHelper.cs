using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class MyHttpServiceHelper
    {
        private static HttpClient _client;
        private static Timer _timer;

        /*
            不能有任何访问修饰符：静态构造函数不能有 public、private 或其他访问修饰符。
            必须是静态的：静态构造函数必须使用 static 关键字声明。
            没有参数：静态构造函数不能有任何参数。
         */
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static MyHttpServiceHelper()
        {
            _client = new HttpClient();
            _timer = new Timer(RefreshClient, null, TimeSpan.FromHours(24), TimeSpan.FromHours(24));
        }

        private static void RefreshClient(object state)
        {
            _client?.Dispose();
            _client = new HttpClient();
        }

        public static HttpClient GetClient()
        {
            return _client;
        }
        /// <summary>
        /// 释放资源
        /// 使用 ? 运算符确保 _client 和 _timer 不为 null 时再调用 Dispose 方法。
        /// </summary>
        public static void DisposeClient()
        {
            _client?.Dispose();
            _timer?.Change(Timeout.Infinite, Timeout.Infinite); // 停止定时器
            _timer?.Dispose();
            _client = null; // 清空引用
            _timer = null;
        }

        #region 每个独立的程序在调用后，都应该结束释放HttpClient资源
        //MyHttpServiceHelper.DisposeClient();
        /*
         解释
                单例模式管理 HttpClient：

                使用单例模式管理 HttpClient，确保在整个应用程序生命周期内只有一个实例。
                定期刷新 HttpClient 实例以保持其有效性。
                在适当的位置调用 DisposeClient 方法：

                在每个请求完成后不需要立即调用 DisposeClient，而是将其放在应用程序退出时调用。
                这样可以避免频繁创建和销毁 HttpClient 对象带来的性能开销。
                通过这种方式，你可以确保 HttpClient 的生命周期得到合理管理，同时避免不必要的性能损失。如果有其他问题，请随时提问。
         */
        #endregion
    }

}
