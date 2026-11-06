namespace CustomerSync.Core.Models;

public enum DiscrepancyType
{
    InvalidRecord,
    DuplicateInSource,
    OrphanedInTarget
}