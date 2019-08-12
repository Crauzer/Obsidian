using System;
using System.Windows;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;

namespace Obsidian.Utils
{
    public static class Logging
    {
        public static void InitializeLogger(string loggingPattern, Level logLevel)
        {
            FileAppender appender = new FileAppender
            {
                Layout = new PatternLayout(loggingPattern),
                File = "ObsidianLog.txt",
                AppendToFile = true,
                Threshold = logLevel
            };
            appender.ActivateOptions();
            BasicConfigurator.Configure(appender);
        }

        public static void LogException(ILog logger, string message, Exception exception)
        {
            MessageBox.Show(string.Format("{0}\n{1}\n{2}", message, exception.Message, exception.StackTrace), "", MessageBoxButton.OK, MessageBoxImage.Error);
            logger.Error(message, exception);
        }

        public static void LogFatal(ILog logger, string message, Exception exception)
        {
            MessageBox.Show(string.Format("{0}\n{1}\n{2}", message, exception.Message, exception.StackTrace), "", MessageBoxButton.OK, MessageBoxImage.Error);
            logger.Fatal(message, exception);
        }
    }
}
