namespace DeCloud.Shared.Enums
{
    /// <summary>
    /// How a workload is deployed on a node.
    /// VirtualMachine: KVM/QEMU VM via libvirt (default, full isolation)
    /// Container: Docker container with GPU sharing (for nodes without IOMMU, e.g. WSL2)
    /// </summary>
    public enum DeploymentMode
    {
        VirtualMachine = 0,
        Container = 1
    }
}
