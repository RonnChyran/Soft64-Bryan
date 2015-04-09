﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Soft64.Engines;

namespace Soft64Binding.WPF
{
    public class EmulatorEngineViewModel : MachineComponentViewModel
    {
        public EmulatorEngineViewModel(MachineViewModel machineModel) : base(machineModel)
        {
            Status = machineModel.TargetMachine.CurrentEngine.Status;

            WeakEventManager<EmulatorEngine, EngineStatusChangedArgs>.AddHandler(
                machineModel.TargetMachine.CurrentEngine,
                "EngineStatusChanged",
                EngineStatusChangedHandler);
        }

        private void EngineStatusChangedHandler(Object sender, EngineStatusChangedArgs e)
        {
            Status = e.NewStatus;
        }

        private static readonly DependencyPropertyKey StatusPropertyKey =
            DependencyProperty.RegisterReadOnly("Status", typeof(EngineStatus), typeof(EmulatorEngineViewModel),
            new PropertyMetadata());

        public static readonly DependencyProperty StatusProperty =
            StatusPropertyKey.DependencyProperty;

        public EngineStatus Status
        {
            get { return (EngineStatus)GetValue(StatusProperty); }
            private set { SetValue(StatusPropertyKey, value); }
        }
    }
}
