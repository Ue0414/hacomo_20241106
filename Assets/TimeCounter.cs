using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeCounter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Text countdownText;  // カウントダウンを表示するためのUIテキスト

    public bool isCountingDown = false;

    //時間を表示するText型の変数
    public Text timeText;

    public Text timeUpText;
 
    public IEnumerator timeCount(int startValue)
    {
        
        isCountingDown = true;  // カウントダウンが進行中であることを示すフラグ

        int currentValue = startValue;

        while (currentValue >= 0)
        {
            countdownText.text = currentValue.ToString("f0");  // カウントダウンの値を表示
            yield return new WaitForSeconds(1);            // 1秒待つ
            currentValue--;                                // カウントダウンを1減らす
        }

        timeUpText.text = "おわり";                 // カウントが0になったら表示
        isCountingDown = false;
        multiMarker multi = GetComponent<multiMarker>();
        multi.CurrentState = multiMarker.PlayState.Finish;
 
    }

}
