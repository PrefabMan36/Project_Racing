using Fusion;
using Fusion.Addons.Physics;

public class Player_Control : NetworkBehaviour
{
    //private NetworkCharacterController characterController;
    private Player_Car player_Car;

    private void Awake()
    {
        //characterController = GetComponent<NetworkCharacterController>();
        player_Car = GetComponent<Player_Car>();
    }
    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputManager data))
        {
            data.direction.Normalize();
            //characterController.Move(5*data.direction*Runner.DeltaTime);
        }
    }
}
