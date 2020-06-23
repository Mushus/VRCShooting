using UnityEngine;
using UdonSharp;
using UnityEngine.UI;

public class StartButton : UdonSharpBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }

    public override void Interact()
    {
        gameObject.SetActive(false);
        // TODO: 検知されてるユーザーを BattleField 上に飛ばす処理
        // GameObject.FindGameObjectsWithTag("Respawn");
    }
}