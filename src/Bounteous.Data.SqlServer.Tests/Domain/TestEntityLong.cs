using Bounteous.Data.Domain;

namespace Bounteous.Data.SqlServer.Tests.Domain;

public class TestEntityLong : IAuditable<long, long>
{
    public long Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public long CreatedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public DateTime SynchronizedOn { get; set; }
    public long ModifiedBy { get; set; }
    public int Version { get; set; }
    public bool IsDeleted { get; set; }
}
