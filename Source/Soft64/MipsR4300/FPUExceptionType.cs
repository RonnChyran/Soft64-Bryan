﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soft64.MipsR4300
{
    public enum FPUExceptionType
    {
        Invalid,
        Underflow,
        Overflow,
        Unimplemented,
        Inexact,
        DivideByZero
    }
}
