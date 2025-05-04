using Bounteous.Data.Domain;

namespace Bounteous.Data.SqlServer.Tests.Domain;

public class TestEntity : IAuditable
{
    public Guid Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public DateTime SynchronizedOn { get; set; }
    public Guid? ModifiedBy { get; set; }
    public int Version { get; set; }
    public bool IsDeleted { get; set; }
}