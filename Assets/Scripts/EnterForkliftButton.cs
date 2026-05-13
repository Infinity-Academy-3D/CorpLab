using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class EnterForkliftButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ForkliftController forkliftController;
    [SerializeField] private Transform assentoPiloto;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private GameObject enterButtonObject;

    private LocomotionProvider[] locomotionProviders;
    private CharacterController characterController;
    private Rigidbody playerRigidbody;

    void Start()
    {
        if (xrOrigin == null)
        {
            var origin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (origin != null) xrOrigin = origin.transform;
        }

        if (xrOrigin != null)
        {
            locomotionProviders  = xrOrigin.GetComponentsInChildren<LocomotionProvider>(true);
            characterController  = xrOrigin.GetComponentInChildren<CharacterController>();
            playerRigidbody      = xrOrigin.GetComponentInChildren<Rigidbody>();
        }
    }

    public void OnEnterPressed()
    {
        if (forkliftController == null || assentoPiloto == null || xrOrigin == null) return;

        SetPlayerPhysics(false);

        xrOrigin.position = assentoPiloto.position;
        xrOrigin.rotation = assentoPiloto.rotation;
        xrOrigin.SetParent(forkliftController.transform);

        forkliftController.Enter();

        if (enterButtonObject != null)
            enterButtonObject.SetActive(false);
    }

    public void OnExitPressed()
    {
        if (forkliftController == null || xrOrigin == null) return;

        xrOrigin.SetParent(null);

        SetPlayerPhysics(true);

        forkliftController.Exit();

        if (enterButtonObject != null)
            enterButtonObject.SetActive(true);
    }

    void SetPlayerPhysics(bool active)
    {
        foreach (var provider in locomotionProviders)
            if (provider != null) provider.enabled = active;

        if (characterController != null) characterController.enabled = active;
        if (playerRigidbody != null)     playerRigidbody.isKinematic = !active;
    }
}
