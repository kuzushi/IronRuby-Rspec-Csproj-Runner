﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rspec.Project.Runner
{
    public class ValidationError
    {
        public int Severity
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }
}
