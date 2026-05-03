using System.Text.Json.Serialization;

namespace DeCloud.Shared.Models;

/// <summary>
/// First-class structured metadata declaring one variable used by a VM template.
///
/// <para>
/// Templates carry a <c>List&lt;TemplateVariable&gt;</c> rather than scattering
/// variable knowledge across renderers, resolvers, and node-side code. The
/// list is the single source of truth that drives:
/// <list type="number">
///   <item>render-time substitution (statics → <c>__VARNAME__</c> placeholders)</item>
///   <item>the node-local environment endpoint (dynamics → <c>$VARNAME</c> shell-source)</item>
///   <item>the in-VM watcher's scope policy (dynamics' <see cref="Scope"/>)</item>
/// </list>
/// </para>
///
/// <para>
/// <b>Field interactions:</b>
/// <list type="bullet">
///   <item><see cref="Scope"/> is meaningful only when <see cref="Kind"/> is
///     <see cref="VariableKind.Dynamic"/>. Ignored for statics.</item>
///   <item><see cref="DefaultValue"/> and <see cref="Required"/> are meaningful
///     only for statics. Dynamics are always platform-bound, never user-supplied.</item>
///   <item><see cref="ResolverKey"/> defaults to <see cref="Name"/>. Override
///     when a template variable should delegate to a differently-named platform
///     resolver (e.g., a tenant template might declare <c>EGRESS_IP</c> resolved
///     by the platform's <c>NodePublicIp</c> resolver).</item>
/// </list>
/// </para>
/// </summary>
public sealed class TemplateVariable
{
    /// <summary>
    /// The variable name as it appears in the cloud-init template.
    /// For statics: appears as <c>__NAME__</c> placeholder.
    /// For dynamics: appears as <c>$NAME</c> shell variable reference.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Resolution time: <see cref="VariableKind.Static"/> resolves at render
    /// time and is substituted into the cloud-init bytes;
    /// <see cref="VariableKind.Dynamic"/> resolves at runtime via the
    /// environment endpoint.
    /// </summary>
    public required VariableKind Kind { get; init; }

    /// <summary>
    /// Watcher reaction policy when this variable's value changes at runtime.
    /// Meaningful only for <see cref="VariableKind.Dynamic"/>; ignored for
    /// statics (which can't change without redeploying the VM).
    /// Default <see cref="WatcherScope.Restart"/> is the conservative choice
    /// when scope hasn't been audited.
    /// </summary>
    public WatcherScope Scope { get; init; } = WatcherScope.Restart;

    /// <summary>
    /// Default value for statics when no user input is provided.
    /// Null for variables without defaults. Meaningless for dynamics.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// True if a static variable must have a value at render time (no
    /// silent fallback to empty string). Meaningless for dynamics.
    /// </summary>
    public bool Required { get; init; }

    /// <summary>
    /// Optional human-readable description, surfaced in tenant-facing UIs
    /// and developer documentation.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional override of the resolver key used to look up the value.
    /// Defaults to <see cref="Name"/>. Use when a template needs to bind
    /// a variable to a platform resolver with a different canonical name.
    /// </summary>
    public string? ResolverKey { get; init; }
}

/// <summary>
/// When a variable's value is bound.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VariableKind
{
    /// <summary>
    /// Resolved at render time; substituted into cloud-init bytes.
    /// </summary>
    Static,

    /// <summary>
    /// Resolved at runtime via the node-local environment endpoint.
    /// </summary>
    Dynamic,
}

/// <summary>
/// In-VM watcher's reaction when a Dynamic variable's value changes.
/// The watcher folds across all changed variables in a poll cycle and
/// applies the maximum scope (Noop &lt; Reload &lt; Restart).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WatcherScope
{
    /// <summary>
    /// Refresh the env file but take no service action. The next service
    /// restart (for any reason) will pick up the new value.
    /// </summary>
    Noop,

    /// <summary>
    /// Send SIGHUP to the role service. The service must understand SIGHUP
    /// as "reload config without dropping in-flight work".
    /// </summary>
    Reload,

    /// <summary>
    /// Restart the role service. In-flight work is dropped. For mesh-participant
    /// roles, the watcher additionally bounces <c>wg-quick@wg-mesh</c> when any
    /// <c>WG_*</c> variable changed (see <c>decloud-env-watcher.sh</c>).
    /// </summary>
    Restart,
}
