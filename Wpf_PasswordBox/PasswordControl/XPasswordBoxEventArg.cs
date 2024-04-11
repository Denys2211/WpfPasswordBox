using System;
using System.Security;

namespace Wpf_PasswordBox
{
    public delegate void XPasswordBoxEventHandler(object sender, XPasswordBoxEventArg e);

    public class XPasswordBoxEventArg : EventArgs
    {
        private readonly SecureString _password;

        public XPasswordBoxEventArg(SecureString password)
        {
            _password = password;
        }

        public SecureString Password
        {
            get { return _password; }
        }
    }
}
