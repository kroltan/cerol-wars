using System;
using Mirror;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Kroltan.GrapplingHook.UI
{
    public class Timer : NetworkBehaviour
    {
        [Required]
        [SerializeField]
        private TMP_Text _label;
        
        [SyncVar]
        private float _startTime;
        
        [SyncVar]
        private float _endTime;

        [SyncVar]
        private bool _running;

        private string _format;

        private void Awake()
        {
            _format = _label.text;
        }

        private void Update()
        {
            if (Input.GetButtonDown("Timer"))
            {
                CmdToggleTimer();
            }

            if (isServer && _running)
            {
                _endTime = Time.time;
            }

            _label.text = string.Format(_format, TimeSpan.FromSeconds(_endTime - _startTime));
        }

        [Command(requiresAuthority = false)]
        private void CmdToggleTimer()
        {
            if (_running)
            {
                _running = false;
            }
            else
            {
                _startTime = Time.time;
                _running = true;
            }
        }
    }
}