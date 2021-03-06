﻿using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Soft64.Engines
{
    [Synchronization]
    public abstract class CoreTaskScheduler : TaskScheduler
    {
        private List<Task> m_ScheduledTasks;
        private List<Thread> m_ThreadList;
        private Int32 m_PauseState;
        private const Int32 NOTSET = 0;
        private const Int32 SET = 1;
        private AutoResetEvent m_PauseEvent;
        private Boolean m_PauseNext;

        protected CoreTaskScheduler()
        {
            m_ScheduledTasks = new List<Task>();
            m_ThreadList = new List<Thread>();
            m_PauseEvent = new AutoResetEvent(false);
        }

        public virtual void Initialize()
        {
        }

        public void CleanThreads()
        {
            foreach (var thread in m_ThreadList)
            {
                if (thread.IsAlive && thread.ThreadState == ThreadState.Running)
                {
                    /* Try to kill off the threads still running with a dirty hack */
                    thread.Abort();
                    m_ThreadList.Remove(thread);
                }
            }

            GC.Collect();
        }

        private void SetPauseNext()
        {
            m_PauseNext = true;
        }

        private Boolean CheckAndSetPause()
        {
            return Interlocked.Exchange(ref m_PauseState, SET) == NOTSET;
        }

        public void PauseThreads()
        {
            if (m_PauseState == (Int32)NOTSET)
            {
                m_PauseEvent.Reset();

                if (!CheckAndSetPause())
                {
                    /* When the atomic operation has failed */
                    throw new InvalidOperationException("Cannot pause the scheduler safely");
                }
            }
        }

        public void ResumeThreads()
        {
            m_PauseState = NOTSET;
            m_PauseEvent.Set();
        }

        [SecurityCritical]
        public void RunThreads()
        {
            Boolean resume = false;

            if (m_PauseState <= 0)
            {
                PauseThreads();
                resume = true;
            }

            foreach (var task in m_ScheduledTasks)
            {
                var thread = GetTaskThread(task);

                if (thread != null && !thread.IsAlive && thread.ThreadState != ThreadState.Running)
                {
                    thread.IsBackground = true;
                    thread.Start();
                    m_ThreadList.Add(thread);
                }
            }

            if (resume)
                ResumeThreads();
        }

        internal void PauseWait()
        {
            if (m_PauseState == SET)
            {
                m_PauseEvent.WaitOne();
            }
        }

        protected Boolean PauseNext
        {
            get { return m_PauseNext; }
            set { m_PauseNext = value; }
        }

        public IEnumerable<Thread> GetThreads()
        {
            return m_ThreadList.AsReadOnly();
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return m_ScheduledTasks;
        }

        protected override void QueueTask(Task task)
        {
            m_ScheduledTasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            throw new NotImplementedException();
        }

        protected abstract Thread GetTaskThread(Task task);

        public Boolean IsPaused
        {
            get { return m_PauseState == SET; }
        }
    }
}