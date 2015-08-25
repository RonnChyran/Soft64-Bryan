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

namespace Soft64.MipsR4300.Interpreter
{
    public partial class PureInterpreter
    {
        [OpcodeHook("SLT")]
        private void Inst_Slt(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                if (MipsState.GPRRegs64.GPRRegs32.GPRRegsSigned32[inst.Rs] < MipsState.GPRRegs64.GPRRegs32.GPRRegsSigned32[inst.Rt])
                {
                    MipsState.GPRRegs64[inst.Rd] = 1;
                }
                else
                {
                    MipsState.GPRRegs64[inst.Rd] = 0;
                }
            }
            else
            {
                if (MipsState.GPRRegs64.GPRRegs64S[inst.Rs] < MipsState.GPRRegs64.GPRRegs64S[inst.Rt])
                {
                    MipsState.GPRRegs64[inst.Rd] = 1;
                }
                else
                {
                    MipsState.GPRRegs64[inst.Rd] = 0;
                }
            }
        }

        [OpcodeHook("SLTU")]
        private void Inst_Sltu(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                if (MipsState.GPRRegs64.GPRRegs32[inst.Rs] < MipsState.GPRRegs64.GPRRegs32[inst.Rt])
                {
                    MipsState.GPRRegs64[inst.Rd] = 1;
                }
                else
                {
                    MipsState.GPRRegs64[inst.Rd] = 0;
                }
            }
            else
            {
                if (MipsState.GPRRegs64[inst.Rs] < MipsState.GPRRegs64[inst.Rt])
                {
                    MipsState.GPRRegs64[inst.Rd] = 1;
                }
                else
                {
                    MipsState.GPRRegs64[inst.Rd] = 0;
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
                    MipsState.GPRRegs64.GPRRegs32[inst.Rt] =
                        MipsState.GPRRegs64.GPRRegs32[inst.Rs] +
                        (UInt32)inst.Immediate.SignExtended32();
                }
                else
                {
                    if (!MipsState.GPRRegs64[inst.Rs].IsSigned32())
                        return;


                    MipsState.GPRRegs64[inst.Rt] =
                        MipsState.GPRRegs64[inst.Rs] +
                        (UInt64)inst.Immediate.SignExtended64();
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
                    UInt32 val = MipsState.GPRRegs64.GPRRegs32[inst.Rs] + MipsState.GPRRegs64.GPRRegs32[inst.Rt];
                    MipsState.GPRRegs64.GPRRegs32[inst.Rd] = val;
                }
                else
                {
                    if (!MipsState.GPRRegs64[inst.Rs].IsSigned32() ||
                        !MipsState.GPRRegs64[inst.Rt].IsSigned32())
                        return;

                    UInt64 val = MipsState.GPRRegs64[inst.Rs] + MipsState.GPRRegs64[inst.Rt];
                    MipsState.GPRRegs64[inst.Rd] = val;
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
                    Int32 val = MipsState.GPRRegs64.GPRRegs32.GPRRegsSigned32[inst.Rs] + MipsState.GPRRegs64.GPRRegs32.GPRRegsSigned32[inst.Rt];
                    MipsState.GPRRegs64.GPRRegs32.GPRRegsSigned32[inst.Rd] = val;
                }
                catch (OverflowException)
                {
                    MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.OverFlow;
                }
                
            }
            else
            {
                if (!MipsState.GPRRegs64[inst.Rs].IsSigned32() ||
                    !MipsState.GPRRegs64[inst.Rt].IsSigned32())
                    return;

                try
                {
                    Int64 val = MipsState.GPRRegs64.GPRRegs64S[inst.Rs] + MipsState.GPRRegs64.GPRRegs64S[inst.Rt];
                    MipsState.GPRRegs64.GPRRegs64S[inst.Rd] = val;
                }
                catch (OverflowException)
                {
                    MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.OverFlow;
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
                    Int32 val = MipsState.GPRRegs64.GPRRegs32.GPRRegsSigned32[inst.Rs] + inst.Immediate.SignExtended32();
                    MipsState.GPRRegs64.GPRRegs32.GPRRegsSigned32[inst.Rt] = val;
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
                    Int64 val = MipsState.GPRRegs64.GPRRegs64S[inst.Rs] + inst.Immediate.SignExtended64();
                    MipsState.GPRRegs64.GPRRegs64S[inst.Rt] = val;
                }
                catch (OverflowException)
                {
                    MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.OverFlow;
                }
            }
        }

        [OpcodeHook("DADDU")]
        private void Inst_Daddu(MipsInstruction inst)
        {
            if (!MipsState.Is64BitMode())
            {
                MipsState.CP0Regs.CauseReg.ExceptionType = CP0.ExceptionCode.ReservedInstruction;
                return;
            }

            unchecked
            {
                MipsState.GPRRegs64[inst.Rd] = MipsState.GPRRegs64[inst.Rs] + MipsState.GPRRegs64[inst.Rt];
            }
        }

        [OpcodeHook("ORI")]
        private void Inst_Ori(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
                MipsState.GPRRegs64.GPRRegs32[inst.Rt] = MipsState.GPRRegs64.GPRRegs32[inst.Rs] | inst.Immediate.ZeroExtended32();
            else
                MipsState.GPRRegs64[inst.Rt] = MipsState.GPRRegs64[inst.Rs] | inst.Immediate.ZeroExtended32();
        }

        [OpcodeHook("OR")]
        private void Inst_Or(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
                MipsState.GPRRegs64.GPRRegs32[inst.Rd] = MipsState.GPRRegs64.GPRRegs32[inst.Rt] | MipsState.GPRRegs64.GPRRegs32[inst.Rs];
            else
                MipsState.GPRRegs64[inst.Rd] = MipsState.GPRRegs64[inst.Rt] | MipsState.GPRRegs64[inst.Rs];
        }

        [OpcodeHook("SLTI")]
        private void Inst_Slti(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                MipsState.GPRRegs64.GPRRegs32[inst.Rt] = MipsState.GPRRegs64.GPRRegs32.GPRRegsSigned32[inst.Rs] < inst.Immediate.SignExtended32() ? 1U : 0U;
            }
            else
            {
                MipsState.GPRRegs64[inst.Rt] = MipsState.GPRRegs64.GPRRegs64S[inst.Rs] < inst.Immediate.SignExtended64() ? 1UL : 0UL;
            }
        }

        [OpcodeHook("ANDI")]
        private void Inst_Andi(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                MipsState.GPRRegs64.GPRRegs32[inst.Rt] = inst.Immediate.ZeroExtended32() & MipsState.GPRRegs64.GPRRegs32[inst.Rs];
            }
            else
            {
                MipsState.GPRRegs64[inst.Rt] = inst.Immediate.ZeroExtended64() & MipsState.GPRRegs64[inst.Rs];
            }
        }

        [OpcodeHook("AND")]
        private void Inst_And(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                MipsState.GPRRegs64.GPRRegs32[inst.Rd] = MipsState.GPRRegs64.GPRRegs32[inst.Rs] & MipsState.GPRRegs64.GPRRegs32[inst.Rt];
            }
            else
            {
                MipsState.GPRRegs64[inst.Rd] = MipsState.GPRRegs64[inst.Rs] & MipsState.GPRRegs64[inst.Rt];
            }
        }

        [OpcodeHook("XORI")]
        private void Inst_Xori(MipsInstruction inst)
        {
            if (MipsState.Is32BitMode())
            {
                MipsState.GPRRegs64.GPRRegs32.GPRRegsSigned32[inst.Rt] = MipsState.GPRRegs64.GPRRegs32.GPRRegsSigned32[inst.Rs] ^ inst.Immediate.SignExtended32();
            }
            else
            {
                MipsState.GPRRegs64.GPRRegs64S[inst.Rt] = MipsState.GPRRegs64.GPRRegs64S[inst.Rs] ^ inst.Immediate.SignExtended64();
            }
        }

        [OpcodeHook("SLL")]
        private void Inst_Sll(MipsInstruction inst)
        {
            UInt32 result = MipsState.GPRRegs64.GPRRegs32[inst.Rt] << inst.ShiftAmount;

            if (MipsState.Is32BitMode())
            {
                MipsState.GPRRegs64.GPRRegs32[inst.Rd] = result;
            }
            else
            {
                MipsState.GPRRegs64.GPRRegs64S[inst.Rd] = result.SignExtended64();
            }
        }

        [OpcodeHook("SRL")]
        private void Inst_Srl(MipsInstruction inst)
        {
            UInt32 result = MipsState.GPRRegs64.GPRRegs32[inst.Rt] >> inst.ShiftAmount;

            if (MipsState.Is32BitMode())
            {
                MipsState.GPRRegs64.GPRRegs32[inst.Rd] = result;
            }
            else
            {
                MipsState.GPRRegs64.GPRRegs64S[inst.Rd] = result.SignExtended64();
            }
        }
    }
}