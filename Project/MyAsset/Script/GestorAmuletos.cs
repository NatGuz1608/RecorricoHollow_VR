using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Tracks the placement of three charms into designated XR socket interactors
/// and triggers visual and audio feedback once all three slots are filled.
/// </summary>
public class GestorAmuletos : MonoBehaviour
{
    [Header("Zonas de Encaje")]
    /// <summary>
    /// First XR socket interactor slot.
    /// </summary>
    public XRSocketInteractor socket1;

    /// <summary>
    /// Second XR socket interactor slot.
    /// </summary>
    public XRSocketInteractor socket2;

    /// <summary>
    /// Third XR socket interactor slot.
    /// </summary>
    public XRSocketInteractor socket3;

    [Header("Recompensa / Feedback")]
    /// <summary>
    /// Special reward text GameObject revealed when all three charms are placed.
    /// </summary>
    public GameObject textoEspecial;

    /// <summary>
    /// Optional sound effect played when all three sockets are occupied.
    /// </summary>
    public AudioClip sfxCompletado;

    /// <summary>
    /// Current number of items placed inside the sockets.
    /// </summary>
    private int elementosColocados = 0;

    /// <summary>
    /// Dynamically attached <see cref="AudioSource"/> used to play completion audio feedback.
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// Initializes the local audio source and ensures the reward text starts hidden.
    /// </summary>
    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        if (textoEspecial != null)
        {
            textoEspecial.SetActive(false); // Hide reward text at scene startup
        }
    }

    /// <summary>
    /// Subscribes to select enter and exit events across all three XR sockets.
    /// </summary>
    void OnEnable()
    {
        // Subscribe to object insertion and removal events
        socket1.selectEntered.AddListener(AlObjetoColocado);
        socket2.selectEntered.AddListener(AlObjetoColocado);
        socket3.selectEntered.AddListener(AlObjetoColocado);

        socket1.selectExited.AddListener(AlObjetoRetirado);
        socket2.selectExited.AddListener(AlObjetoRetirado);
        socket3.selectExited.AddListener(AlObjetoRetirado);
    }

    /// <summary>
    /// Unsubscribes from socket events when disabled to prevent memory leaks.
    /// </summary>
    void OnDisable()
    {
        // Clean up event subscriptions to prevent memory leaks
        socket1.selectEntered.RemoveListener(AlObjetoColocado);
        socket2.selectEntered.RemoveListener(AlObjetoColocado);
        socket3.selectEntered.RemoveListener(AlObjetoColocado);

        socket1.selectExited.RemoveListener(AlObjetoRetirado);
        socket2.selectExited.RemoveListener(AlObjetoRetirado);
        socket3.selectExited.RemoveListener(AlObjetoRetirado);
    }

    /// <summary>
    /// Increments the placed item counter and checks whether the puzzle completion condition is met.
    /// </summary>
    /// <param name="args">Event data associated with inserting an interactable into a socket.</param>
    private void AlObjetoColocado(SelectEnterEventArgs args)
    {
        elementosColocados++;
        VerificarCompletado();
    }

    /// <summary>
    /// Decrements the placed item counter and hides the reward text if an item is removed.
    /// </summary>
    /// <param name="args">Event data associated with removing an interactable from a socket.</param>
    private void AlObjetoRetirado(SelectExitEventArgs args)
    {
        elementosColocados--;
        if (textoEspecial != null)
        {
            textoEspecial.SetActive(false); // Hide reward text if the player removes an item
        }
    }

    /// <summary>
    /// Evaluates if all three sockets are filled and activates the completion text and audio cue.
    /// </summary>
    private void VerificarCompletado()
    {
        if (elementosColocados >= 3)
        {
            if (textoEspecial != null) textoEspecial.SetActive(true);

            if (sfxCompletado != null && audioSource != null)
            {
                audioSource.PlayOneShot(sfxCompletado);
            }
        }
    }
}