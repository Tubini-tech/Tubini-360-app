using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class VRBackButton : MonoBehaviour
{
    private bool wasPressed = false;

    void Update()
    {
        InputDevice leftHand =
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        bool pressed;

        if (leftHand.TryGetFeatureValue(CommonUsages.menuButton, out pressed))
        {
            if (pressed && !wasPressed)
            {
                GoBack();
            }

            wasPressed = pressed;
        }
    }

    void GoBack()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
