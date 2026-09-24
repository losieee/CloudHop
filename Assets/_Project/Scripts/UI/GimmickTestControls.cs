using UnityEngine;
using UnityEngine.UI;

namespace CloudHop
{
    public sealed class GimmickTestControls : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private StageMechanics[] courses;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Text label;
        private bool enabledMechanics = true;
        private void OnEnable() => toggleButton.onClick.AddListener(Toggle);
        private void OnDisable() => toggleButton.onClick.RemoveListener(Toggle);
        private void Toggle()
        {
            enabledMechanics = !enabledMechanics;
            foreach (var course in courses) course.SetEnabled(enabledMechanics);
            label.text = enabledMechanics ? "GIMMICKS: ON" : "GIMMICKS: OFF";
            game.Retry();
        }
    }
}
