using Mirror;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kroltan.GrapplingHook.UI
{
    public class ConnectionWindow : MonoBehaviour
    {
        public string PlayerName => _nameInput.text;

        [Required]
        [SerializeField]
        private TMP_InputField _nameInput;

        [Required]
        [SerializeField]
        private TMP_InputField _ipInput;

        [Required]
        [SerializeField]
        private Button _startHostButton;

        [Required]
        [SerializeField]
        private Button _connectButton;

        public void Start()
        {
            _startHostButton.onClick.AddListener(
                () =>
                {
                    NetworkManager.singleton.StartHost();
                    gameObject.SetActive(false);
                }
            );

            _connectButton.onClick.AddListener(
                () =>
                {
                    NetworkManager.singleton.networkAddress = string.IsNullOrWhiteSpace(_ipInput.text)
                        ? "localhost"
                        : _ipInput.text;
                    NetworkManager.singleton.StartClient();
                    gameObject.SetActive(false);
                }
            );
        }
    }
}