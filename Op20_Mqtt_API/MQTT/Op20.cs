public class Op20
{
    public int count {get;set;}
    public bool faulted {get;set;}
    public string? lastMessage {get;set;}

    public Op20()
    {  
        count = 0;
        faulted = false;
        lastMessage = string.Empty;
    }

    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}