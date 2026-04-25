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

            // Try Mother
            MotherMovement mother = characters[i].GetComponent<MotherMovement>();
            if (mother != null) mother.enabled = isActive;

            // Try Daughter
            DaughterMovement daughter = characters[i].GetComponent<DaughterMovement>();
            if (daughter != null) daughter.enabled = isActive;

            // Disable push when not active
            PlayerPush push = characters[i].GetComponent<PlayerPush>();
            if (push != null) push.enabled = isActive;
        }

        // Camera follows new character
        virtualCamera.Follow = characters[index].transform;
        virtualCamera.LookAt = characters[index].transform;

        // Force instant snap
        virtualCamera.ForceCameraPosition(
            characters[index].transform.position,
            Quaternion.identity
        );
    }
}