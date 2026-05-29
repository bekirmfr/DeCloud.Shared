namespace DeCloud.Shared.Enums
{
    public enum VmStatus
    {
        Pending,        // 0 - Waiting to be scheduled
        Scheduling,     // 1 - Finding a node
        Provisioning,   // 2 - Being created on node
        Running,        // 3 - Active and running
        Paused,
        Suspended,
        Stopping,       // 4 - Being stopped
        Stopped,        // 5 - Stopped but resources reserved
        Deleting,       // 6 - Deletion in progress, waiting for node confirmation
        Deleted,        // 9 - Deletion confirmed, resources freed
        Migrating,      // 7 - Being moved to another node
        Error,          // 8 - Something went wrong
    }
}