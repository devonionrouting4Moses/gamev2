namespace TerminalRacer.Core.Models;

public class PowerupState
{
    public bool Active { get; set; }
    public float Remaining { get; set; }
    public float Cooldown { get; set; }
    
    public PowerupState(bool active = false, float remaining = 0, float cooldown = 0)
    {
        Active = active;
        Remaining = remaining;
        Cooldown = cooldown;
    }
}