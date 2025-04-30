using Fusion;
using Fusion.Addons.Physics;

public class Player_Control : NetworkBehaviour
{
    private NetworkRigidbody3D networkRigidbody;
    private Player_Car player_Car;

    private void Awake()
    {
        networkRigidbody = GetComponent<NetworkRigidbody3D>();
        player_Car = GetComponent<Player_Car>();
    }
    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputManager data))
        {
            data.direction.Normalize();
        }
    }
}
