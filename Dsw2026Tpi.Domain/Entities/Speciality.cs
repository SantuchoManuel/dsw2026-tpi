namespace Dsw2026Tpi.Domain.Entities;

public class Speciality: EntityBase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; private set; }
    #region Constructor for EF
#pragma warning disable CS8618
    private Speciality() { }
#pragma warning restore CS8618
    #endregion

    public Speciality(string name, string description, Guid? id = null) : base(id)
    {
        Name = name;
        Description = description;
        IsDeleted = false;
    }

    public void EstablecerDeleted()
    {
        IsDeleted = true;
    }
}
