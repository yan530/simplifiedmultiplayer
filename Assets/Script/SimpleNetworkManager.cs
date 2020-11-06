using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using TMPro;

public class SimpleNetworkManager : NetworkManager
{
    public List<Cards> cardData = new List<Cards>();
    public List<Elements> elements = new List<Elements>();
    public List<Cards> communications = new List<Cards>();


    //public SyncList<CardData> cardData = new SyncList<CardData>();
    //public SyncList<ElementData> elements = new SyncList<ElementData>();
    //public SyncList<CommunicationData> communications = new SyncList<CommunicationData>();

    public bool isWin;

    public override void OnStartServer()
    {
        base.OnStartServer();
        LoadCards();
        isWin = false;
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
                cardData.Add(Cards.CreateInstance(i, row[1], row[2], row[3]));
            }
        }
    }
}
