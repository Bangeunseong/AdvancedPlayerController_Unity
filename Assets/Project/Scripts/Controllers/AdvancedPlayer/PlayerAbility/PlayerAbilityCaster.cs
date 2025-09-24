using Project.Scripts.Controllers.AdvancedPlayer.PlayerAbility;
using UnityEngine;

namespace AdvancedPlayer.Project.Scripts.Controllers.AdvancedPlayer.PlayerAbility
{
    public class PlayerAbilityCaster : MonoBehaviour
    {
        public Ability[] hotbar;

        void Update() {
            for (int i = 0; i < hotbar.Length; i++) {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                    Cast(hotbar[i], FindFirstObjectByType<AiEnemy.Enemy>());
                }
            }
        }

        void Cast(Ability ability, IDamagable target) {
            ability.Execute(target);

            var targetMb = target as MonoBehaviour;

            if (ability.castVfx && targetMb) {
                Instantiate(ability.castVfx, targetMb.transform.position + Vector3.up * 2, Quaternion.identity);
            }

            if (ability.runningVfx && targetMb) {
                var runningVfxInstance = Instantiate(ability.runningVfx, targetMb.transform);
                Destroy(runningVfxInstance, 3f);
            }

            if (ability.castSfx) {
                AudioSource.PlayClipAtPoint(ability.castSfx, transform.position);
            }
        }
    }
}