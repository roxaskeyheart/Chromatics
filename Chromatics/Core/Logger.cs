﻿using Chromatics.Enums;
using Chromatics.Helpers;
using Chromatics.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chromatics.Core
{
    public delegate void OnConsoleLoggedEventHandler(object source, OnConsoleLoggedEventArgs e);

    public static class Logger
    {        
        public static event OnConsoleLoggedEventHandler OnConsoleLogged = delegate { };

        public static void WriteConsole(LoggerTypes type, string message)
        {
            var color = (Color)EnumExtensions.GetAttribute<DefaultValueAttribute>(type).Value;

            OnConsoleLogged(null, new OnConsoleLoggedEventArgs(message, color));
        }
    }
}
