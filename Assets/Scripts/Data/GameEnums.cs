namespace FieldFriends.Data
{
    public enum CreatureType
    {
        Field,
        Wind,
        Water,
        Meadow
    }

    public enum AreaID
    {
        WillowEnd,
        SouthField,
        CreekPath,
        HillRoad,
        Stonebridge,
        NorthMeadow,
        QuietGrove
    }

    public enum GameState
    {
        Overworld,
        Battle,
        Paused,
        Dialogue,
        Transition
    }

    public enum BattleAction
    {
        Move,
        Wait,
        Back
    }

    public enum EncounterRarity
    {
        Common,
        Uncommon,
        Rare
    }

    public enum CreatureState
    {
        Active,
        Resting // KO state -- never called "faint"
    }
}
