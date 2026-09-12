namespace Frosty.Sdk;

public class BaseTypeOverride
{
    /// <summary>
    /// Used to load the data from Original into custom fields
    /// </summary>
    public virtual void Load()
    {
    }

    /// <summary>
    /// Used to save custom field values back into Original
    /// </summary>
    public virtual void Save(object e)
    {
    }

    /// <summary>
    /// The original object that this type is used to override and add to
    /// </summary>
    public object Original;
}

public class BaseFieldOverride
{
}