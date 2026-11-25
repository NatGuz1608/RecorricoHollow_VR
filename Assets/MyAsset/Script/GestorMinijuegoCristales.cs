using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the crystal-placement VR mini-game lifecycle, including countdown timer,
/// progress HUD, win/loss screens, and XR system re-initialization after scene transitions.
/// </summary>
public class GestorMinijuegoCristales : MonoBehaviour
{
    [Header("Cristales (Para iniciar el juego)")]
    /// <summary>
    /// First grabbable crystal interactable that can trigger the start of the mini-game.
    /// </summary>
    public XRGrabInteractable cristal1;

    /// <summary>
    /// Second grabbable crystal interactable that can trigger the start of the mini-game.
    /// </summary>
    public XRGrabInteractable cristal2;

    /// <summary>
    /// Third grabbable crystal interactable that can trigger the start of the mini-game.
    /// </summary>
    public XRGrabInteractable cristal3;

    [Header("Zonas de Encaje (Altar)")]
    /// <summary>
    /// First altar XR socket interactor where a crystal must be placed.
    /// </summary>
    public XRSocketInteractor socket1;

    /// <summary>
    /// Second altar XR socket interactor where a crystal must be placed.
    /// </summary>
    public XRSocketInteractor socket2;

    /// <summary>
    /// Third altar XR socket interactor where a crystal must be placed.
    /// </summary>
    public XRSocketInteractor socket3;

    [Header("Interfaz de Usuario (Canvas)")]
    /// <summary>
    /// TextMeshPro UI element displaying the remaining countdown timer.
    /// </summary>
    public TextMeshProUGUI textoTemporizador;

    /// <summary>
    /// TextMeshPro UI element displaying the current number of socketed crystals.
    /// </summary>
    public TextMeshProUGUI textoContador;

    /// <summary>
    /// UI panel displayed when the player places all three crystals before time runs out.
    /// </summary>
    public GameObject pantallaVictoria;

    /// <summary>
    /// UI panel displayed when the countdown timer reaches zero before completion.
    /// </summary>
    public GameObject pantallaDerrota;

    [Header("Configuración del Juego")]
    /// <summary>
    /// Total time limit for completing the mini-game, expressed in minutes.
    /// </summary>
    public float tiempoMaximoEnMinutos = 5f;

    /// <summary>
    /// Duration in seconds before automatically hiding the victory or defeat panel.
    /// </summary>
    public float tiempoOcultarPaneles = 30f;

    /// <summary>
    /// Remaining countdown time in seconds for the current session.
    /// </summary>
    private float tiempoRestante;

    /// <summary>
    /// Indicates whether the mini-game timer is currently active and counting down.
    /// </summary>
    private bool juegoActivo = false;

    /// <summary>
    /// Tracks whether the mini-game has already been triggered by a player interaction.
    /// </summary>
    private bool minijuegoIniciado = false;

    /// <summary>
    /// Current count of crystals placed inside the altar sockets.
    /// </summary>
    private int cristalesColocados = 0;

    /// <summary>
    /// Initializes timer values, hides HUD and end-game panels, and triggers XR system synchronization.
    /// </summary>
    void Start()
    {
        tiempoRestante = tiempoMaximoEnMinutos * 60f;
        cristalesColocados = 0;

        juegoActivo = false;
        minijuegoIniciado = false;

        if (pantallaVictoria != null) pantallaVictoria.SetActive(false);
        if (pantallaDerrota != null) pantallaDerrota.SetActive(false);

        if (textoTemporizador != null) textoTemporizador.gameObject.SetActive(false);
        if (textoContador != null) textoContador.gameObject.SetActive(false);

        ActualizarTextosUI();

        // Re-enable XR systems one frame after the previous scene finishes unloading
        StartCoroutine(ReactivarSistemaXREnEscena());
    }

    /// <summary>
    /// Waits one frame after scene load to re-enable input action assets, XR managers,
    /// interactors, interactables, and the XR Device Simulator.
    /// </summary>
    /// <returns>An enumerator used by Unity's coroutine scheduler.</returns>
    private IEnumerator ReactivarSistemaXREnEscena()
    {
        // Wait 1 frame so OnDisable() from the menu scene completes entirely
        yield return null;

        // 1. Force-enable all InputActionAssets in memory (XRI Default Input Actions and Simulator)
        InputActionAsset[] actionAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
        foreach (var asset in actionAssets)
        {
            if (asset != null)
            {
                asset.Enable();
            }
        }

        // 2. Re-enable any InputActionManager present in the main scene
        InputActionManager[] inputManagers = FindObjectsByType<InputActionManager>(FindObjectsSortMode.None);
        foreach (var im in inputManagers)
        {
            im.enabled = false;
            im.enabled = true;
            im.EnableInput();
        }

        // 3. Retrieve and enable the XRInteractionManager belonging to the main scene
        XRInteractionManager managerPrincipal = FindFirstObjectByType<XRInteractionManager>();
        if (managerPrincipal != null)
        {
            managerPrincipal.enabled = true;
        }

        // 4. Cycle the main scene's XR Origin to refresh XRInputModalityManager and controllers
        if (Camera.main != null)
        {
            GameObject xrOriginRoot = Camera.main.transform.root.gameObject;
            xrOriginRoot.SetActive(false);
            xrOriginRoot.SetActive(true);
        }

        // 5. Rebind all Interactors (Near-Far, Poke, Teleport, Sockets) to the main XRInteractionManager
        XRBaseInteractor[] interactors = FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None);
        foreach (var interactor in interactors)
        {
            if (managerPrincipal != null)
            {
                interactor.interactionManager = managerPrincipal;
            }
            interactor.enabled = false;
            interactor.enabled = true;
        }

        // 6. Rebind all Interactables (Crystals, Lecterns, etc.) to the main XRInteractionManager
        XRBaseInteractable[] interactables = FindObjectsByType<XRBaseInteractable>(FindObjectsSortMode.None);
        foreach (var interactable in interactables)
        {
            if (managerPrincipal != null)
            {
                interactable.interactionManager = managerPrincipal;
            }
            interactable.enabled = false;
            interactable.enabled = true;
        }

        // 7. Refresh the XR Device Simulator in DontDestroyOnLoad to restore keyboard/mouse input
        MonoBehaviour[] todosLosScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var script in todosLosScripts)
        {
            if (script != null && script.GetType().Name.Contains("Simulator"))
            {
                script.enabled = false;
                script.enabled = true;
            }
        }

        Debug.Log("Sistema XR en escena Principal sincronizado y activo.");
    }

    /// <summary>
    /// Subscribes to grab and socket interaction events for all crystals and altar sockets.
    /// </summary>
    void OnEnable()
    {
        if (cristal1 != null) cristal1.selectEntered.AddListener(AlCristalAgarrado);
        if (cristal2 != null) cristal2.selectEntered.AddListener(AlCristalAgarrado);
        if (cristal3 != null) cristal3.selectEntered.AddListener(AlCristalAgarrado);

        socket1.selectEntered.AddListener(AlCristalColocado);
        socket2.selectEntered.AddListener(AlCristalColocado);
        socket3.selectEntered.AddListener(AlCristalColocado);

        socket1.selectExited.AddListener(AlCristalRetirado);
        socket2.selectExited.AddListener(AlCristalRetirado);
        socket3.selectExited.AddListener(AlCristalRetirado);
    }

    /// <summary>
    /// Unsubscribes from crystal and socket interaction events when disabled.
    /// </summary>
    void OnDisable()
    {
        if (cristal1 != null) cristal1.selectEntered.RemoveListener(AlCristalAgarrado);
        if (cristal2 != null) cristal2.selectEntered.RemoveListener(AlCristalAgarrado);
        if (cristal3 != null) cristal3.selectEntered.RemoveListener(AlCristalAgarrado);

        socket1.selectEntered.RemoveListener(AlCristalColocado);
        socket2.selectEntered.RemoveListener(AlCristalColocado);
        socket3.selectEntered.RemoveListener(AlCristalColocado);

        socket1.selectExited.RemoveListener(AlCristalRetirado);
        socket2.selectExited.RemoveListener(AlCristalRetirado);
        socket3.selectExited.RemoveListener(AlCristalRetirado);
    }

    /// <summary>
    /// Updates the countdown timer every frame while the mini-game is active and triggers defeat if time expires.
    /// </summary>
    void Update()
    {
        if (!juegoActivo) return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0)
        {
            tiempoRestante = 0;
            PerderJuego();
        }

        ActualizarTextosUI();
    }

    /// <summary>
    /// Starts the mini-game countdown and activates HUD elements on the player's first crystal interaction.
    /// </summary>
    private void IniciarJuego()
    {
        if (!minijuegoIniciado)
        {
            minijuegoIniciado = true;
            juegoActivo = true;

            if (textoTemporizador != null) textoTemporizador.gameObject.SetActive(true);
            if (textoContador != null) textoContador.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Invoked when any crystal is grabbed, ensuring the mini-game session starts.
    /// </summary>
    /// <param name="args">Event data associated with grabbing a crystal.</param>
    private void AlCristalAgarrado(SelectEnterEventArgs args)
    {
        IniciarJuego();
    }

    /// <summary>
    /// Invoked when a crystal is inserted into an altar socket, updating progress and checking for victory.
    /// </summary>
    /// <param name="args">Event data associated with inserting a crystal into a socket.</param>
    private void AlCristalColocado(SelectEnterEventArgs args)
    {
        IniciarJuego();

        cristalesColocados++;
        ActualizarTextosUI();

        if (cristalesColocados >= 3 && tiempoRestante > 0)
        {
            GanarJuego();
        }
    }

    /// <summary>
    /// Invoked when a crystal is removed from an altar socket, decrementing the placed crystal count.
    /// </summary>
    /// <param name="args">Event data associated with removing a crystal from a socket.</param>
    private void AlCristalRetirado(SelectExitEventArgs args)
    {
        cristalesColocados--;
        ActualizarTextosUI();
    }

    /// <summary>
    /// Updates the crystal counter and formatted MM:SS countdown timer on the UI canvas.
    /// </summary>
    private void ActualizarTextosUI()
    {
        if (textoContador != null)
            textoContador.text = $"Cristales: {cristalesColocados} / 3";

        if (textoTemporizador != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTemporizador.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    /// <summary>
    /// Ends the active session in a win state, hides the HUD, and temporarily displays the victory screen.
    /// </summary>
    private void GanarJuego()
    {
        juegoActivo = false;
        OcultarHUD();

        if (pantallaVictoria != null)
        {
            pantallaVictoria.SetActive(true);
            StartCoroutine(OcultarPantallaDespuesDeTiempo(pantallaVictoria, tiempoOcultarPaneles));
        }
    }

    /// <summary>
    /// Ends the active session in a loss state, hides the HUD, and temporarily displays the defeat screen.
    /// </summary>
    private void PerderJuego()
    {
        juegoActivo = false;
        OcultarHUD();

        if (pantallaDerrota != null)
        {
            pantallaDerrota.SetActive(true);
            StartCoroutine(OcultarPantallaDespuesDeTiempo(pantallaDerrota, tiempoOcultarPaneles));
        }
    }

    /// <summary>
    /// Deactivates the timer and crystal counter HUD elements.
    /// </summary>
    private void OcultarHUD()
    {
        if (textoTemporizador != null) textoTemporizador.gameObject.SetActive(false);
        if (textoContador != null) textoContador.gameObject.SetActive(false);
    }

    /// <summary>
    /// Waits for a specified duration before deactivating the given end-game UI panel.
    /// </summary>
    /// <param name="pantallaActiva">The victory or defeat panel GameObject to deactivate.</param>
    /// <param name="tiempoEspera">Delay in seconds before hiding the panel.</param>
    /// <returns>An enumerator used by Unity's coroutine scheduler.</returns>
    private IEnumerator OcultarPantallaDespuesDeTiempo(GameObject pantallaActiva, float tiempoEspera)
    {
        yield return new WaitForSeconds(tiempoEspera);

        if (pantallaActiva != null)
        {
            pantallaActiva.SetActive(false);
        }
    }
}
