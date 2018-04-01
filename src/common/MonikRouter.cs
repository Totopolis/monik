using System;
using System.Text;

namespace Monik.Common
{
    public class MonikRouter : IMonik
    {
        public MonikRouter() // TODO: console support, queue support
        {

        }

        public void ApplicationError(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void ApplicationFatal(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void ApplicationInfo(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void ApplicationVerbose(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void ApplicationWarning(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void KeepAlive()
        {
            throw new NotImplementedException();
        }

        public void LogicError(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void LogicFatal(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void LogicInfo(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void LogicVerbose(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void LogicWarning(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void OnStop()
        {
            Console.WriteLine("Monik.OnStop()");
        }

        public void SecurityError(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void SecurityFatal(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void SecurityInfo(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void SecurityVerbose(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void SecurityWarning(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void SystemError(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void SystemFatal(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void SystemInfo(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void SystemVerbose(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }

        public void SystemWarning(string aBody, params object[] aParams)
        {
            Console.WriteLine(aBody);
        }
    }//end of class
}
