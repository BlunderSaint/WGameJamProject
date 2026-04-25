using Unity.Cinemachine;
using UnityEngine;

public class CharacterSwitchManager : MonoBehaviour
{
    public GameObject[] characters;
    public CinemachineCamera virtualCamera;

    private int currentIndex = 0;

    void Start()
    {
        ActivateCharacter(currentIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchCharacter();
        }
    }

    void SwitchCharacter()
    {
        currentIndex++;

        if (currentIndex >= characters.Length)
            currentIndex = 0;

        ActivateCharacter(currentIndex);
    }

    void ActivateCharacter(int index)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            bool isActive = (i == index);
            characters[i].GetComponent<PlayerMovement>().enabled = isActive;
        }

        // 🎥 Camera follows new character
        virtualCamera.Follow = characters[index].transform;
        virtualCamera.LookAt = characters[index].transform;

        // 🔥 FORCE INSTANT SNAP (IMPORTANT)
        virtualCamera.ForceCameraPosition(
            characters[index].transform.position,
            Quaternion.identity
        );
    }
}