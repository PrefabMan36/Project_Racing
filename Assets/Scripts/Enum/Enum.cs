public enum eSCENE
{
    eSCENE_TITLE = 0,
    eSCENE_LOADING = 1,
    eSCENE_MAINMENU = 2,
    eSCENE_MAINGAME = 3,
    eSCENE_RESULT = 4,
    eSCENE_LOBBY = 5,
    eSCENE_CITY_NIGHT = 6,
    eSCENE_MOUNT_TRACK = 7
}

public enum eCAR_COLOR
{
    Black,
    Black_tuned,
    Blue,
    Blue_tuned,
    BlueDark,
    BlueDark_tuned,
    Green,
    Green_tuned,
    GreenDark,
    GreenDark_tuned,
    Grey,
    Grey_tuned,
    Red,
    Red_tuned,
    RedDark,
    RedDark_tuned,
    White,
    White_tuned,
    Yellow,
    Yellow_tuned
}

public enum eCAR_DRIVEAXEL
{
    eFWD,
    eRWD,
    e4WD
}

public enum eUI_TYPE
{
    YES,
    NO,
    EXIT,
    PREVIOUS,
    SETTING,
    HOST,
    JOIN,
    MAINBAR,
    BOTTOMBAR,
    PROFILESETTING,
    AUDIOSETTING,
    CREATEROOM,
    JOINROOM,
    READY,
    LEAVESESSION,
    RACESTART,
    NULL
}

public enum ePOPUP_TYPE
{
    EXIT
}

public enum eCONNECTIONSTATUS
{
    DISCONNECTED,
    CONNECTING,
    FAILED,
    CONNECTED
}

public enum eGAMESTATE
{
    LOBBY,
    GAMECUTSCENE,
    GAMEREADY
}