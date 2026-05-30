namespace DeCloud.Shared.Enums
{
    public enum VmStatus
    {
        Pending,        // 0 - Waiting to be scheduled
        Scheduling,     // 1 - Finding a node
        Provisioning,   // 2 - Being created on node
        Running,        // 3 - Active and running
        Paused,         // 4 - Paused
        Suspended,      // 5 - Suspended (admin action)
        Stopping,       // 6 - Being stopped
        Stopped,        // 7 - Stopped but resources reserved
        Deleting,       // 8 - Deletion in progress, waiting for node confirmation
        Deleted,        // 9 - Deletion confirmed, resources freed
        Migrating,      // 10 - Being moved to another node
        Error,          // 11 - Something went wrong
    }
}