using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;


public struct CardData
{
    public string type;
}

public struct ElementData
{
    public string type;
    public string question;
    public bool collected;
}

public struct CommunicationData
{
    public string type;
}

public class PlayerManager : NetworkBehaviour
{
    public GameObject Card;
    public GameObject Panel_upper;
    public GameObject Panel_lower;
    public GameObject Feedback;
    public GameObject Element;
    //public List<Cards> activeCardData = new List<Cards>();
    public List<GameObject> activeCards = new List<GameObject>();
    public bool collectElement;

    public SyncList<CardData> activeCardData= new SyncList<CardData>();
    //public SyncList<CardData> cardData = new SyncList<CardData>();
    //public SyncList<CommunicationData> communications = new SyncList<CommunicationData>();

    private SimpleNetworkManager nm;
    private SimpleNetworkManager NM
    {
        get
        {
            if (nm != null) { return nm; }
            return nm = NetworkManager.singleton as SimpleNetworkManager;
        }
    }

    public SyncList<ElementData> elements = new SyncList<ElementData>();

    public override void OnStartServer()
    {
        base.OnStartServer();
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
                elements.Add(new ElementData { type = row[1], question = row[2], collected = false });
            }
        }
    }

    [Command]
    public void CmdDrawCardData()
    {
        if (NM.cardData == null || NM.cardData.Count == 0)
        {
            Debug.Log("No cards left");
        }

        Cards drawCard;
        drawCard = NM.cardData[UnityEngine.Random.Range(0, NM.cardData.Count - 1)];
        activeCardData.Add(new CardData { type = drawCard.GetCardType() });
        //activeCardData.Add(drawCard);
        //activeCardData.Sort();
        NM.cardData.Remove(drawCard);
        RpcDrawACard(drawCard.GetCardType());
        EndTurn();
    }

    [ClientRpc]
    public void RpcDrawACard(string type)
    {
        Panel_lower = GameObject.Find("Panel_lower");
        GameObject card = Instantiate(Card, new Vector3(0, 0, 0), Quaternion.identity);
        if (hasAuthority)
        {
            card.GetComponent<Transform>().SetParent(Panel_lower.transform, false);
            card.transform.GetChild(0).GetComponent<TMP_Text>().text = type;
            activeCards.Add(card);
        } else
        {
            Destroy(card);
        }
    }

    [Command]
    public void CmdCollectElements(string cardType)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].type == cardType)
            {
                string question = elements[i].question;
                elements.Remove(elements[i]);
                elements.Add(new ElementData { type = cardType, question = question, collected = true });
            }
        }
        foreach (CardData cards in activeCardData)
        {
            if (cards.type == cardType)
            {
                activeCardData.Remove(cards);
            }
        }
        RpcDestroyCard(cardType);
        Debug.Log("You collected the element");
        CmdGiveFeedback("You collected " + cardType + " element.");
    }

    [ClientRpc]
    public void RpcDestroyCard(string cardType)
    {
        if (hasAuthority)
        {
            //remove gameobject of the card
            foreach (GameObject active in activeCards.ToArray())
            {
                if (active.transform.GetChild(0).GetComponent<TMP_Text>().text == cardType)
                {
                    activeCards.Remove(active);
                    Destroy(active);
                }
            }
        }

    }

    //give the currentplayer feedback on whether the element has been collected or the player does not have enough cards
    public bool NotEnoughCards(string cardType)
    {
        int count = 0;
        foreach (CardData cards in activeCardData)
        {
            if (cards.type == cardType)
            {
                count++;
            }
        }

        //if there're enough element cards
        if (count >= 4)
        {
            Debug.Log("have enough cards");
            return true;
        }
        else
        {
            Debug.Log("don't have enough cards");
            CmdGiveFeedback("You don't have enough " + cardType + " cards.");
            return false;
        }
    }

    //only check when an element is going to get collected
    //[Command]
    public bool CheckAElement(string cardType)
    {
        foreach (ElementData element in elements)
        {
            if (!element.collected)
            {
                if (element.type == cardType)
                {
                    Debug.Log("element can be collected");
                    return true;
                }
            }
            collectElement = false;
            Debug.Log("element is already collected");
            CmdGiveFeedback("You have already collected " + cardType + " cards.");
        }
        return false;
    }

    //the function that shows the feedback
    [Command]
    public void CmdGiveFeedback(string giveFeedback)
    {
        Feedback = GameObject.Find("Feedback");
        if (hasAuthority)
        {
            Feedback.transform.GetComponent<TMP_Text>().text = giveFeedback;
        }
    }

    public void EndTurn()
    {
        //RpcClearCards();
        //SetUpCards();
    }

    //clear the cards in the playarea
    [ClientRpc]
    public void RpcClearCards()
    {
        if (hasAuthority)
        {
            foreach (GameObject card in activeCards)
            {
                Destroy(card);
            }
            activeCards.Clear();
        }
    }

    //set up cards for the new round
    public void SetUpCards()
    {
        if (activeCardData.Count > 0)
        {
            foreach (CardData drawCard in activeCardData)
            {
                //GameObject card = Instantiate(Card, new Vector3(0, 0, 0), Quaternion.identity);
                //NetworkServer.Spawn(card);
                RpcDrawACard(drawCard.type);
            }
        }
    }
}
