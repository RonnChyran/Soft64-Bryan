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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NLog;
using Soft64.IO;
using Soft64.MipsR4300;
using MipsOp = System.Action<Soft64.MipsR4300.MipsInstruction>;

namespace Soft64.MipsR4300
{
    /* TODO: Implement optable for fixed FPU opcodes */

    public partial class Interpreter : ExecutionEngine
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private InstructionReader m_InstReader;
        private BinaryReader m_DataBinReader;
        private BinaryWriter m_DataBinWriter;
        private ExecutionState m_CPUState;
        private Int32 m_OpcodeErrorCount = 0;
        private MipsOp m_SubSpecial;
        private MipsOp m_SubRegImm;
        private MipsOp m_COP0;
        private MipsOp m_COP1;
        private MipsOp m_BC1;
        private MipsOp m_SI;
        private MipsOp m_DI;
        private MipsOp m_WI;
        private MipsOp m_LI;
        private MipsOp m_TLB;
        private MipsOp[] m_OpsTableMain;
        private MipsOp[] m_OpsTableSpecial;
        private MipsOp[] m_OpsTableRegImm;
        private MipsOp[] m_OpsTableCP0;
        private MipsOp[] m_OpsTableCP1;
        private MipsOp[] m_OpsTableBC1;
        private MipsOp[] m_OpsTableFloat;
        private MipsOp[] m_OpsTableFixed;
        private MipsOp[] m_OpsTableTLB;

        public Interpreter()
        {
        }

        protected virtual void InitializeOpcodes()
        {
            SetupOpcodeHandlers(this);
        }

        public sealed override void Initialize()
        {
            base.Initialize();
            m_CPUState = ParentMips.State;
            m_InstReader = new InstructionReader(MemoryAccessMode.Virtual);
            m_DataBinReader = new BinaryReader(ParentMips.VirtualMemoryStream);
            m_DataBinWriter = new BinaryWriter(ParentMips.VirtualMemoryStream);
            InitializeOpcodes();
        }

        private void InitializeCallTables()
        {
            m_SubSpecial = (inst) => OpCall(m_OpsTableSpecial, inst.Function, inst);
            m_SubRegImm = (inst) => OpCall(m_OpsTableRegImm, inst.Rt, inst);
            m_TLB = (inst) => OpCall(m_OpsTableTLB, inst.Function, inst);
            m_COP0 = (inst) => OpCall(m_OpsTableCP0, inst.Rs, inst);
            m_COP1 = (inst) => OpCall(m_OpsTableCP1, inst.Rs, inst);
            m_BC1 = (inst) => OpCall(m_OpsTableBC1, inst.Rt & 0x3, inst);
            m_WI = (inst) => { inst.DataFormat = DataFormat.FixedWord; OpCall(m_OpsTableFixed, inst.Function, inst); };
            m_LI = (inst) => { inst.DataFormat = DataFormat.FixedLong; OpCall(m_OpsTableFixed, inst.Function, inst); };
            m_SI = (inst) => { inst.DataFormat = DataFormat.FloatingSingle; OpCall(m_OpsTableFloat, inst.Function, inst); };
            m_DI = (inst) => { inst.DataFormat = DataFormat.FloatingDouble; OpCall(m_OpsTableFloat, inst.Function, inst); };

            m_OpsTableMain = new MipsOp[] {
                m_SubSpecial, m_SubRegImm, J, JAL, BEQ, BNE, BLEZ, BGTZ,
                ADDI, ADDIU, SLTI, SLTIU, ANDI, ORI, XORI, LUI,
                m_COP0, m_COP1, null, null, BEQL, BNEL, BLEZL, BGTZL,
                DADDI, DADDIU, LDL, LDR, null, null, null, null,
                LB, LH, LWL, LW, LBU, LHU, LWR, LWU,
                SB, SH, SWL, SW, SDL, SDR, SWR, CACHE,
                LL, LWC1, null, null, LLD, LDC1, LDC2, LD,
                SC, SWC1, null, null, SCD, SDC1, SDC2, SD
            };

            m_OpsTableSpecial = new MipsOp[] {
                SLL, null, SRL, SRA, SLLV, null, SRLV, SRAV,
                JR, JALR, null, null, SYSCALL, BREAK, null, SYNC,
                MFHI, MTHI, MFLO, MTLO, DSLLV, null, DSRLV, DSRAV,
                MULT, MULTU, DIV, DIVU, DMULT, DMULTU, DDIV, DDIVU,
                ADD, ADDU, SUB, SUBU, AND, OR, XOR, NOR,
                null, null, SLT, SLTU, DADD, DADDU, DSUB, DSUBU,
                TGE, TGEU, TLT, TLTU, TEQ, null, TNE, null,
                DSLL, null, DSRL, DSRA, DSLL32, null, DSRL32, DSRA32
            };

            m_OpsTableRegImm = new MipsOp[] {
                BLTZ, BGEZ, BLTZL, BGEZL, null, null, null, null,
                TGEI, TGEIU, TLTI, TLTIU, TEQI, null, TNEI, null,
                BLTZAL, BGEZAL, BLTZALL, BGEZALL, null, null, null, null,
                null, null, null, null, null, null, null, null
            };

            m_OpsTableCP0 = new MipsOp[] {
                MCF0, DMFC0, null, null, MTC0, DTMC0, null, null,
                null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null
            };

            m_OpsTableTLB = new MipsOp[] {
                null, TLBR, TLBWI, null, null, null, TLBWR, null,
                TLBP, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null,
                ERET, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null
            };

            m_OpsTableCP1 = new MipsOp[] {
                MFC1, DMFC1, CFC1, null, MTC1, DMTC1, CTC1, null,
                m_BC1, null, null, null, null, null, null, null,
                m_SI, m_DI, null, null, m_WI, m_LI, null, null,
                null, null, null, null, null, null, null, null
            };

            m_OpsTableBC1 = new MipsOp[] {
                BC1F, BC1T, BC1FL, BC1TL
            };

            m_OpsTableFloat = new MipsOp[] {
                FPU_ADD, FPU_SUB, FPU_MUL, FPU_DIV, FPU_SQRT, FPU_ABS, FPU_MOV, FPU_NEG,
                FPU_ROUND_L, FPU_TRUNC_L, FPU_CEIL_L, FPU_FLOOR_L, FPU_ROUND_W, FPU_TRUNC_W, FPU_CEIL_W, FPU_FLOOR_W,
                null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null,
                FPU_CVT_S, FPU_CVT_D, null, null, FPU_CVT_W, FPU_CVT_L, null, null,
                null, null, null, null, null, null, null, null,
                FPU_C_F, FPU_C_UN, FPU_C_EQ, FPU_C_UEQ, FPU_C_UEQ, FPU_C_OLT, FPU_C_ULT, FPU_C_OLE, FPU_C_ULE,
                FPU_C_SF, FPU_C_NGLE, FPU_C_SEQ, FPU_C_NGL, FPU_C_LT, FPU_C_NGE, FPU_C_LE, FPU_C_NGT
            };
        }

        public override void Step()
        {
            DoInterpreterStep();
        }

        protected void DoInterpreterStep()
        {
            /* Do one step through the interpreter */

            try
            {
                /* Go to branch */
                if (BranchEnabled)
                    MipsState.PC = BranchTarget;

                /* Fetch instruction at PC */
                MipsInstruction inst = FetchInstruction(MipsState.PC);

                /* If we branched, execute the delay slot */
                if (BranchEnabled)
                {
                    if (!NullifyEnabled)
                    {
                        MipsState.BranchPC = BranchDelaySlot;
                        MipsInstruction bsdInst = FetchInstruction(BranchDelaySlot);
                        TraceOp(BranchDelaySlot, bsdInst);
                        CallInstruction(bsdInst);
                        BranchEnabled = false;
                    }
                    else
                    {
                        /* Skip the delay slot */
                        NullifyEnabled = false;
                        BranchEnabled = false;
                    }
                }
                else
                {

                    TraceOp(MipsState.PC, inst);
                    CallInstruction(inst);

                    /* If branching has been set, skip PC increment */
                    if (!BranchEnabled)
                        MipsState.PC += 4;
                    else
                        MipsState.PC += 8;
                }

            }
            catch (Exception e)
            {
                MipsState.PC -= 4; /* Move the counter back to the instruction that has faulted */
                HasFaulted = true;

                /* Important: This ensures that the engine thread comes to an halt seeing the thrown exception */
                throw e;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OpCall(MipsOp[] callTable, Int32 index, MipsInstruction inst)
        {
            if (inst.Instruction == 0)
                return;


            /* TODO: Clean this mess up */

#if !FAST_UNSAFE_BUILD
            try
            {
                MipsOp e = callTable[index];

                if (e != null)
                    e(inst);
                else
                    throw new InvalidOperationException("Unsupport Instruction: " + inst.ToString());
            }
            catch (OverflowException e)
            {
#if DEBUG
                Console.WriteLine("Overflow at " + e.TargetSite.Name);
#endif
                return;
            }
            catch (Exception e)
            {
                m_OpcodeErrorCount++;
                Console.WriteLine("Interpreter Opcode Error: " + e.Message);

                if (m_OpcodeErrorCount >= 1)
                    throw new InvalidOperationException("Too many opcode errors have occured");

                return;
            }
#else
            callTable[index](inst);
#endif
        }

        protected void CallInstruction(MipsInstruction inst)
        {
            OpCall(m_OpsTableMain, inst.Opcode, inst);
        }

        protected ExecutionState MipsState
        {
            get { return m_CPUState; }
        }

        protected BinaryReader DataBinaryReader
        {
            get { return m_DataBinReader; }
        }

        protected BinaryWriter DataBinaryWriter
        {
            get { return m_DataBinWriter; }
        }

        protected MipsInstruction FetchInstruction(Int64 address)
        {
            m_InstReader.Position = address;
            return m_InstReader.ReadInstruction();
        }

        protected void SetupOpcodeHandlers(Object target)
        {
            var methodQuery =
                from method in this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).AsParallel()
                let attribute = method.GetCustomAttribute<OpcodeHookAttribute>()
                where attribute != null
                select new { MethodTarget = method.CreateDelegate(typeof(Action<MipsInstruction>), this), OpName = attribute.OpcodeName };

            var opTablePropDictionary = typeof(Interpreter)
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                    .AsParallel()
                        .ToDictionary(prop => prop.Name, prop => prop);


            foreach (var m in methodQuery)
            {
                PropertyInfo propInfo = null;

                if (opTablePropDictionary.TryGetValue(m.OpName, out propInfo))
                {
                    propInfo.SetValue(target, m.MethodTarget);
                }
            }

            InitializeCallTables();
        }

        public static Int64 BranchComputeTargetAddress(Int64 pc, UInt16 immediate)
        {
            return (pc + 4) + (((Int64)(Int16)immediate) << 2);
        }

        private void DoBranch(Boolean condition, MipsInstruction inst)
        {
            BranchEnabled = true;
            BranchDelaySlot = MipsState.PC + 4;
            BranchTarget = condition ? BranchComputeTargetAddress(inst.Address, inst.Immediate).ResolveAddress() : MipsState.PC + 8;
        }

        private void DoBranchLikely(Boolean condition, MipsInstruction inst)
        {
            NullifyEnabled = !condition;
            DoBranch(condition, inst);
        }

        private void DoJump(Int64 addressTarget)
        {
            BranchEnabled = true;
            BranchDelaySlot = MipsState.PC + 4;
            BranchTarget = addressTarget.ResolveAddress();
        }

        [Conditional("DEBUG")]
        protected void TraceOp(Int64 pc, MipsInstruction inst)
        {
            if (logger.IsDebugEnabled)
                logger.Debug("{0:X8} {1:X4} {2:X4} {3}", pc, inst.Instruction >> 16, inst.Instruction & 0xFFFF, inst.ToString());
        }

        public Boolean NullifyEnabled { get; set; }

        public Boolean BranchEnabled { get; set; }

        public Int64 BranchTarget { get; set; }

        public Int64 BranchDelaySlot { get; set; }

        #region Instruction Delegates

        protected MipsOp J { get; set; }

        protected MipsOp JAL { get; set; }

        protected MipsOp BEQ { get; set; }

        protected MipsOp BNE { get; set; }

        protected MipsOp BLEZ { get; set; }

        protected MipsOp BGTZ { get; set; }

        protected MipsOp ADDI { get; set; }

        protected MipsOp ADDIU { get; set; }

        protected MipsOp SLTI { get; set; }

        protected MipsOp SLTIU { get; set; }

        protected MipsOp ANDI { get; set; }

        protected MipsOp ORI { get; set; }

        protected MipsOp XORI { get; set; }

        protected MipsOp LUI { get; set; }

        protected MipsOp BEQL { get; set; }

        protected MipsOp BNEL { get; set; }

        protected MipsOp BLEZL { get; set; }

        protected MipsOp BGTZL { get; set; }

        protected MipsOp DADDI { get; set; }

        protected MipsOp DADDIU { get; set; }

        protected MipsOp LDL { get; set; }

        protected MipsOp LDR { get; set; }

        protected MipsOp LB { get; set; }

        protected MipsOp LH { get; set; }

        protected MipsOp LWL { get; set; }

        protected MipsOp LW { get; set; }

        protected MipsOp LBU { get; set; }

        protected MipsOp LWR { get; set; }

        protected MipsOp LWU { get; set; }

        protected MipsOp SB { get; set; }

        protected MipsOp SH { get; set; }

        protected MipsOp SWL { get; set; }

        protected MipsOp SW { get; set; }

        protected MipsOp SDL { get; set; }

        protected MipsOp SDR { get; set; }

        protected MipsOp SWR { get; set; }

        protected MipsOp CACHE { get; set; }

        protected MipsOp LL { get; set; }

        protected MipsOp LWC1 { get; set; }

        protected MipsOp LLD { get; set; }

        protected MipsOp LDC1 { get; set; }

        protected MipsOp LDC2 { get; set; }

        protected MipsOp LD { get; set; }

        protected MipsOp SC { get; set; }

        protected MipsOp SWC1 { get; set; }

        protected MipsOp SCD { get; set; }

        protected MipsOp SDC1 { get; set; }

        protected MipsOp SDC2 { get; set; }

        protected MipsOp SD { get; set; }

        protected MipsOp SLL { get; set; }

        protected MipsOp SRL { get; set; }

        protected MipsOp SRA { get; set; }

        protected MipsOp SLLV { get; set; }

        protected MipsOp SRLV { get; set; }

        protected MipsOp SRAV { get; set; }

        protected MipsOp JR { get; set; }

        protected MipsOp JALR { get; set; }

        protected MipsOp SYSCALL { get; set; }

        protected MipsOp BREAK { get; set; }

        protected MipsOp SYNC { get; set; }

        protected MipsOp MFHI { get; set; }

        protected MipsOp MTHI { get; set; }

        protected MipsOp MFLO { get; set; }

        protected MipsOp MTLO { get; set; }

        protected MipsOp DSLLV { get; set; }

        protected MipsOp DSRLV { get; set; }

        protected MipsOp DSRAV { get; set; }

        protected MipsOp MULT { get; set; }

        protected MipsOp DIV { get; set; }

        protected MipsOp DIVU { get; set; }

        protected MipsOp DMULT { get; set; }

        protected MipsOp DMULTU { get; set; }

        protected MipsOp DDIV { get; set; }

        protected MipsOp DDIVU { get; set; }

        protected MipsOp ADD { get; set; }

        protected MipsOp ADDU { get; set; }

        protected MipsOp SUB { get; set; }

        protected MipsOp SUBU { get; set; }

        protected MipsOp AND { get; set; }

        protected MipsOp OR { get; set; }

        protected MipsOp XOR { get; set; }

        protected MipsOp NOR { get; set; }

        protected MipsOp SLT { get; set; }

        protected MipsOp SLTU { get; set; }

        protected MipsOp DADD { get; set; }

        protected MipsOp DADDU { get; set; }

        protected MipsOp DSUB { get; set; }

        protected MipsOp DSUBU { get; set; }

        protected MipsOp TGE { get; set; }

        protected MipsOp TGEU { get; set; }

        protected MipsOp TLT { get; set; }

        protected MipsOp TLTU { get; set; }

        protected MipsOp TEQ { get; set; }

        protected MipsOp TNE { get; set; }

        protected MipsOp DSLL { get; set; }

        protected MipsOp DSRL { get; set; }

        protected MipsOp DSRA { get; set; }

        protected MipsOp DSLL32 { get; set; }

        protected MipsOp DSRL32 { get; set; }

        protected MipsOp DSRA32 { get; set; }

        protected MipsOp BLTZ { get; set; }

        protected MipsOp BGEZ { get; set; }

        protected MipsOp BLTZL { get; set; }

        protected MipsOp BGEZL { get; set; }

        protected MipsOp TGEI { get; set; }

        protected MipsOp TGEIU { get; set; }

        protected MipsOp TLTI { get; set; }

        protected MipsOp TLTIU { get; set; }

        protected MipsOp TEQI { get; set; }

        protected MipsOp TNEI { get; set; }

        protected MipsOp BLTZAL { get; set; }

        protected MipsOp BGEZAL { get; set; }

        protected MipsOp BLTZALL { get; set; }

        protected MipsOp BGEZALL { get; set; }

        protected MipsOp MCF0 { get; set; }

        protected MipsOp MTC0 { get; set; }

        protected MipsOp TLBR { get; set; }

        protected MipsOp TLBWI { get; set; }

        protected MipsOp TLBWR { get; set; }

        protected MipsOp TLBP { get; set; }

        protected MipsOp ERET { get; set; }

        protected MipsOp MFC1 { get; set; }

        protected MipsOp DMFC1 { get; set; }

        protected MipsOp CFC1 { get; set; }

        protected MipsOp MTC1 { get; set; }

        protected MipsOp DMTC1 { get; set; }

        protected MipsOp CTC1 { get; set; }

        protected MipsOp BC1F { get; set; }

        protected MipsOp BC1T { get; set; }

        protected MipsOp BC1FL { get; set; }

        protected MipsOp BC1TL { get; set; }

        protected MipsOp FPU_ADD { get; set; }

        protected MipsOp FPU_SUB { get; set; }

        protected MipsOp FPU_MUL { get; set; }

        protected MipsOp FPU_DIV { get; set; }

        protected MipsOp FPU_SQRT { get; set; }

        protected MipsOp FPU_ABS { get; set; }

        protected MipsOp FPU_MOV { get; set; }

        protected MipsOp FPU_NEG { get; set; }

        protected MipsOp FPU_ROUND_L { get; set; }

        protected MipsOp FPU_TRUNC_L { get; set; }

        protected MipsOp FPU_CEIL_L { get; set; }

        protected MipsOp FPU_FLOOR_L { get; set; }

        protected MipsOp FPU_ROUND_W { get; set; }

        protected MipsOp FPU_TRUNC_W { get; set; }

        protected MipsOp FPU_CEIL_W { get; set; }

        protected MipsOp FPU_FLOOR_W { get; set; }

        protected MipsOp FPU_CVT_D { get; set; }

        protected MipsOp FPU_CVT_W { get; set; }

        protected MipsOp FPU_CVT_L { get; set; }

        protected MipsOp FPU_C_F { get; set; }

        protected MipsOp FPU_C_UN { get; set; }

        protected MipsOp FPU_C_EQ { get; set; }

        protected MipsOp FPU_C_UEQ { get; set; }

        protected MipsOp FPU_C_OLT { get; set; }

        protected MipsOp FPU_C_ULT { get; set; }

        protected MipsOp FPU_C_OLE { get; set; }

        protected MipsOp FPU_C_ULE { get; set; }

        protected MipsOp FPU_C_SF { get; set; }

        protected MipsOp FPU_C_NGLE { get; set; }

        protected MipsOp FPU_C_SEQ { get; set; }

        protected MipsOp FPU_C_NGL { get; set; }

        protected MipsOp FPU_C_LT { get; set; }

        protected MipsOp FPU_C_NGE { get; set; }

        protected MipsOp FPU_C_LE { get; set; }

        protected MipsOp LHU { get; set; }

        protected MipsOp MULTU { get; set; }

        protected MipsOp FPU_CVT_S { get; set; }

        protected MipsOp FPU_C_NGT { get; set; }

        protected MipsOp DMFC0 { get; set; }

        protected MipsOp DTMC0 { get; set; }

        #endregion
    }
}