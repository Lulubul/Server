namespace NetworkTypes
{
    public enum Command
    {
        Connect,
        Login,
        Logout,
        Register,
        Create,
        Leave,
        Join,
        ChangeTeam,
        Lobby,
        Search,
        Start,
        Disconnect,
        Shoot,
        Die,
        Move,
        SyncRooms,
        SyncLobby,
        UpdateLobby,
        FinishAction,
        Turn,
        SyncHero,
        InitializeBoard,
        Attack,
        Defend,
        GameIsReady,
        SelectUnits,
        SendUnits
    };

    public enum Response
    {
        Succed,
        Fail
    }

    public enum Team
    {
        Red,
        Blue
    }

    public enum GameType
    {
        Classic,
        Fast
    }

    public enum State
    {
        None,
        Connect,
        Join,
        Lobby,
        Play
    }

    public enum RaceEnum { Human, Orc };
    public enum HeroType { Might, Magic }
    public enum CreatureType { Range, Melee }
    public enum CreatureStatus { Alive, Death }

}
