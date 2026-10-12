public abstract class Participant
{
    public string Name { get; }

    protected Participant (string name)
    {

        Name = name.Trim();
    }

        public override string ToString()
    {
        return Name;
    }

    public virtual void NewRound()
    {
    }

}