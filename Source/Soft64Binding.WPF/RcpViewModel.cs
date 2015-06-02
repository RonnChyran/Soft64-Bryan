﻿using System.Windows;
using Soft64;
using Soft64.Toolkits.WPF;

namespace Soft64Binding.WPF
{
    public sealed class RcpViewModel : DependencyObject
    {
        private MachineViewModel m_MachineModel;

        internal RcpViewModel(MachineViewModel model)
        {
            m_MachineModel = model;
            Machine machine = model.TargetMachine;;
        }
    }
}