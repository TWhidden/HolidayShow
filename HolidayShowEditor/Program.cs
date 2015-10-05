using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HolidayShowEditor
{
    class Program
    {
        public static List<string> ApplicationArgs { get; private set; }

        [STAThread]
        static void Main(string[] args)
        {
            ApplicationArgs = new List<string>();

            foreach (var s in args)
            {
                ApplicationArgs.Add(s);
            }

            LoadApplication();
        }



        [MethodImpl(MethodImplOptions.NoInlining)]
        static void LoadApplication()
        {
            var app = new App();
            app.InitializeComponent();
            try
            {
                app.Run();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
