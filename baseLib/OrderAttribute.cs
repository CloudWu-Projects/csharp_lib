[AttributeUsage(AttributeTargets.Property)]
public class OrderAttribute : Attribute
{
    public int Sequence { get; }

    public OrderAttribute(int sequence)
    {
        Sequence = sequence;
    }
}