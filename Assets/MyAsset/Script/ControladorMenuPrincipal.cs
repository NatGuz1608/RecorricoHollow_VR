using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Manages main menu UI navigation, scene transitions into the primary VR experience,
/// and application termination.
/// </summary>
public class ControladorMenuPrincipal : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    /// <summary>
    /// Name of the main gameplay scene to load when starting the experience.
    /// </summary>
    public string nombreEscenaJuego = "Principal";

    [Header("Jugador VR (Menú)")]
    /// <summary>
    /// Root GameObject of the XR Origin rig used in the menu scene.
    /// </summary>
    public GameObject xrOriginMenu;

    [Header("Paneles de la Interfaz")]
    /// <summary>
    /// Main menu UI panel containing primary navigation buttons.
    /// </summary>
    public GameObject panelPrincipal;

    /// <summary>
    /// UI panel displaying project credits and attributions.
    /// </summary>
    public GameObject panelCreditos;

    /// <summary>
    /// Initializes the menu interface by activating the main panel and hiding the credits panel.
    /// </summary>
    void Start()
    {
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
        if (panelCreditos != null) panelCreditos.SetActive(false);
    }

    /// <summary>
    /// Deactivates menu-specific XR components to prevent cross-scene conflicts and loads the gameplay scene.
    /// </summary>
    public void IniciarJuego()
    {
        Debug.Log("Cargando la escena del juego...");

        // 1. Disable the menu scene's XR Origin
        if (xrOriginMenu != null)
        {
            xrOriginMenu.SetActive(false);
        }

        // 2. Disable the menu's XRInteractionManager so it does not intercept objects in the target scene
        XRInteractionManager managerMenu = FindFirstObjectByType<XRInteractionManager>();
        if (managerMenu != null)
        {
            managerMenu.enabled = false;
        }

        SceneManager.LoadScene(nombreEscenaJuego);
    }

    /// <summary>
    /// Switches the active UI view from the main menu panel to the credits panel.
    /// </summary>
    public void MostrarCreditos()
    {
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(true);
    }

    /// <summary>
    /// Switches the active UI view from the credits panel back to the main menu panel.
    /// </summary>
    public void OcultarCreditos()
    {
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
    }

    /// <summary>
    /// Exits and closes the running application.
    /// </summary>
    public void SalirJuego()
    {
        Debug.Log("Cerrando la aplicación...");
        Application.Quit();
    }
}