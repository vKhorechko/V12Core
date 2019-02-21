using System;
using System.Collections.Generic;
using System.Text;

namespace V12Core.Domain.Entities
{
    public class EmailOptions
    {
        public string SMTPserver { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
