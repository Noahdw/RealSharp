using System;
using Qml.Net;
using Qml.Net.Runtimes;


namespace RealSharp
{
    class Program
    {
        static int Main(string[] args)
        {
            RuntimeManager.DiscoverOrDownloadSuitableQtRuntime();

          using (var app = new QGuiApplication(args))
            {
                using (var engine = new QQmlApplicationEngine())
                {
                    Qml.Net.Qml.RegisterType<EditorModel>("Editor");
                    engine.Load("main.qml");
                    return app.Exec();
                }
            }
        }
    }
}
