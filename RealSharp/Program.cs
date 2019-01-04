using System;
using Qml.Net;


namespace RealSharp
{
    class Program
    {
        static int Main(string[] args)
        {
            Qt.PutEnv("QT_OPENGL", "angle");
            Qt.PutEnv("QT_ANGLE_PLATFORM", "warp");
            Qt.PutEnv("QT_DISABLE_SHADER_DISK_CACHE", "1");
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
