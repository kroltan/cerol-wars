using JetBrains.Annotations;
using Mirror;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Kroltan.GrapplingHook.UI
{
    public class Nameplate : NetworkBehaviour
    {
        public string Name
        {
            get => _name;
            private set => CmdSetName(value);
        }

        [Required]
        [SerializeField]
        private RectTransform _root;

        [Required]
        [SerializeField]
        private TMP_Text _text;

        [SyncVar(hook = nameof(OnNameChanged))]
        private string _name;

        public override void OnStartLocalPlayer()
        {
            Name = FindObjectOfType<ConnectionWindow>(true).PlayerName;
        }

        private void Update()
        {
            _root.transform.rotation = Camera.main!.transform.rotation;
        }

        [Command]
        private void CmdSetName(string name)
        {
            _name = string.IsNullOrWhiteSpace(name) ? "Player" : name;
            gameObject.name = _name;
        }

        [UsedImplicitly]
        private void OnNameChanged(string oldValue, string newValue)
        {
            _text.text = newValue;
        }
    }
}