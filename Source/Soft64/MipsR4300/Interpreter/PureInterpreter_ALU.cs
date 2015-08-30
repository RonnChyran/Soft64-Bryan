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
using System.Numerics;

namespace Soft64.MipsR4300.Interpreter
{
    public partial class PureInterpreter
    {
        [OpcodeHook("SLT")]
        private void Inst_Slt(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                if (MipsState.ReadGPR32Signed(inst.Rs) < MipsState.ReadGPR32Signed(inst.Rt))
                {
                    MipsState.GPRRegs[inst.Rd] = 1;
                }
                else
                {
                    MipsState.GPRRegs[inst.Rd] = 0;
                }
            }
            else
            {
                if (MipsState.ReadGPRSigned(inst.Rs) < MipsState.ReadGPRSigned(inst.Rt))
                {
                    MipsState.GPRRegs[inst.Rd] = 1;
                }
                else
                {
                    MipsState.GPRRegs[inst.Rd] = 0;
                }
            }
        }

        [OpcodeHook("SLTU")]
        private void Inst_Sltu(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                if (MipsState.ReadGPR32Unsigned(inst.Rs) < MipsState.ReadGPR32Unsigned(inst.Rt))
                {
                    MipsState.GPRRegs[inst.Rd] = 1;
                }
                else
                {
                    MipsState.GPRRegs[inst.Rd] = 0;
                }
            }
            else
            {
                if (MipsState.ReadGPRUnsigned(inst.Rs) < MipsState.ReadGPRUnsigned(inst.Rt))
                {
                    MipsState.GPRRegs[inst.Rd] = 1;
                }
                else
                {
                    MipsState.GPRRegs[inst.Rd] = 0;
                }
            }
        }

        [OpcodeHook("ADDIU")]
        private void Inst_Addiu(MipsInstruction inst)
        {
            unchecked
            {
                if (MipsState.Is32BitMode())
                {
                    MipsState.WriteGPR32Unsigned(inst.Rt, MipsState.ReadGPR32Unsigned(inst.Rs) + (UInt32)(Int32)(Int16)inst.Immediate);
                }
                else
                {
                    if (MipsState.ReadGPRUnsigned(inst.Rs).IsSigned32())
                    {
                        MipsState.WriteGPRUnsigned(inst.Rt, MipsState.ReadGPRUnsigned(inst.Rs) + (UInt64)(Int64)(Int16)inst.Immediate);
                    }
                }
            }
        }

        [OpcodeHook("ADDU")]
        private void Inst_Addu(MipsInstruction inst)
        {
            unchecked
            {
                if (MipsState.Is32BitMode())
                {
                    MipsState.WriteGPR32Unsigned(inst.Rd, MipsState.ReadGPR32Unsigned(inst.Rs) + MipsState.ReadGPR32Unsigned(inst.Rt));
                }
                else
                {
                    if (MipsState.ReadGPRUnsigned(inst.Rs).IsSigned32() && MipsState.ReadGPRUnsigned(inst.Rt).IsSigned32())
                    {
                        MipsState.WriteGPRUnsigned(inst.Rd, MipsState.ReadGPRUnsigned(inst.Rs) + MipsState.ReadGPRUnsigned(inst.Rt));
                    }
                }
            }
        }

        [OpcodeHook("ADD")]
        private void Inst_Add(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                try
                {
                    MipsState.WriteGPR32Signed(inst.Rd, MipsState.ReadGPR32Signed(inst.Rs) + MipsState.ReadGPR32Signed(inst.Rt));
                }
                catch (OverflowException)
                {
                    MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.OverFlow;
                }
                
            }
            else
            {
                if (MipsState.ReadGPRUnsigned(inst.Rs).IsSigned32() && MipsState.ReadGPRUnsigned(inst.Rt).IsSigned32())
                {
                    try
                    {
                        MipsState.WriteGPRSigned(inst.Rd, MipsState.ReadGPRSigned(inst.Rs) + MipsState.ReadGPRSigned(inst.Rt));
                    }
                    catch (OverflowException)
                    {
                        MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.OverFlow;
                    }
                }
            }
        }

        [OpcodeHook("ADDI")]
        private void Inst_Addi(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                try
                {
                    MipsState.WriteGPR32Signed(inst.Rt, MipsState.ReadGPR32Signed(inst.Rs) + (Int32)(Int16)inst.Immediate);
                }
                catch (OverflowException)
                {
                    MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.OverFlow;
                }
            }
            else
            {
                try
                {
                    MipsState.WriteGPRSigned(inst.Rt, MipsState.ReadGPRSigned(inst.Rs) + (Int64)(Int16)inst.Immediate);
                }
                catch (OverflowException)
                {
                    MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.OverFlow;
                }
            }
        }

        [OpcodeHook("DADD")]
        private void Inst_Dadd(MipsInstruction inst)
        {
            if (MipsState.Is64BitMode())
            {
                try
                {
                    MipsState.WriteGPRSigned(inst.Rd, MipsState.ReadGPRSigned(inst.Rs) + MipsState.ReadGPRSigned(inst.Rt));
                }
                catch (OverflowException)
                {
                    MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.OverFlow;
                }
            }
            else
            {
                MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.ReservedInstruction;
            }
        }

        [OpcodeHook("DADDI")]
        private void Inst_Dadid(MipsInstruction inst)
        {
            if (MipsState.Is64BitMode())
            {
                try
                {
                    MipsState.WriteGPRSigned(inst.Rd, MipsState.ReadGPRSigned(inst.Rs) + (Int64)(Int16)inst.Immediate);
                }
                catch (OverflowException)
                {
                    MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.OverFlow;
                }
            }
            else
            {
                MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.ReservedInstruction;
            }
        }

        [OpcodeHook("DADDU")]
        private void Inst_Daddu(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.ReservedInstruction;
            }
            else
            {
                unchecked
                {
                    MipsState.WriteGPRUnsigned(inst.Rd, MipsState.ReadGPRUnsigned(inst.Rs) + MipsState.ReadGPRUnsigned(inst.Rt));
                }
            }
        }

        [OpcodeHook("DADDIU")]
        private void Inst_Daddiu(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.ReservedInstruction;
            }
            else
            {
                unchecked
                {
                    MipsState.WriteGPRUnsigned(inst.Rd, MipsState.ReadGPRUnsigned(inst.Rs) + (UInt64)(Int64)(Int16)inst.Immediate);
                }
            }
        }


        [OpcodeHook("ORI")]
        private void Inst_Ori(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
                MipsState.WriteGPR32Unsigned(inst.Rt, MipsState.ReadGPR32Unsigned(inst.Rs) | (UInt32)inst.Immediate);
            else
                MipsState.WriteGPRUnsigned(inst.Rt, MipsState.ReadGPRUnsigned(inst.Rs) | (UInt64)inst.Immediate);
        }

        [OpcodeHook("OR")]
        private void Inst_Or(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
                MipsState.WriteGPR32Unsigned(inst.Rd, MipsState.ReadGPR32Unsigned(inst.Rs) | MipsState.ReadGPR32Unsigned(inst.Rt));
            else
                MipsState.WriteGPRUnsigned(inst.Rd, MipsState.ReadGPRUnsigned(inst.Rs) | MipsState.ReadGPR32Unsigned(inst.Rt));
        }

        [OpcodeHook("SLTI")]
        private void Inst_Slti(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                Boolean condition = MipsState.ReadGPR32Signed(inst.Rs) < ((Int32)(Int16)inst.Immediate);
                MipsState.WriteGPR32Unsigned(inst.Rt, condition ? 1U : 0U);
            }
            else
            {
                Boolean condition = MipsState.ReadGPRSigned(inst.Rs) < ((Int64)(Int16)inst.Immediate);
                MipsState.WriteGPRUnsigned(inst.Rt, condition ? 1UL : 0UL);
            }
        }

        [OpcodeHook("ANDI")]
        private void Inst_Andi(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                MipsState.WriteGPR32Unsigned(inst.Rt, MipsState.ReadGPR32Unsigned(inst.Rs) & (UInt32)inst.Immediate);
            }
            else
            {
                MipsState.WriteGPRUnsigned(inst.Rt, MipsState.ReadGPRUnsigned(inst.Rs) & (UInt64)inst.Immediate);
            }
        }

        [OpcodeHook("AND")]
        private void Inst_And(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                MipsState.WriteGPR32Unsigned(inst.Rd, MipsState.ReadGPR32Unsigned(inst.Rs) & MipsState.ReadGPR32Unsigned(inst.Rt));
            }
            else
            {
                MipsState.WriteGPRUnsigned(inst.Rd, MipsState.ReadGPRUnsigned(inst.Rs) & MipsState.ReadGPR32Unsigned(inst.Rt));
            }
        }

        [OpcodeHook("XORI")]
        private void Inst_Xori(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                MipsState.WriteGPR32Unsigned(inst.Rt, MipsState.ReadGPR32Unsigned(inst.Rs) ^ (UInt32)inst.Immediate);
            }
            else
            {
                MipsState.WriteGPRUnsigned(inst.Rt, MipsState.ReadGPRUnsigned(inst.Rs) ^ (UInt64)inst.Immediate);
            }
        }

        [OpcodeHook("SLL")]
        private void Inst_Sll(MipsInstruction inst)
        {
            UInt32 result = MipsState.ReadGPR32Unsigned(inst.Rt) << inst.ShiftAmount;

            if (MipsState.Is32BitMode())
            {
                MipsState.WriteGPR32Unsigned(inst.Rd, result);
            }
            else
            {
                MipsState.WriteGPRUnsigned(inst.Rd, (UInt64)(Int64)(Int16)result);
            }
        }

        [OpcodeHook("SRL")]
        private void Inst_Srl(MipsInstruction inst)
        {
            UInt32 result = MipsState.ReadGPR32Unsigned(inst.Rt) >> inst.ShiftAmount;

            if (MipsState.Is32BitMode())
            {
                MipsState.WriteGPR32Unsigned(inst.Rd, result);
            }
            else
            {
                MipsState.WriteGPRUnsigned(inst.Rd, (UInt64)(Int64)(Int16)result);
            }
        }

        [OpcodeHook("DDIV")]
        private void Inst_Ddiv(MipsInstruction inst)
        {
            if (MipsState.Is64BitMode())
            {
                try
                {
                    MipsState.Hi = (UInt64)(MipsState.ReadGPRSigned(inst.Rs) / MipsState.ReadGPRSigned(inst.Rt));
                    MipsState.Lo = (UInt64)(MipsState.ReadGPRSigned(inst.Rs) % MipsState.ReadGPRSigned(inst.Rt));
                }
                catch (OverflowException)
                {
                    MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.OverFlow;
                }
            }
            else
            {
                MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.ReservedInstruction;
            }
        }

        [OpcodeHook("DDIVU")]
        private void Inst_Ddivu(MipsInstruction inst)
        {
            if (MipsState.Is64BitMode())
            {
                unchecked
                {
                    MipsState.Hi = (UInt64)(MipsState.ReadGPRSigned(inst.Rs) / MipsState.ReadGPRSigned(inst.Rt));
                    MipsState.Lo = (UInt64)(MipsState.ReadGPRSigned(inst.Rs) % MipsState.ReadGPRSigned(inst.Rt));
                }
            }
            else
            {
                MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.ReservedInstruction;
            }
        }

        [OpcodeHook("DIV")]
        private void Inst_Div(MipsInstruction inst)
        {
            try
            {
                unchecked
                {
                    if (MipsState.Is32BitMode() || (MipsState.ReadGPRUnsigned(inst.Rs).IsSigned32() && MipsState.ReadGPRUnsigned(inst.Rt).IsSigned32()))
                    {
                        /* Data input is always the size of a MIPS word */
                        MipsState.Hi = (UInt64)(MipsState.ReadGPR32Signed(inst.Rs) / MipsState.ReadGPR32Signed(inst.Rt));
                        MipsState.Lo = (UInt64)(MipsState.ReadGPR32Signed(inst.Rs) % MipsState.ReadGPR32Signed(inst.Rt));
                    }
                }
            }
            catch (DivideByZeroException)
            {
                MipsState.Hi = 0;
                MipsState.Lo = 0;
                return;
            }
        }

        [OpcodeHook("DIVU")]
        private void Inst_Divu(MipsInstruction inst)
        {
            try
            {
                unchecked
                {
                    if (MipsState.Is32BitMode() || (MipsState.ReadGPRUnsigned(inst.Rs).IsSigned32() && MipsState.ReadGPRUnsigned(inst.Rt).IsSigned32()))
                    {
                        /* Data input is always the size of a MIPS word */
                        MipsState.Hi = (UInt64)(MipsState.ReadGPR32Unsigned(inst.Rs) / MipsState.ReadGPR32Unsigned(inst.Rt));
                        MipsState.Lo = (UInt64)(MipsState.ReadGPR32Unsigned(inst.Rs) % MipsState.ReadGPR32Unsigned(inst.Rt));
                    }
                }
            }
            catch (DivideByZeroException)
            {
                MipsState.Hi = 0;
                MipsState.Lo = 0;
                return;
            }
        }

        [OpcodeHook("DMULT")]
        private void Inst_Dmult(MipsInstruction inst)
        {
            if (MipsState.Is64BitMode())
            {
                BigInteger left = new BigInteger(MipsState.ReadGPRSigned(inst.Rs));
                BigInteger right = new BigInteger(MipsState.ReadGPRSigned(inst.Rt));
                BigInteger product = BigInteger.Multiply(left, right);

                Byte[] productBytes = product.ToByteArray();

                MipsState.Lo = (UInt64)(
                    productBytes[0] |
                    productBytes[1] << 8 |
                    productBytes[2] << 16 |
                    productBytes[3] << 24);

                MipsState.Hi = (UInt64)(
                    productBytes[4] |
                    productBytes[5] << 8 |
                    productBytes[6] << 16 |
                    productBytes[7] << 24);
            }
            else
            {
                MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.ReservedInstruction;
            }
        }

        [OpcodeHook("DMULTU")]
        private void Inst_Dmultu(MipsInstruction inst)
        {
            if (MipsState.Is64BitMode())
            {
                BigInteger left = new BigInteger(MipsState.ReadGPRUnsigned(inst.Rs));
                BigInteger right = new BigInteger(MipsState.ReadGPRUnsigned(inst.Rt));
                BigInteger product = BigInteger.Multiply(left, right);

                Byte[] productBytes = product.ToByteArray();

                MipsState.Lo = (UInt64)(
                    productBytes[0] |
                    productBytes[1] << 8 |
                    productBytes[2] << 16 |
                    productBytes[3] << 24);

                MipsState.Hi = (UInt64)(
                    productBytes[4] |
                    productBytes[5] << 8 |
                    productBytes[6] << 16 |
                    productBytes[7] << 24);
            }
            else
            {
                MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.ReservedInstruction;
            }
        }

        [OpcodeHook("DSLL")]
        private void Inst_Dsll(MipsInstruction inst)
        {
            if (MipsState.Is64BitMode())
            {
                MipsState.WriteGPRUnsigned(inst.Rs, MipsState.ReadGPRUnsigned(inst.Rt) << inst.ShiftAmount);
            }
            else
            {
                MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.ReservedInstruction;
            }
        }

        [OpcodeHook("DSLLV")]
        private void Inst_Dsllv(MipsInstruction inst)
        {
            if (MipsState.Is64BitMode())
            {
                MipsState.WriteGPRUnsigned(inst.Rd, MipsState.ReadGPRUnsigned(inst.Rt) << (Int32)(MipsState.ReadGPR32Unsigned(inst.Rs) & 0x3FUL));
            }
            else
            {
                MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.ReservedInstruction;
            }
        }

        [OpcodeHook("DSLL32")]
        private void Inst_Dsll32(MipsInstruction inst)
        {
            if (MipsState.Is64BitMode())
            {
                MipsState.WriteGPRUnsigned(inst.Rd, MipsState.ReadGPRUnsigned(inst.Rt) << (32 + inst.ShiftAmount));
            }
        }
    }
}