using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class ControladorMenuPrincipal : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    public string nombreEscenaJuego = "Principal";

    [Header("Jugador VR (Menú)")]
    public GameObject xrOriginMenu; 

    [Header("Paneles de la Interfaz")]
    public GameObject panelPrincipal;
    public GameObject panelCreditos;

    void Start()
    {
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
        if (panelCreditos != null) panelCreditos.SetActive(false);
    }

    public void IniciarJuego()
    {
        Debug.Log("Cargando la escena del juego...");

        // 1. Apagar el XR Origin del menú
        if (xrOriginMenu != null)
        {
            xrOriginMenu.SetActive(false);
        }

        // 2. Desactivar el XRInteractionManager del menú para que no intercepte los objetos de la escena Principal
        XRInteractionManager managerMenu = FindFirstObjectByType<XRInteractionManager>();
        if (managerMenu != null)
        {
            managerMenu.enabled = false;
        }

        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void MostrarCreditos()
    {
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(true);
    }

    public void OcultarCreditos()
    {
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
    }

    public void SalirJuego()
    {
        Debug.Log("Cerrando la aplicación...");
        Application.Quit();
    }
}