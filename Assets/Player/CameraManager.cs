using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera astronautCam;
    [SerializeField] private CinemachineCamera rocketCam;

    private void Awake()
    {
        if (astronautCam.Follow == null)
        {
            Debug.LogError("Astronaut Camera is missing a target. Please assign the astronaut GameObject to the camera's target.");
        }
        if (rocketCam.Follow == null)
        {
            Debug.LogError("Rocket Camera is missing a target. Please assign the rocket GameObject to the camera's target.");
        }
    }

    public void SwitchToRocketCam()
    {
        rocketCam.Priority = 10;
        astronautCam.Priority = 5;
    }

    public void SwitchToAstronautCam()
    {
        rocketCam.Priority = 5;
        astronautCam.Priority = 10;
    }
}
