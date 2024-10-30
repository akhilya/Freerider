using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public Animator menuAnimator;

    private bool wasTouch;
    private float firstTouchTime;
    private Vector2 firstTouchPos;

    private float lastTouchTime;
    private Vector2 lastTouchPos;

    private bool isMenuOpened = false;
    void Update()
    {
        bool touch = Input.touchCount > 0;
        if (touch) {
            float curTime = Time.timeSinceLevelLoad;
            Vector2 curPos = Input.GetTouch(0).position;

            if (!wasTouch) {
                firstTouchTime = curTime;
                firstTouchPos = curPos;
            }

            lastTouchTime = curTime;
            lastTouchPos = curPos;
        }
        else if (!touch && wasTouch) {
            if (lastTouchTime - firstTouchTime < 1.5f 
                && Vector2.Distance(lastTouchPos, firstTouchPos) > 50f
                && Mathf.Abs(firstTouchPos.x - lastTouchPos.x) < 90f) 
            {
                if (lastTouchPos.y > firstTouchPos.y && !isMenuOpened)
                    OpenMenu();
                else if (lastTouchPos.y < firstTouchPos.y && isMenuOpened)
                    CloseMenu();
            }
        }
        wasTouch = touch;

        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (isMenuOpened)
                CloseMenu();
            else 
                OpenMenu();
        }

        Time.timeScale = Mathf.Lerp(Time.timeScale, isMenuOpened ? 0f : 1f, Time.deltaTime * 8f);
    }

    public void OpenMenu() {
        if (isMenuOpened)
            return;
        menuAnimator.SetTrigger("open");
        isMenuOpened = true;
    }

    public void CloseMenu() {
        if (!isMenuOpened)
            return;
        menuAnimator.SetTrigger("close");
        isMenuOpened = false;
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
