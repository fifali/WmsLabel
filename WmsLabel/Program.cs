using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace WmsLabel
{
    class Program
    {
        static void Main(string[] args)
        {
            new Hello();
        }

        public class Hello
        {
            private GetTcpListener listener = new GetTcpListener();
            public Hello()
            {
                listener.GetListener();
            }
        }
    }
}
