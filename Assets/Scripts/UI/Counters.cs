using Mirror;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Kroltan.GrapplingHook.UI
{
    public class Counters : MonoBehaviour
    {
        [Required]
        [SerializeField]
        private TMP_Text _fps;

        [Required]
        [SerializeField]
        private TMP_Text _speed;

        [Required]
        [SerializeField]
        private TMP_Text _height;

        private Rigidbody _subject;

        private string _fpsFormat;
        private string _speedFormat;
        private string _heightFormat;

        private void Awake()
        {
            _fpsFormat = _fps.text;
            _speedFormat = _speed.text;
            _heightFormat = _height.text;
        }

        private void Update()
        {
            if (!_subject)
            {
                var player = NetworkClient.localPlayer;
                if (player)
                {
                    _subject = player.GetComponent<Rigidbody>();
                    _fps.gameObject.SetActive(true);
                    _speed.gameObject.SetActive(true);
                    _height.gameObject.SetActive(true);
                }
                else
                {
                    _fps.gameObject.SetActive(false);
                    _speed.gameObject.SetActive(false);
                    _height.gameObject.SetActive(false);
                    return;
                }
            }

            var bottom = _subject.transform.position;
            bottom.y = -100;
            bottom = _subject.ClosestPointOnBounds(bottom);

            SetCounterValue(_fps, _fpsFormat, 1 / Time.smoothDeltaTime);
            SetCounterValue(_speed, _speedFormat, _subject.velocity.magnitude);
            SetCounterValue(_height, _heightFormat, bottom.y);
        }

        private void SetCounterValue(TMP_Text counter, string format, float value)
        {
            var record = PlayerPrefs.GetFloat(format, 0);
            if (value >= record && !float.IsInfinity(value))
            {
                PlayerPrefs.SetFloat(format, value);
            }
            
            counter.text = string.Format(format, value, record);
        }
    }
}