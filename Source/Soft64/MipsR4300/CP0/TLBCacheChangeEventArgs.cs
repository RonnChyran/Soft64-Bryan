﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soft64.MipsR4300.CP0
{
    public sealed class TLBCacheChangeEventArgs : EventArgs
    {
        private Int32 m_Index;

        public TLBCacheChangeEventArgs(Int32 index)
        {
            m_Index = index;
        }

        public Int32 Index
        {
            get { return m_Index; }
        }
    }
}
