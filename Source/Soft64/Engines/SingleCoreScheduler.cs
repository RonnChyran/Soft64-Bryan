﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Soft64.Engines
{
    /// <summary>
    /// Runs all tasks on a single thread
    /// </summary>
    /// <remarks>
    /// Warning: There should be just one self looping task after other tasks have finished, otherwise 
    /// there is no way to ensure the loop will ever end and execute the tasks after it.  The core
    /// scheduler expects the engine to instantly kill all tasks when stopping the machine.  No resources
    /// need to be cleaned up inside the scheduler because the machine the machine already implements a clean
    /// disposal pattern.
    /// </remarks>
    public sealed class SingleCoreScheduler : CoreTaskScheduler
    {
        private Thread m_SingleThread;
        private Action m_CallChain;

        public SingleCoreScheduler() : base()
        {

        }

        protected override System.Threading.Thread GetTaskThread(Task task)
        {
            Action taskAction = new Action(() =>
            {
                this.TryExecuteTask(task);
            });

            if (m_CallChain == null)
            {
                m_CallChain = taskAction;
            }
            else
            {
                MulticastDelegate.Combine(m_CallChain, taskAction);
            }

            if (m_SingleThread == null)
            {
                m_SingleThread = new Thread((o) => {
                    /* Call the chained tasks */
                    m_CallChain();
                });
            }

            return m_SingleThread;
        }

        public override void ExecuteNext()
        {
            if (m_CallChain != null)
            {
                m_CallChain();
            }
        }

        public override void Initialize()
        {


            base.Initialize();
        }
    }
}
