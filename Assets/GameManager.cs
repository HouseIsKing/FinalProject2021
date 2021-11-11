using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    LivingAI[] aiAlive;
    bool[] teamGotWeapon;
    public GameObject textObject;
    Text text;
    float timer;
    // Start is called before the first frame update
    void Start()
    {
        aiAlive = FindObjectsOfType<LivingAI>();
        teamGotWeapon = new bool[]{ false,false};
        text = textObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        aiAlive = FindObjectsOfType<LivingAI>();

        bool won = true;
        if (timer > 0)
            timer -= Time.deltaTime;
        foreach(LivingAI livingAI in aiAlive)
        {
            if (livingAI.Health <= 0)
            {
                text.enabled = true;
                timer = 5f;
                text.text = "A " + (livingAI.team == 0 ? "Friendly has died" : "Enemy has died");
            }
            if(livingAI.HasWeapon && !teamGotWeapon[livingAI.team])
            {
                teamGotWeapon[livingAI.team] = true;
                text.enabled = true;
                if(livingAI.team == 1)
                {
                    text.text = "Enemy team found a weapon!";
                }
                timer = 5f;
            }
            if(livingAI.team == 1)
            {
                won = false;
            }
        }
        if(timer < 0)
        {
            text.enabled = false;
        }
        if(won)
        {
            SceneManager.LoadScene(2);
        }
    }
}
