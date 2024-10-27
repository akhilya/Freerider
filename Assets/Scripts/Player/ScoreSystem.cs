using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSystem : MonoBehaviour
{
    public PlayerController player;

    public TMP_Text totalScoreText;
    public TMP_Text rampageScoreText;
    public Image rampageHaloImage;
    public Animator rampageScoreAnimator;

    private float baseScore;
    private int totalRampageScore;
    private float rampageScore;
    private float rampageScoreMultiplier = 1.0f;
    private float rampageVisualAlpha = 0.0f;

    private float smoothedTotalScore;

    private bool wasGrounded;

    void Update()
    {
        int playerY = Mathf.Abs((int)player.transform.position.y),
            playerZ = Mathf.Abs((int)player.transform.position.z);
        baseScore = (playerY * 2.0f + playerZ) / 10.0f;

        float rampageVisualAlphaTarget = 0.0f;
        if (!player.IsHighGrounded) 
        {
            if (wasGrounded) {
                rampageScoreMultiplier = 1.0f;
                rampageScore = 0;
            }
            rampageScore += Time.deltaTime * 50.0f * rampageScoreMultiplier;
            if (rampageScoreMultiplier < 5.0f)
                rampageScoreMultiplier += Time.deltaTime * 0.5f;

            rampageVisualAlphaTarget = rampageScore > 40.0f ? 1.0f : 0.0f;
            if (rampageScoreAnimator.GetBool("transparent") != false) {
                rampageScoreAnimator.SetTrigger("break");
                rampageScoreAnimator.SetBool("transparent", false);
            }
        }
        else if (player.IsHighGrounded && !wasGrounded) 
        {
            totalRampageScore += Mathf.RoundToInt(rampageScore);
            rampageVisualAlphaTarget = 0.0f;
            if (rampageScoreAnimator.GetBool("transparent") != true) {
                rampageScoreAnimator.SetBool("transparent", true);
            }
            if (rampageScore > 40.0f) {
                rampageScoreAnimator.SetTrigger("add");
            }
            //rampageVisualAlpha = 0.0f;
        }
        wasGrounded = player.IsHighGrounded;

        float totalScore = baseScore + totalRampageScore;
        smoothedTotalScore = Mathf.Lerp(smoothedTotalScore, totalScore, Time.deltaTime * 5.0f);
        
        totalScoreText.text = Mathf.RoundToInt(smoothedTotalScore).ToString();
        rampageScoreText.text = Mathf.RoundToInt(rampageScore).ToString();

        rampageVisualAlpha = Mathf.Lerp(rampageVisualAlpha, rampageVisualAlphaTarget, Time.deltaTime * 5.0f);
        rampageScoreText.color = SetAlpha(rampageScoreText.color, rampageVisualAlpha);
        rampageHaloImage.color = SetAlpha(rampageHaloImage.color, rampageVisualAlpha);
    }

    Color SetAlpha(Color c, float alpha)
    {
        c.a = alpha;
        return c;
    }
}
