using System;
using Microsoft.SPOT;

namespace OakhillLandroverController
{
    public class HomeMonitorException : Exception
    {
        public HomeMonitorException()
        {

        }

        public HomeMonitorException(string message)
            : base(message)
        {

        }

        public HomeMonitorException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
