[System.Serializable]
public class Character
{
    private string fileName;
    private string body;
    private string texture;
    private string action_idle;
    private string action_run;
    private string action_attack;

    public string FileName { get => fileName; set => fileName = value; }
    public string Body { get => body; set => body = value; }
    public string Texture { get => texture; set => texture = value; }
    public string Action_idle { get => action_idle; set => action_idle = value; }
    public string Action_run { get => action_run; set => action_run = value; }
    public string Action_attack { get => action_attack; set => action_attack = value; }

    public Character()
    {
        FileName = Constants.none;
        Body = Constants.none;
        Texture = Constants.none;
        Action_idle = Constants.none;
        Action_run = Constants.none;
        Action_attack = Constants.none;
    }

    public Character(string fileName, string body, string texture, string action_idle, string action_run, string action_attack)
    {
        this.FileName = fileName;
        this.Body = body;
        this.Texture = texture;
        this.Action_idle = action_idle;
        this.Action_run = action_run;
        this.Action_attack = action_attack;
    }
}
