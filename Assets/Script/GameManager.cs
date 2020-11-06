using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public List<Cards> cards = new List<Cards>();
    public List<Elements> elements = new List<Elements>();
    public List<Cards> communications = new List<Cards>();


    // Start is called before the first frame update
    void Start()
    {
        LoadCards();
    }

    //load data from csv file
    private void LoadCards()
    {
        AddData("Card_data", "cards");
        AddData("Element_data", "elements");
    }

    //load data and concatinate it into appropriate form
    private void AddData(string resource, string type)
    {
        TextAsset elementData = Resources.Load<TextAsset>(resource);

        string[] data = elementData.text.Split(new char[] { '\n' });

        for (int i = 1; i < data.Length; i++)
        {
            string[] row = data[i].Split(new char[] { ',' });

            if (type == "elements")
            {
                elements.Add(Elements.CreateInstance(i, row[1], row[2], false));
            }
            else
            {
                cards.Add(Cards.CreateInstance(i, row[1], row[2], row[3]));
            }
        }
    }

    public Cards DrawCardData()
    {
        if (cards == null || cards.Count == 0)
        {
            Debug.Log("No cards left");
        }

        //check if an card has been exchanged and moved to the top of the deck
        Cards drawCard;
        drawCard = cards[UnityEngine.Random.Range(0, cards.Count - 1)];
        cards.Remove(drawCard);
        return drawCard;
    }
}
