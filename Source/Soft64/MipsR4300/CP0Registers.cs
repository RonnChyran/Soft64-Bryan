﻿/*
Soft64 - C# N64 Emulator
Copyright (C) Soft64 Project @ Codeplex
Copyright (C) 2013 - 2014 Bryan Perris

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using System;
using System.Collections.Generic;

namespace Soft64.MipsR4300
{
    public enum CP0RegName : int
    {
        /// <summary>
        /// Invalid register name
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// Programmable pointer into TLB array
        /// </summary>
        Index,

        /// <summary>
        /// Pseudorandom pointer into TLB array (read only)
        /// </summary>
        Random,

        /// <summary>
        /// Low half of TLB entry for even virtual address (VPN)
        /// </summary>
        EntryLo0,

        /// <summary>
        /// Low half of TLB entry for odd virtual address (VPN)
        /// </summary>
        EntryLo1,

        /// <summary>
        /// Pointer to kernel virtual page table entry (PTE) in 32-bit addressing mode
        /// </summary>
        Context,

        /// <summary>
        /// TLB Page Mask
        /// </summary>
        PageMask,

        /// <summary>
        /// Number of wired TLB entries
        /// </summary>
        Wired,

        /// <summary>
        /// Bad virtual address
        /// </summary>
        BadVAddr = 8,

        /// <summary>
        /// Timer Count
        /// </summary>
        Count,

        /// <summary>
        /// High half of TLB entry
        /// </summary>
        EntryHi,

        /// <summary>
        /// Timer Compare
        /// </summary>
        Compare,

        /// <summary>
        /// Status register
        /// </summary>
        SR,

        /// <summary>
        /// Cause of last exception
        /// </summary>
        Cause,

        /// <summary>
        /// Exception Program Counter
        /// </summary>
        EPC,

        /// <summary>
        /// Processor Revision Identifier
        /// </summary>
        PRId,

        /// <summary>
        /// Configuration register
        /// </summary>
        Config,

        /// <summary>
        /// Load Linked Address
        /// </summary>
        LLAddr,

        /// <summary>
        /// Memory reference trap address low bits
        /// </summary>
        WatchLo,

        /// <summary>
        /// Memory reference trap address high bits
        /// </summary>
        WatchHi,

        /// <summary>
        /// Pointer to kernel virtual PTE table in 64-bit addressing mode
        /// </summary>
        XContext,

        /// <summary>
        /// Secondary-cache error checking and correcting (ECC) and Primary parity
        /// </summary>
        ECC = 26,

        /// <summary>
        /// Cache Error and Status register
        /// </summary>
        CacheErr,

        /// <summary>
        /// Cache Tag register
        /// </summary>
        TagLo,

        /// <summary>
        /// Cache Tag register
        /// </summary>
        TagHi,

        /// <summary>
        /// Error Exception Program Counter
        /// </summary>
        ErrorEPC,
    };

    [Serializable]
    public sealed class CP0Registers : IEnumerable<CP0RegName>
    {
        private UInt64[] m_Regs;
        private StatusRegister m_SR;
        private CauseRegister m_CauseReg;
        private UInt64 m_Condition;
        private UInt32 m_Count;
        private UInt32 m_Compare;

        public CP0Registers()
        {
            m_Regs = new UInt64[32];
            m_SR = new StatusRegister();
            m_CauseReg = new CauseRegister();
        }

        public UInt64 this[CP0RegName index]
        {
            get
            {
                switch (index)
                {
                    case CP0RegName.BadVAddr: return m_Regs[8];
                    case CP0RegName.EntryHi: return m_Regs[10];
                    case CP0RegName.EntryLo0: return m_Regs[2];
                    case CP0RegName.EntryLo1: return m_Regs[3];
                    case CP0RegName.Index: return m_Regs[0];
                    case CP0RegName.PageMask: return m_Regs[5];
                    case CP0RegName.Random: return m_Regs[1];
                    case CP0RegName.Wired: return m_Regs[6];
                    case CP0RegName.SR: return m_Regs[12];
                    case CP0RegName.Cause: return m_CauseReg.RegisterValue64;
                    default: return m_Regs[(int)index];
                }
            }

            set
            {
                switch (index)
                {
                    case CP0RegName.BadVAddr: m_Regs[8] = value; break;
                    case CP0RegName.EntryHi: m_Regs[10] = value; break;
                    case CP0RegName.EntryLo0: m_Regs[2] = value; break;
                    case CP0RegName.EntryLo1: m_Regs[3] = value; break;
                    case CP0RegName.Index: m_Regs[0] = value; break;
                    case CP0RegName.PageMask: m_Regs[5] = value; break;
                    case CP0RegName.Wired: m_Regs[6] = value; break;
                    case CP0RegName.Random: m_Regs[1] = value; break;
                    case CP0RegName.SR: m_Regs[12] = value; m_SR.RegisterValue64 = value; break;
                    case CP0RegName.Cause: m_CauseReg.RegisterValue64 = value; break;
                    default: m_Regs[(int)index] = value; break;
                }
            }
        }

        public UInt64 this[Int32 index]
        {
            get { return m_Regs[index]; }
            set { m_Regs[index] = value; }
        }

        public IEnumerator<CP0RegName> GetEnumerator()
        {
            foreach (var index in Enum.GetValues(typeof(CP0RegName)))
            {
                yield return (CP0RegName)index;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public RingMode GetCurrentRingMode(out WordSize size)
        {
            size = WordSize.MIPS32;
            return RingMode.User;
        }

        public StatusRegister StatusReg
        {
            get { return m_SR; }
        }

        public CauseRegister CauseReg
        {
            get { return m_CauseReg; }
        }



        public void Clear()
        {
            Array.Clear(m_Regs, 0, m_Regs.Length);
            m_SR.RegisterValue = 0;
            m_CauseReg.RegisterValue = 0;
        }

        public UInt64 Index
        {
            get { return this[CP0RegName.Index]; }
            set { this[CP0RegName.Index] = value; }
        }

        public UInt64 Random
        {
            get { return this[CP0RegName.Random]; }
        }

        public UInt64 EntryLo0
        {
            get { return this[CP0RegName.EntryLo0]; }
            set { this[CP0RegName.EntryLo0] = value; }
        }

        public UInt64 EntryLo1
        {
            get { return this[CP0RegName.EntryLo1]; }
            set { this[CP0RegName.EntryLo1] = value; }
        }

        public UInt64 Context
        {
            get { return this[CP0RegName.Context]; }
            set { this[CP0RegName.Context] = value; }
        }

        public UInt64 PageMask
        {
            get { return this[CP0RegName.PageMask]; }
            set { this[CP0RegName.PageMask] = value; }
        }

        public UInt64 Wired
        {
            get { return this[CP0RegName.Wired]; }
            set { this[CP0RegName.Wired] = value; }
        }

        public UInt64 BadVAddr
        {
            get { return this[CP0RegName.BadVAddr]; }
        }

        public UInt32 Count
        {
            get { return m_Count; }
            set { m_Count = value; }
        }

        public UInt64 EntryHi
        {
            get { return this[CP0RegName.EntryHi]; }
            set { this[CP0RegName.EntryHi] = value; }
        }

        public UInt32 Compare
        {
            get { return m_Compare; }
            set { m_Compare = value; m_Count = 0; }
        }

        public UInt64 SR
        {
            get { return this[CP0RegName.SR]; }
            set { this[CP0RegName.SR] = value; }
        }

        public UInt64 Cause
        {
            get { return m_CauseReg.RegisterValue64; }
            set { m_CauseReg.RegisterValue64 = value; }
        }

        public UInt64 Status
        {
            get { return m_SR.RegisterValue64; }
            set { m_SR.RegisterValue64 = value; }
        }

        public UInt64 EPC
        {
            get { return this[CP0RegName.EPC]; }
            set { this[CP0RegName.EPC] = value; }
        }

        public UInt64 PRId
        {
            get { return this[CP0RegName.PRId]; }
            set { this[CP0RegName.PRId] = value; }
        }

        public UInt64 Config
        {
            get { return this[CP0RegName.Config]; }
            set { this[CP0RegName.Config] = value; }
        }

        public UInt64 LLAddr
        {
            get { return this[CP0RegName.LLAddr]; }
            set { this[CP0RegName.LLAddr] = value; }
        }

        public UInt64 WatchLo
        {
            get { return this[CP0RegName.WatchLo]; }
            set { this[CP0RegName.WatchLo] = value; }
        }

        public UInt64 WatchHi
        {
            get { return this[CP0RegName.WatchHi]; }
            set { this[CP0RegName.WatchHi] = value; }
        }

        public UInt64 XContext
        {
            get { return this[CP0RegName.XContext]; }
            set { this[CP0RegName.XContext] = value; }
        }

        public UInt64 ECC
        {
            get { return this[CP0RegName.ECC]; }
            set { this[CP0RegName.ECC] = value; }
        }

        public UInt64 CacheErr
        {
            get { return this[CP0RegName.CacheErr]; }
            set { this[CP0RegName.CacheErr] = value; }
        }

        public UInt64 TagLo
        {
            get { return this[CP0RegName.TagLo]; }
            set { this[CP0RegName.TagLo] = value; }
        }

        public UInt64 TagHi
        {
            get { return this[CP0RegName.TagHi]; }
            set { this[CP0RegName.TagHi] = value; }
        }

        public UInt64 ErrorEPC
        {
            get { return this[CP0RegName.ErrorEPC]; }
            set { this[CP0RegName.ErrorEPC] = value; }
        }

        public UInt64 Condition
        {
            get { return m_Condition; }
            set { m_Condition = value; }
        }
    }
}