using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera astronautCam;
    [SerializeField] private CinemachineCamera rocketCam;

    private void Awake()
    {
        if (astronautCam.Follow == null && FindFirstObjectByType<Astronaut>() != null)
        {
            astronautCam.Follow = FindFirstObjectByType<Astronaut>().transform;
        }
        if (rocketCam.Follow == null && FindFirstObjectByType<Rocket>() != null)
        {
            rocketCam.Follow = FindFirstObjectByType<Rocket>().transform;
        }
        SwitchToRocketCam();
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
