using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections; // Required for Coroutines

/// <summary>
/// Controls an interactive VR lore lectern that toggles a character description panel,
/// updates button materials, plays spatial voice-over narration, and temporarily pauses
/// ambient background music during playback.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
[RequireComponent(typeof(AudioSource))]
public class AtrilLoreHollowKnight : MonoBehaviour
{
    [Header("Referencias Visuales")]
    /// <summary>
    /// GameObject representing the character lore description panel to toggle.
    /// </summary>
    public GameObject cartelDescripcionPersonaje;

    /// <summary>
    /// MeshRenderer of the interactive lectern button used to display pressed and idle states.
    /// </summary>
    public MeshRenderer renderizadorBotonRojo;

    /// <summary>
    /// Material applied to the lectern button when in its default idle state.
    /// </summary>
    public Material materialBotonReposo;

    /// <summary>
    /// Material applied to the lectern button while it is being pressed.
    /// </summary>
    public Material materialBotonActivado;

    [Header("Feedback Sonoro")]
    /// <summary>
    /// Audio clip containing the character's voice-over narration.
    /// </summary>
    public AudioClip clipVozPersonaje;

    /// <summary>
    /// Sound effect played when the lectern button is pressed.
    /// </summary>
    public AudioClip sfxClickBoton;

    /// <summary>
    /// Reference to the scene's ambient background music <see cref="AudioSource"/>.
    /// </summary>
    public AudioSource musicaAmbiente;

    /// <summary>
    /// Cached reference to the <see cref="UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable"/> component.
    /// </summary>
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    /// <summary>
    /// Cached local <see cref="AudioSource"/> configured for 3D spatial audio playback.
    /// </summary>
    private AudioSource fuenteAudio;

    /// <summary>
    /// Initializes component references and configures 3D spatial audio parameters.
    /// </summary>
    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        fuenteAudio = GetComponent<AudioSource>();

        fuenteAudio.spatialBlend = 1.0f;
        fuenteAudio.playOnAwake = false;
    }

    /// <summary>
    /// Subscribes to XR selection events when the component is enabled.
    /// </summary>
    void OnEnable()
    {
        interactable.selectEntered.AddListener(AlPresionarBoton);
        interactable.selectExited.AddListener(AlSoltarBoton);
    }

    /// <summary>
    /// Unsubscribes from XR selection events when the component is disabled.
    /// </summary>
    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(AlPresionarBoton);
        interactable.selectExited.RemoveListener(AlSoltarBoton);
    }

    /// <summary>
    /// Handles button press interaction by toggling the lore panel, triggering audio feedback,
    /// and managing ambient music state.
    /// </summary>
    /// <param name="args">Event data associated with the XR selection enter event.</param>
    private void AlPresionarBoton(SelectEnterEventArgs args)
    {
        if (renderizadorBotonRojo != null)
            renderizadorBotonRojo.material = materialBotonActivado;

        if (cartelDescripcionPersonaje != null)
        {
            bool nuevoEstado = !cartelDescripcionPersonaje.activeSelf;
            cartelDescripcionPersonaje.SetActive(nuevoEstado);

            fuenteAudio.Stop();
            StopAllCoroutines(); // Stop any active audio wait coroutines

            if (sfxClickBoton != null)
                fuenteAudio.PlayOneShot(sfxClickBoton);

            if (nuevoEstado)
            {
                // Lore panel is turned ON
                if (musicaAmbiente != null)
                {
                    musicaAmbiente.Pause(); // Pause ambient music during narration
                }

                if (clipVozPersonaje != null)
                {
                    fuenteAudio.clip = clipVozPersonaje;
                    fuenteAudio.Play();
                    // Start coroutine to resume ambient music once narration finishes
                    StartCoroutine(EsperarAudioTermine(clipVozPersonaje.length));
                }
            }
            else
            {
                // Lore panel is turned OFF
                if (musicaAmbiente != null && !musicaAmbiente.isPlaying)
                {
                    musicaAmbiente.Play(); // Resume ambient music if the panel is closed early
                }
            }
        }
    }

    /// <summary>
    /// Restores the button's idle material when the XR selection ends.
    /// </summary>
    /// <param name="args">Event data associated with the XR selection exit event.</param>
    private void AlSoltarBoton(SelectExitEventArgs args)
    {
        if (renderizadorBotonRojo != null)
            renderizadorBotonRojo.material = materialBotonReposo;
    }

    /// <summary>
    /// Waits for the voice-over clip duration to elapse before resuming ambient music
    /// if the lore panel remains active.
    /// </summary>
    /// <param name="duracion">Duration in seconds to wait before resuming ambient audio.</param>
    /// <returns>An enumerator used by Unity's coroutine scheduler.</returns>
    private IEnumerator EsperarAudioTermine(float duracion)
    {
        yield return new WaitForSeconds(duracion);

        // Resume ambient music if the lore panel is still visible after narration ends
        if (cartelDescripcionPersonaje.activeSelf && musicaAmbiente != null)
        {
            musicaAmbiente.Play();
        }
    }
}