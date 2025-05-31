// Tells Unity to use the Unity.Netcode library for multiplayer networking
using Unity.Netcode;

// Unity's main class for attaching scripts to GameObjects
using UnityEngine;

// This class controls the player’s movement, and is a NetworkBehaviour (for multiplayer scripts)
public class Move : NetworkBehaviour
{
    // How fast the player moves in the scene
    public float moveSpeed = 5f;

    // A variable to store the player's input (left/right/up/down)
    private Vector2 input;

    // This is a special Netcode variable that automatically syncs its value across the network
    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();

    // This is Unity's Update() method — runs every frame (60+ times per second)
    void Update()
    {
        // If this GameObject belongs to the local client (your player)
        if (IsOwner)
        {
            // Read input from the keyboard/controller
            HandleInput();
        }

        // Update the position of this object in the scene for everyone (host + all clients)
        transform.position = networkedPosition.Value;
    }

    // This function reads the player's movement input (WASD / arrow keys)
    void HandleInput()
    {
        // Get left/right input (A/D or arrow keys)
        input.x = Input.GetAxis("Horizontal");


        // Get up/down input (W/S or arrow keys)
        input.y = Input.GetAxis("Vertical");

        // If any input was pressed
        if (input != Vector2.zero)
        {
            // Send this input to the server for processing
            MoveServerRpc(input);
        }
    }

    // This function is called by clients and runs on the server
    // It moves the character on the server, which updates everyone’s game
    [ServerRpc]   // Attribute to mark this method as a ServerRpc
    void MoveServerRpc(Vector2 moveInput)
    {
        // Convert the 2D input to a 3D movement vector
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.deltaTime;
        Debug.Log($"IsOwner: {IsOwner}, IsServer: {IsServer}, IsClient: {IsClient}");
        // Add the movement to the player's current position (server-side)
        networkedPosition.Value += move;
    }
}
