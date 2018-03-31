using MICE.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace MICE.WPF.ViewModels
{
    public class MemoryViewerViewModel
    {
        private readonly NESViewModel nesVM;

        public MemoryViewerViewModel(NESViewModel nesVM)
        {
            this.nesVM = nesVM;
        }

        public IEnumerable<IMemorySegment> GetCPUMemorySegments() => this.nesVM.NES.CPUMemoryMap.GetMemorySegments();
        public IEnumerable<IMemorySegment> GetPPUMemorySegments() => this.nesVM.NES.PPU.MemoryMap.GetMemorySegments();
    }
}
