namespace DeCloud.Shared.Enums
{
    // <summary>
    /// How GPU access is provided to a virtual machine.
    /// </summary>
    public enum GpuMode
    {
        /// <summary>No GPU access</summary>
        None = 0,

        /// <summary>
        /// VFIO passthrough: GPU bound to vfio-pci, passed as PCI hostdev to VM.
        /// Requires IOMMU enabled in BIOS/kernel. One GPU per VM, full performance.
        /// </summary>
        Passthrough = 1,

        /// <summary>
        /// GPU proxy: VM communicates with a host-side GPU proxy daemon over virtio-vsock.
        /// A CUDA shim (LD_PRELOAD) inside the VM intercepts CUDA calls and forwards them.
        /// Works without IOMMU. Multiple VMs can share one GPU.
        /// </summary>
        Proxied = 2
    }
}
